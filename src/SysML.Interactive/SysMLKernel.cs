using Microsoft.AspNetCore.Html;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Formatting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace SysML.Interactive
{
    public class SysMLKernel :
        Kernel,
        IKernelCommandHandler<SubmitCode>
    {
        private SysMLRpcClient SysMLRpcClient { get; set; }
        private Process SysMLProcess { get; set; }

        public SysMLKernel() : base("sysml")
        {
            
        }

        private void SetProcessEventHandlers(Process SysMLProcess)
        {
            SysMLProcess.Exited += (o, args) =>
            {
                SysMLRpcClient = null;
            };

            SysMLProcess.ErrorDataReceived += (o, args) =>
            {
                KernelInvocationContext.Current?.Publish(new ErrorProduced( args.Data, KernelInvocationContext.Current?.Command));
            };
            SysMLProcess.BeginErrorReadLine();
        }

        public async Task HandleAsync(SubmitCode submitCode, KernelInvocationContext context)
        {
            EnsureSysMLKernelServerIsRunning();

            var result = await SysMLRpcClient.EvalAsync(submitCode.Code);

            var errors = result.SyntaxErrors.Concat(result.SemanticErrors).ToList();

            if (errors.Count > 0)
            {
                var errorMessage = new StringBuilder();
                foreach(var error in errors.Select(e => e.Message))
                {
                    errorMessage.AppendLine(error);
                }

                context.Fail(submitCode, message: errorMessage.ToString());
                return;
            }

            var sumbittedItems = result.Content.Select(c => c.Name);
            var svgText = await SysMLRpcClient.GetSvgAsync(sumbittedItems, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());
            var svg = new SysMLSvg(svgText);

            context.Display(svg, HtmlFormatter.MimeType);
            context.Publish(new ReturnValueProduced(result, submitCode, FormattedValue.FromObject(result)));
        }

        private void EnsureSysMLKernelServerIsRunning()
        {
            if (SysMLProcess is null)
            {
                EnsureJavaRuntimeIsInstalled();

                var sysMLJarPath = Environment.GetEnvironmentVariable("SYSML_JAR_PATH");
                if (sysMLJarPath is null)
                {
                    throw new Exception("Environment variable \"SYSML_JAR_PATH\" is not set");
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar SysMLKernelServer.jar",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = sysMLJarPath
                };

                //var path = Environment.GetEnvironmentVariable("PATH");
                //psi.EnvironmentVariables["PATH"] = $"{path};{sysMLJarPath}";

                SysMLProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                RegisterForDisposal(() =>
                {
                    SysMLRpcClient = null;
                    SysMLProcess.Kill(true);
                    SysMLProcess.Dispose();
                });

                SysMLProcess.Start();
                SetProcessEventHandlers(SysMLProcess);

                SysMLRpcClient = new(SysMLProcess.StandardInput.BaseStream, SysMLProcess.StandardOutput.BaseStream);
            }
        }

        private static void EnsureJavaRuntimeIsInstalled()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = " --version",
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                Process pr = Process.Start(psi);
            }
            catch (Exception)
            {
                throw new Exception("Missing JRE. JRE is required to use the SysML Kernel");
            }
        }
    }

    public class SysMLSvg
    {
        public SysMLSvg(string svg)
        {
            Svg = svg;
        }

        public string Svg { get; }
    }

    public static class SysMLKernelExtension
    {
        public static CompositeKernel UseSysML(this CompositeKernel kernel)
        {
            var sysMLKernel = new SysMLKernel();
            kernel.Add(sysMLKernel);

            RegisterFormatters();
            return kernel;
        }

        public static void RegisterFormatters()
        {
            Formatter.Register<SysMLSvg>((value, writer) =>
            {
                var html = div(new HtmlString(value.Svg));
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.Register<SysMLInteractiveResult>((value, writer) =>
            {
                var html = div(new HtmlString($"done with {value.Warnings?.Count()} warnings."));
                writer.Write(html);
            }, HtmlFormatter.MimeType);
        }
    }
}