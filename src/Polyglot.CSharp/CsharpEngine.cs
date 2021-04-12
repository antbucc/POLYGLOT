using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.CSharp
{
    public class CsharpEngine : LanguageEngine
    {
        public override async Task<bool> TryInstallForAsync(Kernel kernel)
        {
            switch (kernel)
            {
                case CSharpKernel cSharpKernel:
                    AddInterceptorMiddleware(cSharpKernel);

                    await cSharpKernel.SendAsync(new DisplayValue(new FormattedValue("text/markdown", $@"Installed Game Engine Integration for `{cSharpKernel.Name} Kernel`.")));
                    return true;
                    
            }

            return false;
        }

        private static void AddInterceptorMiddleware(CSharpKernel cSharpKernel)
        {
            cSharpKernel.AddMiddleware(async (kernelCommand, c, next) =>
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
                            var alreadyDefinedVariables = new HashSet<string>(cSharpKernel.GetVariableNames());
                            timer.Start();
                            await next(kernelCommand, c);
                            timer.Stop();
                            var newVariables = new HashSet<string>(cSharpKernel.GetVariableNames());
                            subscription.Dispose();

                            newVariables.ExceptWith(alreadyDefinedVariables);
                            var variableData = new Dictionary<string, string>();

                            foreach (var variableName in newVariables)
                            {
                                if (cSharpKernel.TryGetVariable<object>(variableName, out var variableValue))
                                {
                                    variableData[variableName] = variableValue.GetType().Name;
                                }
                                else
                                {
                                    variableData[variableName] = "undefined";
                                }
                            }

                            var report = await client.SubmitActions(c.Command as SubmitCode, cSharpKernel, events, variableData,
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