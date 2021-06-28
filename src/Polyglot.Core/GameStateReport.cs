using System.Collections.Generic;

namespace Polyglot.Core
{
    public record GameStateReport(string CurrentLevel, double ExercisePoints, double AssignmentPoints, double ExerciseGoldCoins, double AssignmentGoldCoins, IEnumerable<string> Feedbacks);
}