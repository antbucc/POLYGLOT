using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyglot.Core
{
    public static class BodyHelper
    {
        private static JsonSerializerOptions Options { get; } =
            new(JsonSerializerDefaults.General)
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(),
                    new NumberFloatJsonConverterFactory()
                }
            };

        public static StringContent ToBody(this object source)
        {
            return new(source.ToJson(), Encoding.UTF8, "application/json");
        }

        public static string ToJson(this object source)
        {
            var text =  JsonSerializer.Serialize(source, Options);
            return text;
        }

        public static T ToObject<T>(this string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString, Options);
        }
    }
}