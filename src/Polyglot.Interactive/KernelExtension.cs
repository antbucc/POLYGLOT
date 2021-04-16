using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Polyglot.Core;
using Polyglot.CSharp;

namespace Polyglot.Interactive
{
    public class KernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            return OnLoadAsync(kernel, () => new HttpClient());
        }

        public async Task OnLoadAsync(Kernel kernel, Func<HttpClient> httpClientFactory)
        {
            await kernel.VisitSubkernelsAndSelfAsync(k => InstallGameEngineAsync(k, httpClientFactory));
            await Engine.Instance.InstallLanguageEngineAsync(new CsharpEngine());
        }

        private Task InstallGameEngineAsync(Kernel targetKernel, Func<HttpClient> httpClientFactory)
        {
            targetKernel.UseGameEngine(httpClientFactory);
            Engine.Instance.RegisterKernel(targetKernel);
            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);
            return Task.CompletedTask;
        }
    }
}
