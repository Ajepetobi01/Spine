using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Spine.Common.Helper;

namespace Spine.Common.Extensions
{
    public static class EnumExtensions
    {
        private static readonly Dictionary<Enum, DescriptionAttribute> EnumDescriptions;
        private static readonly HashSet<Enum> ExcludedEnumsFromDescriptions;

        static EnumExtensions()
        {
            var (Descriptions, ExcludedEnums) = EnumExtensionHelper.GetEnumDescriptions();
            EnumDescriptions = Descriptions;
            ExcludedEnumsFromDescriptions = ExcludedEnums;
        }

        public static class EnumConverter<TEnum> where TEnum : struct, IConvertible
        {
            public static readonly Func<int, TEnum> FromInt32 = GenerateMethod<int, TEnum>();
            public static readonly Func<TEnum, int> ToInt32 = GenerateMethod<TEnum, int>();
            public static readonly Func<TEnum, long> ToLong = GenerateMethod<TEnum, long>();

            private static Func<TFrom, TTo> GenerateMethod<TFrom, TTo>()
            {
                var parameter = Expression.Parameter(typeof(TFrom));
                var convert = Expression.Convert(parameter, typeof(TTo));
                return Expression.Lambda<Func<TFrom, TTo>>(convert, parameter).Compile();
            }
        }

        [DebuggerStepThrough]
        public static string GetDescription<TEnum>(this int value) where TEnum : struct, Enum
        {
            return EnumConverter<TEnum>.FromInt32(value).GetDescription();
        }

        [DebuggerStepThrough]
        public static string GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            return value.GetMetadata().Description;
        }

        [DebuggerStepThrough]
        public static string GetNullableDescription<TEnum>(this TEnum value) where TEnum : Enum
        {
            return value?.GetMetadata().Description;
        }

        private static DescriptionAttribute GetMetadata<TEnum>(this TEnum value) where TEnum : Enum
        {
            if (!EnumDescriptions.TryGetValue(value, out var metadata))
            {
                var description = value.ToString();
                metadata = new DescriptionAttribute(description != "0" ? description : "");

                //If the description is empty or None then don't store it in the enum descriptions list. 
                //This is because doing so would bypass the ExcludeEnumValue attribute and cause it to be included in usages of GetValues.
                //There is no reason for None to have not have been in the EnumDescriptions dictionary already if it did not have the ExcludeEnumValue attribute.
                if (description != "" && description != "None" && !ExcludedEnumsFromDescriptions.Contains(value))
                {
                    EnumDescriptions[value] = metadata;
                }
            }

            return metadata;
        }

