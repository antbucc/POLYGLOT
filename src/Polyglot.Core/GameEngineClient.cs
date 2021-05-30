using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;

namespace Polyglot.Core
{
    public class GameEngineClient
    {
        public static GameEngineClient Current { get; set; }
        private GameStatus _gameStatus;
        private readonly HttpClient _client;
        private DateTime? _lastRun;
        private string _currentLevel = "0";
        private readonly Dictionary<string, IMetricCalculator> _metrics = new ();
        public string GameId { get; }
        public string UserId { get; }
        public string Password { get; }
        public string PlayerId { get; }
        public Uri ServerUrl { get; }
        public static string DefaultServerUrl { get; } = "http://139.177.202.145:9090/";

        private GameEngineClient(string gameId, string userId, string password, string playerId, string serverUrl,
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

            GameId = gameId;
            UserId = userId;
            Password = password;
            PlayerId = playerId;
            ServerUrl = new Uri(serverUrl);
            _client = httpClient;
            
        }

        public static void Configure(string gameId, string userId, string password, string playerId,
            string serverUrl = null, Func<HttpClient> clientFactory = null)
        {
            Current = new GameEngineClient(gameId, userId, password, playerId,
                string.IsNullOrWhiteSpace(serverUrl) ? DefaultServerUrl : serverUrl, clientFactory?.Invoke() ?? new HttpClient());
        }

        public void AddMetric(string metricId, IMetricCalculator metric)
        {
            _metrics[metricId] = metric;
        }

        private Task<string[]> GetMetricsAsync()
        {
            return Task.FromResult(new[] { "timeSpent", "warnings", "errors", "newVariables", "newVariablesWithValue", "timeSinceLastAction", "success", "declaredClasses", "declarationsStructure", "topLevelClassesStructureMetric" });
        }

        public async Task<GameStateReport> SubmitActions(SubmitCode command, Kernel kernel,
            List<KernelEvent> events, IReadOnlyDictionary<string, object> newVariables, TimeSpan runTime)
        {

            var authenticated = await EnsureAuthentication();
            if(!authenticated)
            {
                var errorString = "Authentication Error";
                ImmutableArray<FormattedValue> formattedValues = ImmutableArray.Create(new FormattedValue(PlainTextFormatter.MimeType, errorString));

                KernelInvocationContext.Current?.Publish(new ErrorProduced(errorString,
                    KernelInvocationContext.Current?.Command, formattedValues));

                return null;
            }

            var callUrl = new Uri(ServerUrl, "api/submit-code");

            // find required metrics for current stage
            var metrics = await GetMetricsAsync();

            var data = new Dictionary<string, object>();

            // compile data using the values calculated for the required metrics
            foreach (var metric in metrics)
            {
                if (_metrics.TryGetValue(metric, out var metricCalculator))
                {
                    data[metric] =
                        await metricCalculator.CalculateAsync(command, kernel, events, newVariables, runTime, _lastRun);
                }
                else
                {
                    data[metric] = "not found";
                }
            }

            var bodyObject = new
            {
                gameId = GameId,
                playerId = PlayerId,
                exerciseNumber = _currentLevel,
                data = data
            };

            var response = await _client.PostAsync(callUrl, bodyObject.ToBody());
            if (response.StatusCode != HttpStatusCode.OK)
            {
                ImmutableArray<FormattedValue> formattedValues = ImmutableArray.Create(new FormattedValue(PlainTextFormatter.MimeType, response.ReasonPhrase));
            
                KernelInvocationContext.Current?.Publish(new ErrorProduced(response.ReasonPhrase,
                    KernelInvocationContext.Current?.Command, formattedValues));

                return null;
            }

            _lastRun = DateTime.Now;
            return await GetGameStateReportFromResponseAsync(response);
        }

        private async Task<bool> EnsureAuthentication()
        {
            var callUrl = new Uri(ServerUrl, "oauth/token");

            var authenticationContent = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username", UserId },
                { "password", Password }
            };

            //JwtSecurityTokenHandler;

            // login authentication token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "NjRiMDRkZDgtY2Y0Zi00MzU2LTk1OGItNzY1ZTNkOGY0MzM4OnBvbHlnbG90");

            using var encodedContent = new FormUrlEncodedContent(authenticationContent);
            encodedContent.Headers.Clear();
            encodedContent.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            var response = await _client.PostAsync(callUrl, encodedContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var auth = responseContent.ToObject<AuthenticationResponse>();
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.access_token);
                return true;
            }
            return false;

        }

        public async Task<GameStateReport> GetReportAsync()
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

        private async Task<GameStateReport> GetGameStateReportFromResponseAsync(HttpResponseMessage response)
        {
            var contents = await response.Content.ReadAsStringAsync();

            var gameStatus = contents.ToObject<GameStatus>();

            // report the new status to the player
            _gameStatus = gameStatus with { CustomData = gameStatus.CustomData with { Level = gameStatus.CustomData.Level ?? "0" } };
            _currentLevel = _gameStatus.CustomData.Level;

            var scoring = gameStatus.State.PointConcept.ToDictionary(p => p.Name);

            return new GameStateReport(_currentLevel,
                scoring["points"].Score,
                scoring["gold coins"].Score,
                _gameStatus.CustomData.Feedback ?? "");
        }

        public static void Reset()
        {
            Current = null;
        }
    }

}