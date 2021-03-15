using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Polyglot.Interactive
{
    internal static class KernelExtensions
    {
        public static T UseGameEngine<T>(this T kernel)
            where T : Kernel
        {
            kernel.AddDirective(SetupEngine());

            kernel.AddDirective(GetCurrentState());

            return kernel;

            static Command GetCurrentState()
            {
                var command = new Command("#!game-status", "Gets the current Game Status.")
                {
                    Handler = CommandHandler.Create<KernelInvocationContext>(async context =>
                    {
                        var status = await GameEngineClient.Current.GetReportAsync();
                        context.Display(status);
                    })

                };

                return command;
            }

            static Command SetupEngine()
            {
                var gameIdOption = new Option<string>(
                    "--game-id",
                    "The game id to use.");

                var gameTokenOption = new Option<string>(
                    "--game-token",
                    "The game token to use.");

                var playerIdOption = new Option<string>(
                    "--player-id",
                    "The player id to use.");

                var serverUrlOption = new Option<string>(
                    "--server-url",
                    "The server url to use.");

                var command = new Command("#!start-game", "Configures the game engine for the current notebook.")
                {
                    Handler = CommandHandler.Create<string, string, string, string, KernelInvocationContext>((gameId, gameToken, playerId, serverUrl, context) =>
                       {

                           GameEngineClient.Configure(gameId, gameToken, playerId, serverUrl);

                           KernelInvocationContext.Current?.Display(
                               @"Game Engine configuration is now complete.",
                               "text/markdown");

                           return Task.CompletedTask;
                       })

                };
                command.AddOption(gameIdOption);
                command.AddOption(gameTokenOption);
                command.AddOption(playerIdOption);
                command.AddOption(serverUrlOption);
                return command;
            }
        }

        public static T UseSubmitCodeInterceptor<T>(this T kernel)
            where T : Kernel
        {
            kernel.AddMiddleware(async (kernelCommand, c, next) =>
            {
                // intercept code submission
                switch (kernelCommand)
                {
                    case SubmitCode _:
                        var client = GameEngineClient.Current;

                        // before is all done we append a final action to submit all vents for the command
                        if (client != null)
                        { // let/s record all events
                            var events = new List<KernelEvent>();
                            var subscription = c.KernelEvents.Subscribe(events.Add);
                            
                            var timer = new Stopwatch();
                            timer.Start();
                            await next(kernelCommand, c);
                            timer.Stop();
                            subscription.Dispose();
                            var report = await client.SubmitActions(c.Command, c.HandlingKernel, events, timer.Elapsed);
                            if (report is { })
                            {
                                c.Display(report);
                            }
                        }
                        else
                        {
                            await next(kernelCommand, c);
                        }

                        break;
                    default:
                        await next(kernelCommand, c);
                        break;
                }


            });

            return kernel;
        }
    }
}