using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Polyglot.Interactive
{
    public class GameEngineClient
    {
        public string GameId { get; }
        public string UserId { get; }
        public string ServerUrl { get; }

        private GameEngineClient(string gameId, string userId, string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(gameId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(gameId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serverUrl));
            }

            GameId = gameId;
            UserId = userId;
            ServerUrl = serverUrl;
        }

        public static GameEngineClient Current { get; set; }

        public static void Configure(string gameId, string userId, string serverUrl = null)
        {
            Current = new GameEngineClient(gameId, userId, serverUrl ?? DefaultServerUrl);
        }

        public static string DefaultServerUrl { get; } = "http://no-idea";

        public Task<GameStateReport> SubmitActions(KernelCommand contextCommand, Microsoft.DotNet.Interactive.Kernel handlingKernel, List<KernelEvent> events)
        {
            throw new NotImplementedException();
        }
    }

    public class GameStateReport    
    {
    }
}