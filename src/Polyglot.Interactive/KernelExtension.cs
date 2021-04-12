using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Polyglot.Core;
using Polyglot.CSharp;

namespace Polyglot.Interactive
{
    public class KernelExtension : IKernelExtension
    {
        public async Task OnLoadAsync(Kernel kernel)
        {
            await kernel.VisitSubkernelsAndSelfAsync(InstallGameEngineAsync);
            await Engine.Instance.InstallLanguageEngineAsync(new CsharpEngine());

        }

        private Task InstallGameEngineAsync(Kernel targetKernel)
        {
            targetKernel.UseGameEngine();
            Engine.Instance.RegisterKernel(targetKernel);
            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);
            return Task.CompletedTask;
        }
    }
}
