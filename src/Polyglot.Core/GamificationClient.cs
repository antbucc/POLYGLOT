using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Gamification
{
    public class GamificationClient
    {
        public static GamificationClient Current { get; private set; }
        private GameStatus _gameStatus;
        private readonly HttpClient _client;
        private DateTime? _lastRun;
        private string _currentLevel = "0";
        public string GameId { get; }
        public string UserId { get; }
        public string Password { get; }
        public string PlayerId { get; }
        public Uri ServerUrl { get; }
        public static string DefaultServerUrl { get; } = "http://139.177.202.145:9090/";

        private GamificationClient(string gameId, string userId, string password, string playerId, string serverUrl,
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
            Current = new GamificationClient(gameId, userId, password, playerId,
                string.IsNullOrWhiteSpace(serverUrl) ? DefaultServerUrl : serverUrl, clientFactory?.Invoke() ?? new HttpClient());
        }

        public async Task<GameStateReport> InvokeGamification(IReadOnlyDictionary<string, object> data)
        {
            var authenticated = await EnsureAuthentication();
            if(!authenticated)
            {
                throw new AuthenticationException("Failed authentication with the credentials provided.");
            }

            var callUrl = new Uri(ServerUrl, "api/submit-code");

            var bodyObject = new
            {
                gameId = GameId,
                playerId = PlayerId,
                exerciseNumber = _currentLevel,
                lastRun = _lastRun,
                data
            };

            var response = await _client.PostAsync(callUrl, bodyObject.ToBody());
            _lastRun = DateTime.Now;
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

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
                scoring["exercisePoints"].Score,
                scoring["assignmentPoints"].Score,
                scoring["exerciseGoldCoins"].Score,
                scoring["assignmentGoldCoins"].Score,
                _gameStatus.CustomData.Feedbacks?.Select(f => f.Text) ?? new List<string>());
        }

        public static void Reset()
        {
            Current._client.Dispose();
            Current = null;
        }
    }

}