using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.CommandLine;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;

namespace Polyglot.Interactive
{
    public class GameEngineClient
    {
        private bool _authenticated;
        public string GameId { get; }
        public string Token { get; }
        public string PlayerId { get; }
        public string ServerUrl { get; }

        private GameEngineClient(string gameId, string token, string playerId, string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
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
            Token = token;
            PlayerId = playerId;
            ServerUrl = serverUrl;
        }

        public static GameEngineClient Current { get; set; }

        public static void Configure(string gameId, string token, string playerId, string serverUrl = null)
        {
            Current = new GameEngineClient(gameId, token, playerId, serverUrl ?? DefaultServerUrl);
        }

        public static string DefaultServerUrl { get; } = "https://dev.smartcommunitylab.it/gamification-v3/exec/game/";

        public async Task<GameStateReport> SubmitActions(KernelCommand contextCommand, Kernel handlingKernel, List<KernelEvent> events)
        {
            using var client = new HttpClient();

            await EnsureAuthentication(client);

            var callUrl = new Uri(ServerUrl);
            var action = "SubmitCode";

            callUrl = new Uri(callUrl, $"{GameId}/action/{action}");
            var bodyObject = new
            {
                gameId = GameId,
                playerId = PlayerId
            };


            var response = await client.PostAsync(callUrl, bodyObject.ToBody());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // get the report here and display

                return new GameStateReport();
            }

            var formattedValues = new ImmutableArray<FormattedValue>
            {
                new FormattedValue(PlainTextFormatter.MimeType, response.ReasonPhrase)
            };

            KernelInvocationContext.Current?.Publish(new ErrorProduced(response.ReasonPhrase,
                KernelInvocationContext.Current?.Command, formattedValues));

            return null;
        }

        private async Task EnsureAuthentication(HttpClient client)
        {
            if (!_authenticated)
            {
                var callUrl = new Uri(ServerUrl);
                var action = "SubmitCode";

                callUrl = new Uri(callUrl, $"{GameId}/action/{action}");
                var bodyObject = new
                {
                    gameId = GameId,
                    playerId = PlayerId
                };

                var response = await client.PostAsync(callUrl, bodyObject.ToBody());

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _authenticated = true;
                }
            } 
        }
    }

    public class GameStateReport    
    {
    }
}