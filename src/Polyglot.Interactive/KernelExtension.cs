using System;
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

        Formatter.Register<GameStateReport>((report, writer) =>
            {
                var color = report.Points > 0 ? "green" : "red";

                string scoreImage = (int)report.Points switch
                {
                    int n when (0 <= n && n <= 15) => "https://img-premium.flaticon.com/png/512/132/132268.png?token=exp=1621862049~hmac=98ca862d52589f9f24a042d968629476",
                    int n when (15 < n && n <= 30) => "https://img-premium.flaticon.com/png/512/132/132272.png?token=exp=1621862102~hmac=1f9149d59b300dcee8d56480655d9c98",
                    int n when (30 < n && n <= 50) => "https://img-premium.flaticon.com/png/512/132/132233.png?token=exp=1621862133~hmac=782c516c9a4d23fb6397f7d6a69ea932",
                    int n when (50 < n && n <= 75) => "https://img-premium.flaticon.com/png/512/132/132269.png?token=exp=1621862135~hmac=5421b42cf5d63ddefe2dec47d1e70597",
                    int n when (75 < n && n <= 100) => "https://img-premium.flaticon.com/png/512/132/132250.png?token=exp=1621862139~hmac=92483f00db5c501effec203fd5534ba7",
                    _ => "https://img-premium.flaticon.com/png/512/132/132250.png?token=exp=1621862139~hmac=92483f00db5c501effec203fd5534ba7"
                };

                var feedbackDisplay = report.Feedback == "" ? "display:none" : "";
                var divStyle = "font-size: 2em; display: flex; justify-content: center; align-items: center";

                var html = div[style: "width:800px; border: 1px solid black;"](
                    table(
                        tr(
                            td[style: "width: 50px"]("Level:"), td[style: "width:150px"](div[style: divStyle](report.CurrentLevel)),
                            td[style: "width: 50px"]("Score:"), td[style: "width:200px"](div[style: divStyle](img[src: scoreImage, style: "height:2em"])),
                            td[style: "width: 50px"]("Coins:"), td[style: "width:250px"](div[style: divStyle]($"{report.GoldCoins} x", img[src: "https://www.iconpacks.net/icons/1/free-icon-coin-794.png", style: "margin-left: 5px; height:2em"]))
                        ),
                        tr[style: feedbackDisplay](
                            td[style: "width: 50px"]("Feedback"),
                            td["colspan='5'"](report.Feedback)
                        )
                    )
                );
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);
            
            // TODO: credits to artist
            // <div>Icons made by <a href="https://www.flaticon.com/authors/baianat" title="Baianat">Baianat</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a></div>

            return Task.CompletedTask;
        }
    }
}
