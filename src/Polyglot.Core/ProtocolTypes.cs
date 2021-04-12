using System.Text.Json.Serialization;

namespace Polyglot.Core
{
    public record PointConcept(string Id, string Name, double Score);

    public record GameState(PointConcept[] PointConcept)
    {
        [JsonPropertyName("PointConcept")]
        public PointConcept[] PointConcept { get; init; } = PointConcept;
    }

    public record GameStatus(string PlayerId, string GameId, GameState State, CustomData CustomData);

    public record CustomData(string Level);
}
