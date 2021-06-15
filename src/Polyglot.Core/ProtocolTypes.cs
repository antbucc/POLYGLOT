using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Polyglot.Core
{
    public record StringSpan(int Start, int End);

    public record PointConcept(string Id, string Name, double Score);

    public record GameState(PointConcept[] PointConcept)
    {
        [JsonPropertyName("PointConcept")]
        public PointConcept[] PointConcept { get; init; } = PointConcept;
    }

    public record GameStatus(string PlayerId, string GameId, GameState State, CustomData CustomData);

    public record Feedback(string Text, StringSpan Span);

    public record CustomData(string Level, IEnumerable<Feedback> Feedbacks);

    public record AuthenticationResponse(string access_token, string token_type, string refresh_token, int expires_in, string scope, string jti);
}
