using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyglot.Core
{
    public class NumberFloatJsonConverterFactory : JsonConverterFactory
    {
        private class NumberConverter<T> : JsonConverter<T>
        {

            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                switch (typeToConvert)
                {
                    case Type x when x == typeof(int):
                        {
                            int result;
                            try
                            {
                                result = reader.GetInt32();
                            }
                            catch (InvalidOperationException)
                            {
                                result = (int)float.Parse(reader.GetString(), CultureInfo.InvariantCulture);
                            }
                            return (T)(object)result;
                        }
                    case Type x when x == typeof(double):
                        {
                            double result;
                            try
                            {
                                result = reader.GetDouble();
                            }
                            catch (InvalidOperationException)
                            {
                                result = double.Parse(reader.GetString(), CultureInfo.InvariantCulture);
                            }
                            return (T)(object)result;
                        }
                    case Type x when x == typeof(float):
                        {
                            float result;
                            try
                            {
                                result = reader.GetInt32();
                            }
                            catch (InvalidOperationException)
                            {
                                result = float.Parse(reader.GetString(), CultureInfo.InvariantCulture);
                            }
                            return (T)(object)result;
                        }
                    default:
                        return default;
                }
            }

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {

                var stringValue = "0.0";

                switch (value)
                {
                    case int x:
                        stringValue = x.ToString("0.0#", CultureInfo.InvariantCulture);
                        break;
                    case double x:
                        stringValue = x.ToString("0.0#", CultureInfo.InvariantCulture);
                        break;
                    case float x:
                        stringValue = x.ToString("0.0#", CultureInfo.InvariantCulture);
                        break;
                }

                writer.WriteStringValue(stringValue);
            }
        }

        public override bool CanConvert(Type typeToConvert)
        {
            switch (typeToConvert)
            {
                case Type x when x == typeof(int):
                    return true;
                case Type x when x == typeof(double):
                    return true;
                case Type x when x == typeof(float):
                    return true;
                default:
                    return false;
            }
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(typeof(NumberConverter<>).MakeGenericType(new[] { typeToConvert }));
            return converter;
        }
    }
}
