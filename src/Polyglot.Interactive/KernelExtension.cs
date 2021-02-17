using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Formatting;

namespace Polyglot.Interactive
{
    public class KernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            return kernel.VisitSubkernelsAndSelfAsync(InstallGameEngineAsync);
        }

        private async Task InstallGameEngineAsync(Kernel targetKernel)
        {
            targetKernel.UseGameEngine();
            switch (targetKernel)
            {
                case CSharpKernel cSharpKernel:
                    await InstallCsharpGameEngineAsync(cSharpKernel);
                    break;
            }
            
        }

        private async Task InstallCsharpGameEngineAsync(CSharpKernel kernel)
        {
            kernel.UseSubmitCodeInterceptor();
            KernelInvocationContext.Current?.Display(
                @"Installed Game Engine Integration for `CSharp Kernel`.",
                "text/markdown");
        }
    }
}
