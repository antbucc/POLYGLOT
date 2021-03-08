using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
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
            kernel.AddDirective(setupEngine());
            KernelInvocationContext.Current?.Display(
                @"Installed magic command `#!game-time`.",
                "text/markdown");
            return kernel;

            static Command setupEngine()
            {
                var gameIdOption = new Option<string>(
                    "--game-id",
                    "The game id to use.");

                var gameTokenOption = new Option<string>(
                    "--game-token",
                    "The game token to use.");

                var playerIdOption = new Option<string>(
                    "--player-id",
                    "The user id to use.");

                var serverUrlOption = new Option<string>(
                    "--server-url",
                    "The game id to use.");

                var command =  new Command("#!game-time", "Configures the game engine for the current notebook.")
                {
                    Handler = CommandHandler.Create<string,string,string,string, KernelInvocationContext>((gameId, gameToken, playerId, serverUrl,  context) =>
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
                    case SubmitCode submitCode:
                        // let/s record all events
                        var events = new List<KernelEvent>();
                        var subscription = c.KernelEvents.Subscribe(events.Add);
                        subscription.Dispose();
                        var client = GameEngineClient.Current;
                       
                        await next(kernelCommand, c);

                        // before is all done we append a final action to submit all vents for the command
                        var report = await client.SubmitActions(c.Command, c.HandlingKernel, events);
                        if (report is { })
                        {
                            c.Display(report);
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