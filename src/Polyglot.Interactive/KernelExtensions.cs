using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Polyglot.Core;

namespace Polyglot.Interactive
{
    public static class KernelExtensions
    {
        public static T UseGameEngine<T>(this T kernel, Func<HttpClient> clientFactory = null)
            where T : Kernel
        {
            kernel.AddDirective(SetupEngine(clientFactory));

            kernel.AddDirective(GetCurrentState());

            return kernel;

            static Command GetCurrentState()
            {
                var command = new Command("#!game-status", "Gets the current Game Status.")
                {
                    Handler = CommandHandler.Create<KernelInvocationContext>(async context =>
                    {
                        var status = await GamificationClient.Current.GetReportAsync();
                        context.Display(status);
                    })

                };

                return command;
            }

            static Command SetupEngine(Func<HttpClient> factory)
            {
                var gameIdOption = new Option<string>(
                    "--game-id",
                    "The game id to use.");

                var passwordOption = new Option<string>(
                    "--password",
                    "The game token to use.");

                var userIdOption = new Option<string>(
                    "--user-id",
                    "The game token to use.");

                var playerIdOption = new Option<string>(
                    "--player-id",
                    "The player id to use.");

                var serverUrlOption = new Option<string>(
                    "--server-url",
                    "The server url to use.");

                var command = new Command("#!start-game", "Configures the game engine for the current notebook.")
                {
                    Handler = CommandHandler.Create<string, string,string, string, string, KernelInvocationContext>((gameId, userId, password, playerId, serverUrl, context) =>
                       {

                           GamificationClient.Configure(gameId, userId, password, playerId, serverUrl, factory);

                           context?.Display(
                               @"Game Engine configuration is now complete.",
                               "text/markdown");

                           return Task.CompletedTask;
                       })

                };
                command.AddOption(gameIdOption); 
                command.AddOption(userIdOption);
                command.AddOption(passwordOption);
                command.AddOption(playerIdOption);
                command.AddOption(serverUrlOption);
                return command;
            }
        }
    }
}