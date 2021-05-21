using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Polyglot.Core;
using Polyglot.CSharp;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

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
            

            Formatter.Register<GameStateReport>(( report,  writer)=>
            {
                var color = report.Points > 0 ? "green" : "red";
                var coins = Enumerable.Range(0, (int)report.GoldCoins)
                    .Select(_ => img[src: "https://upload.wikimedia.org/wikipedia/commons/d/d6/Gold_coin_icon.png", style: "height:1.5em"]);
                var html = div[style: "width:100px"](
                    table(
                        tr(td("Current Level"), td[style: $"color:{color}; width:100px"](report.CurrentLevel)),
                        tr(td("Coins"), td[style: $"color:{color}"](coins))
                    )
                );
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

            return Task.CompletedTask;
        }
    }
}
