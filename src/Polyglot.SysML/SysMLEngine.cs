using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;
using SysML.Interactive;

namespace Polyglot.CSharp
{
    public class SysMLEngine : LanguageEngine
    {
        public override async Task<bool> TryInstallForAsync(Kernel kernel)
        {
            switch (kernel)
            {
                case SysMLKernel sysMLKernel:
                    AddInterceptorMiddleware(sysMLKernel);

                    await sysMLKernel.SendAsync(new DisplayValue(new FormattedValue("text/markdown", $@"Installed Game Engine Integration for `{sysMLKernel.Name} Kernel`.")));
                    return true;
                    
            }

            return false;
        }

        private static void AddInterceptorMiddleware(SysMLKernel sysMLKernel)
        {
            sysMLKernel.AddMiddleware(async (kernelCommand, c, next) =>
            {
                // intercept code submission
                switch (kernelCommand)
                {
                    case SubmitCode submitCode:
                        var client = GameEngineClient.Current;

                        // before is all done we append a final action to submit all vents for the command
                        if (client != null && !submitCode.Code.StartsWith("#!"))
                        {
                            // let's record all events
                            var events = new List<KernelEvent>();
                            var subscription = c.KernelEvents.Subscribe(events.Add);

                            var timer = new Stopwatch();
                            timer.Start();
                            await next(kernelCommand, c);
                            timer.Stop();
                            subscription.Dispose();

                            var report = await client.SubmitActions(c.Command as SubmitCode, sysMLKernel, events, null,
                                timer.Elapsed);
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
        }
    }
}