        public static IEnumerable<string> GetDescriptionsAsText<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            return GetFlags(value).Select(GetDescription);
        }

        public static IEnumerable<EnumValueModel> GetValues<TEnum>() where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            var enumType = typeof(TEnum);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var isInt = underlyingType == typeof(int);
            var values = Enum.GetValues(enumType).Cast<TEnum>();
            foreach (var value in values)
            {
                var enumValue = value as Enum;
                if (enumValue != null)
                {
                    if (EnumDescriptions.TryGetValue(enumValue, out var metadata))
                    {
                        if (isInt)
                        {
                            yield return new EnumValueModel
                            {
                                Name = metadata.Description,
                                Value = EnumConverter<TEnum>.ToInt32(value)
                            };
                        }
                        else
                        {
                            yield return new EnumValueModel
                            {
                                Name = metadata.Description,
                                Value = EnumConverter<TEnum>.ToLong(value).ToString()
                            };
                        }
                    }
                }
            }
        }

        //For system defined Enums, since EnumDescriptions has mappings for created Enums
        public static IEnumerable<EnumValueModel> GetSystemValues<TEnum>() where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var underlyingType = Enum.GetUnderlyingType(enumType);
            var isInt = underlyingType == typeof(int);
            var values = Enum.GetValues(enumType).Cast<TEnum>();
            foreach (var value in values)
            {
                if (value is Enum enumValue)
                {
                    if (isInt)
                    {
                        yield return new EnumValueModel
                        {
                            Name = enumValue.ToString(),
                            Value = EnumConverter<TEnum>.ToInt32(value)
                        };
                    }
                    else
                    {
                        yield return new EnumValueModel
                        {
                            Name = enumValue.ToString(),
                            Value = EnumConverter<TEnum>.ToLong(value).ToString()
                        };
                    }
                }
            }
        }

        public class EnumValueModel
        {
            public string Name { get; set; }

            public object Value { get; set; }
        }

        [DebuggerStepThrough]
        public static IEnumerable<TEnum> GetFlags<TEnum>(TEnum input) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return FindFlags(EnumConverter<TEnum>.ToInt32(input)).Cast<TEnum>();
        }

        [DebuggerStepThrough]
        public static IEnumerable<int> GetIntFlags<TEnum>(this TEnum input) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return FindFlags(EnumConverter<TEnum>.ToInt32(input));
        }

        public static IEnumerable<int> FindFlags(int flags)
        {
            for (int shift = 0, potentialFlag = 1;
                //continues until we are at a number larger than the flags int,
                //or until we have shifted through every bit (flag uses the last bit 30, 31 is the sign)
                potentialFlag <= flags && shift < 31;
                shift += 1, potentialFlag = 1 << shift)
            {
                if ((flags & potentialFlag) == potentialFlag)
                {
                    yield return potentialFlag;
                }
            }
        }

        public static IEnumerable<long> FindFlags(long flags)
        {
            var potentialFlag = 1L;
            for (var shift = 0;
                //continues until we are at a number larger than the flags long,
                //or until we have shifted through every bit (flag uses the last bit 62, 63 is the sign)
                potentialFlag <= flags && shift < 63;
                shift += 1, potentialFlag = 1L << shift)
            {
                if ((flags & potentialFlag) == potentialFlag)
                {
                    yield return potentialFlag;
                }
            }
        }

        /// <summary>
        /// Allows sorting of an enum in the database by the enum's description
        /// https://stackoverflow.com/a/40203664
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Expression<Func<TSource, int>> DescriptionOrder<TSource, TEnum>(this Expression<Func<TSource, TEnum>> source)
            where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException();
            }

            var body = ((TEnum[])Enum.GetValues(enumType))
                .OrderBy(value => value.GetDescription())
                .Select((value, ordinal) => new
                {
                    value,
                    ordinal
                })
                .Reverse()
                .Aggregate((Expression)null, (next, item) => next == null
                   ? (Expression)
                   Expression.Constant(item.ordinal)
                   : Expression.Condition(
                       Expression.Equal(source.Body, Expression.Constant(item.value)),
                       Expression.Constant(item.ordinal),
                       next));

            return Expression.Lambda<Func<TSource, int>>(body, "enumOrder", source.Parameters);
        }

        public static Expression<Func<TSource, int>> DescriptionOrder<TSource, TEnum>(this Expression<Func<TSource, TEnum?>> source)
            where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            if (!enumType.IsEnum)
            {
                throw new InvalidOperationException();
            }

            var values = ((TEnum[])Enum.GetValues(enumType))
                .OrderBy(value => value.GetDescription())
                .Select((value, ordinal) => new
                {
                    value = (TEnum?)value,
                    ordinal = ordinal + 1 //the null value will be the first
                })
                .ToList();

            values.Reverse();

            //Add null as the first option, which is now the end of the reversed list
            values.Add(new
            {
                value = (TEnum?)null,
                ordinal = 0
            });

            var body = values
                .Aggregate((Expression)null, (next, item) => next == null
                   ? (Expression)
                   Expression.Constant(item.ordinal)
                   : Expression.Condition(
                       Expression.Equal(source.Body, Expression.Constant(item.value, typeof(TEnum?))),
                       Expression.Constant(item.ordinal),
                       next));

            return Expression.Lambda<Func<TSource, int>>(body, "enumOrder", source.Parameters);
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

    }
}
