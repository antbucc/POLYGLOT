using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Polyglot.Gamification
{
    public record PointConcept(string Id, string Name, double Score);
    public record BadgeConcept(string Id, string Name, IEnumerable<string> BadgeEarned);

    public record GameState(BadgeConcept[] BadgeConcept, PointConcept[] PointConcept)
    {
        [JsonPropertyName("BadgeCollectionConcept")]
        public BadgeConcept[] BadgeConcept { get; init; } = BadgeConcept;

        [JsonPropertyName("PointConcept")]
        public PointConcept[] PointConcept { get; init; } = PointConcept;
    }
    public record StringSpan(int Start, int End);

    public record Feedback(string Text, StringSpan Span);

    // public record CustomData(string Level, IEnumerable<Feedback> Feedbacks);
    public record CustomData(IDictionary<string, int> AttemptsC, IDictionary<string, int> AttemptsS);

    public record GameStatus(string PlayerId, string GameId, GameState State, CustomData CustomData);

    internal record AuthenticationResponse(string access_token, string token_type, string refresh_token, int expires_in, string scope, string jti);

    public record AssignmentFileResponseBody(string id, string name, string description, string userid, string competencyid, string submissionstatus, string url, string pathnamehash);

    public record AssignmentFileResponse(bool success, AssignmentFileResponseBody response);
}
