using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;

namespace Polyglot.Gamification
{
    public class GamificationClient
    {
        public static GamificationClient Current { get; private set; }
        private GameStatus _gameStatus;
        private readonly HttpClient _client;
        private string _currentLevel = "0";
        public string GameId { get; }
        public string UserId { get; }
        public string Password { get; }
        public string PlayerId { get; }
        public Uri ServerUrl { get; }
        public Uri LMSUrl { get; }
        public static string DefaultServerUrl { get; } = "http://139.177.202.145:9090/";
        public static string DefaultLMSUrl { get; } = "http://93.104.214.51/dashboard/local/api/";

        private GamificationClient(string gameId, string userId, string password, string playerId, string serverUrl, string lmsUrl,
            HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(password));
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serverUrl));
            }

            if (string.IsNullOrWhiteSpace(lmsUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(lmsUrl));
            }

            GameId = gameId;
            UserId = userId;
            Password = password;
            PlayerId = playerId;
            ServerUrl = new Uri(serverUrl);
            LMSUrl = new Uri(lmsUrl);
            _client = httpClient;

        }

        public static void Configure(string gameId, string userId, string password, string playerId,
            string serverUrl = null, Func<HttpClient> clientFactory = null)
        {
            Reset();
            Current = new GamificationClient(gameId, userId, password, playerId,
                string.IsNullOrWhiteSpace(serverUrl) ? DefaultServerUrl : serverUrl, DefaultLMSUrl, clientFactory?.Invoke() ?? new HttpClient());
        }

        public async Task<AssignmentFileResponseBody> GetNextAssignmentAsync(string userId, string courseId, List<int> assignmentsToExclude)
        {
            var parameters = new Dictionary<string, string>
        {
            { "action", "assignmentfile" },
            { "authtoken", "9fc7714b3e9eb904191427479baea02b" },
            { "userid", userId },
            { "courseid", courseId }
        };


            var builder = new QueryBuilder(parameters);
            assignmentsToExclude.ForEach(id => builder.Add("excludeAssignIds[]", $"{id}"));

            var callUrl = new Uri(LMSUrl, builder.ToString());

            var response = await _client.GetAsync(callUrl);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var contents = await response.Content.ReadAsStringAsync();
            var assignment = contents.ToObject<AssignmentFileResponse>();
            return assignment?.response;
        }

        public string GetAssignmentNotebookUrl(string pathnamehash)
        {
            var parameters = new Dictionary<string, string>
        {
            { "action", "file" },
            { "authtoken", "9fc7714b3e9eb904191427479baea02b" },
            { "pathnamehash", pathnamehash }
        };

            var builder = new QueryBuilder(parameters);
            return LMSUrl + builder.ToString();
        }

        public async Task<string> ExercisePassedAsync(string courseId, string assignmentId, string numberOfAttempts, string competencyId) => await SubmitExerciseStatusAsync(courseId, assignmentId, numberOfAttempts, competencyId, "pass");
        public async Task<string> ExerciseStepPassedAsync(string courseId, string assignmentId, string numberOfAttempts, string competencyId) => await SubmitExerciseStatusAsync(courseId, assignmentId, numberOfAttempts, competencyId, "stepPass");
        public async Task<string> ExerciseFailedAsync(string courseId, string assignmentId, string numberOfAttempts, string competencyId) => await SubmitExerciseStatusAsync(courseId, assignmentId, numberOfAttempts, competencyId, "fail");

        private async Task<string> SubmitExerciseStatusAsync(string courseId, string assignmentId, string numberOfAttempts, string competencyId, string status)
        {
            var parameters = new Dictionary<string, string>
            {
                { "action", "assignmentDone" },
                { "authtoken", "9fc7714b3e9eb904191427479baea02b" },
                { "userid", PlayerId },
                { "courseid", courseId },
                { "assignmentid", assignmentId },
                { "competencyid", competencyId },
                { "numberofattempts", numberOfAttempts },
                { "result", status }
            };


            var builder = new QueryBuilder(parameters);
            var callUrl = new Uri(LMSUrl, builder.ToString());

            var response = await _client.PostAsync(callUrl, (new { }).ToBody());
            return response.StatusCode.ToString();
        }

        public async Task<GameStatus> GetReportAsync()
        {
            var callUrlStatus = new Uri("https://dev.smartcommunitylab.it/gamification-v3/");
            callUrlStatus = new Uri(callUrlStatus, $"data/game/{GameId}/player/{PlayerId}");


            var authToken = Encoding.ASCII.GetBytes($"{UserId}:{Password}");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(authToken));

            var responseStatus = await _client.GetAsync(callUrlStatus);
            if (responseStatus.StatusCode == HttpStatusCode.OK)
            {
                // retrieve the player status from the GET call response and print it
                return await GetGameStateReportFromResponseAsync(responseStatus);
            }

            throw new InvalidCastException(
                $"Failed Game Engine Step, Code: {responseStatus.StatusCode}, Reason: {responseStatus.ReasonPhrase}");
        }

        private async Task<GameStatus> GetGameStateReportFromResponseAsync(HttpResponseMessage response)
        {
            var contents = await response.Content.ReadAsStringAsync();

            var gameStatus = contents.ToObject<GameStatus>();

            _gameStatus = gameStatus;

            return gameStatus;

            // report the new status to the player
            //_gameStatus = gameStatus with { CustomData = gameStatus.CustomData with { Level = gameStatus.CustomData.Level ?? "0" } };
            //_currentLevel = _gameStatus.CustomData.Level;

            //var scoring = gameStatus.State.PointConcept.ToDictionary(p => p.Name);

            //return new GameStateReport(scoring["exercisePoints"].Score,
            //    scoring["assignmentPoints"].Score,
            //    scoring["exerciseGoldCoins"].Score,
            //    scoring["assignmentGoldCoins"].Score,
            //    _gameStatus.CustomData.Feedbacks?.Select(f => f.Text) ?? new List<string>());
        }

        public static void Reset()
        {
            Current?._client.Dispose();
            Current = null;
        }
    }
}