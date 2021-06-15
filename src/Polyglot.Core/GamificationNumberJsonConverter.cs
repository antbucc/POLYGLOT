using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Polyglot.Core
{
    class GamificationFloatJsonConverter : JsonConverter<float>
    {
        public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Single.Parse(reader.GetString());

        public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("0.0#", CultureInfo.InvariantCulture));
    }

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
                            catch (InvalidOperationException e)
                            {
                                result = (int)float.Parse(reader.GetString());
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
                            catch (InvalidOperationException e)
                            {
                                result = double.Parse(reader.GetString());
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
                            catch (InvalidOperationException e)
                            {
                                result = float.Parse(reader.GetString());
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
