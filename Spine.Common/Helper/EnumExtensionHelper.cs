using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Spine.Common.Attributes;

namespace Spine.Common.Helper
{
    public static class EnumExtensionHelper
    {
        internal static (Dictionary<Enum, DescriptionAttribute> Descriptions, HashSet<Enum> ExcludedEnums) GetEnumDescriptions()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var enums = assemblies
                .Where(assembly => assembly.FullName.StartsWith("Spine.Common"))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsEnum)
                .SelectMany(GetEnumValues)
                .ToList();

            var descriptions = enums
                    .Where(ExcludeEnumValue)
                    .ToDictionary(tuple => tuple.Value, elementSelector: GetEnumDescription);

            var excluded = enums.Where(w => !ExcludeEnumValue(w)).Select(s => s.Value).ToHashSet();

            return (descriptions, excluded);
        }

        private struct EnumValueTuple
        {
            public Enum Value { get; set; }
            public FieldInfo FieldInfo { get; set; }
        }

        private static IEnumerable<EnumValueTuple> GetEnumValues(Type enumType)
        {
            return enumType.GetEnumValues()
                           .Cast<Enum>()
                           .Select(enumValue => new EnumValueTuple
                           {
                               Value = enumValue,
                               FieldInfo = enumType.GetField(enumValue.ToString())
                           });
        }

        private static bool ExcludeEnumValue(EnumValueTuple tuple)
        {
            var excludeEnumValueAttribute = tuple.FieldInfo.GetCustomAttribute<ExcludeEnumValueAttribute>();
            return excludeEnumValueAttribute == null;
        }

        private static DescriptionAttribute GetEnumDescription(EnumValueTuple tuple)
        {
            var defaultDescription = tuple.Value.ToString();

            foreach (var attribute in tuple.FieldInfo.GetCustomAttributes())
            {
                if (TryGetDescriptionFromAttribute(attribute, out string description))
                {
                    return new DescriptionAttribute(description);
                }
            }

            return new DescriptionAttribute(defaultDescription);
        }

        private static bool TryGetDescriptionFromAttribute(Attribute attribute, out string description)
        {
            var descriptionAttribute = attribute as DescriptionAttribute;
            if (descriptionAttribute != null)
            {
                description = descriptionAttribute.Description;
                return true;
            }
            else if (attribute is DisplayAttribute)
            {
                var displayAttribute = (DisplayAttribute)attribute;
                return TryGetDescriptionFromDisplayAttribute(displayAttribute, out description);
            }
            else if (attribute is DisplayNameAttribute)
            {
                description = ((DisplayNameAttribute)attribute).DisplayName;
                return true;
            }
            else
            {
                description = null;
                return false;
            }
        }

        private static bool TryGetDescriptionFromDisplayAttribute(DisplayAttribute displayAttribute, out string description)
        {
            description = displayAttribute.GetDescription();
            if (!string.IsNullOrWhiteSpace(description))
            {
                return true;
            }

            description = displayAttribute.GetName();
            if (!string.IsNullOrWhiteSpace(description))
            {
                return true;
            }

            return false;
        }

        public static string GenerateRandomString(int length, bool uppercase, bool specialXter)
        {
            RandomStringGenerator stringGenerator = new RandomStringGenerator();

            return stringGenerator.NextString(length, true, uppercase, true, specialXter);
        }

        #region Helper Methods

        public static bool HasMinimumLength(string password, int minLength)
        {
            return password.Length >= minLength;
        }

        public static bool HasMinimumUniqueChars(string password, int minUniqueChars)
        {
            return password.Distinct().Count() >= minUniqueChars;
        }

        /// <summary>
        /// Returns TRUE if the password has at least one digit
        /// </summary>
        public static bool HasDigit(string password)
        {
            return password.Any(c => char.IsDigit(c));
        }

        /// <summary>
        /// Returns TRUE if the password has at least one special character
        /// </summary>
        public static bool HasSpecialChar(string password)
        {
            // return password.Any(c => char.IsPunctuation(c)) || password.Any(c => char.IsSeparator(c)) || password.Any(c => char.IsSymbol(c));
            return password.IndexOfAny("!@#$%^&*?_~-£().,".ToCharArray()) != -1;
        }

        /// <summary>
        /// Returns TRUE if the password has at least one uppercase letter
        /// </summary>
        public static bool HasUpperCaseLetter(string password)
        {
            return password.Any(c => char.IsUpper(c));
        }

        /// <summary>
        /// Returns TRUE if the password has at least one lowercase letter
        /// </summary>
        public static bool HasLowerCaseLetter(string password)
        {
            return password.Any(c => char.IsLower(c));
        }
        #endregion

        public static bool IsStrongPassword(string password)
        {
            return HasMinimumLength(password, 6)
                && HasUpperCaseLetter(password)
                && HasLowerCaseLetter(password)
                && (HasDigit(password) || HasSpecialChar(password));
        }


    }
}
