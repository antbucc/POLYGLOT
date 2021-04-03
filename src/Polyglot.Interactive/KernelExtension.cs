using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
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
                    //await targetKernel.SendAsync(new DisplayValue(new FormattedValue("text/markdown", @"Installed Game Engine Integration for `CSharp Kernel`.")));
                    KernelInvocationContext.Current?.Display(
                        @"Installed Game Engine Integration for `CSharp Kernel`.",
                        "text/markdown");
                    break;
            }

            
        }

        private Task InstallCsharpGameEngineAsync(CSharpKernel kernel)
        {
           
            kernel.UseSubmitCodeInterceptor();
            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

            return Task.CompletedTask;
        }
    }
}
