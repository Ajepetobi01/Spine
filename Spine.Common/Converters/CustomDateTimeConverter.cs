
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spine.Common.Extensions;

namespace Spine.Common.Converters
{

    public class DateTimeConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DateTime) ||
                   typeToConvert == typeof(DateTime?) ||
                   typeToConvert == typeof(DateTimeOffset) ||
                   typeToConvert == typeof(DateTimeOffset?);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            //You may be tempted to cache these converter objects. 
            //Don't. JsonSerializer caches them already.
            if (typeToConvert == typeof(DateTime))
            {
                return new CustomDateTimeConverter<DateTime>();
            }
            else if (typeToConvert == typeof(DateTime?))
            {
                return new CustomDateTimeConverter<DateTime?>();
            }
            else if (typeToConvert == typeof(DateTimeOffset))
            {
                return new CustomDateTimeConverter<DateTimeOffset>();
            }
            else if (typeToConvert == typeof(DateTimeOffset?))
            {
                return new CustomDateTimeConverter<DateTimeOffset?>();
            }

            throw new NotSupportedException(
                "CreateConverter got called on a type that this converter factory doesn't support");
        }

        private class CustomDateTimeConverter<T> : JsonConverter<T>
        {
            //private static readonly string _format = "dd-MM-yyyy'T'HH:mm:ss.fff'Z'";  //formato api?
            private static readonly string _format = "dd-MM-yyyy'T'HH:mm:ss.fff";
            
            public override void Write(Utf8JsonWriter writer, T date, JsonSerializerOptions options)
            {
                writer.WriteStringValue((date as dynamic).ToString(_format));
            }

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var dt = reader.GetString();
                if (dt.IsNullOrEmpty()) return null as dynamic;
                return DateTime.Parse(dt) as dynamic; //.ParseExact(dt, _format, System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }

    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly JsonSerializerOptions ConverterOptions;

        public CustomDateTimeConverter()
        {
        }
        
        public CustomDateTimeConverter(JsonSerializerOptions converterOptions)
        {
            ConverterOptions = converterOptions;
        }

        //private static readonly string _format = "dd-MM-yyyy'T'HH:mm:ss.fff'Z'";  //formato api?
        private static readonly string _format = "dd-MM-yyyy'T'HH:mm:ss.fff";


        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dt = reader.GetString();
            return DateTime.Parse(dt); //.ParseExact(dt, _format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format));
        }
        
        
        // public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        // {
        //     //Very important: Pass in ConverterOptions here, not the 'options' method parameter.
        //     return JsonSerializer.Deserialize<DateTime>(ref reader, ConverterOptions);
        // }
        //
        // public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        // {
        //     //Very important: Pass in ConverterOptions here, not the 'options' method parameter.
        //     JsonSerializer.Serialize<DateTime>(writer, value, ConverterOptions);
        // }
    }
}