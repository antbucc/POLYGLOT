using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyglot.Interactive
{
    internal static class BodyHelper
    {
        private static JsonSerializerOptions Options { get; } =
            new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };

        public static StringContent ToBody(this object source)
        {
            return new StringContent(source.ToJson(), Encoding.UTF8, "application/json");
        }

        public static string ToJson(this object source)
        {
            return JsonSerializer.Serialize(source, Options);
        }
    }
}