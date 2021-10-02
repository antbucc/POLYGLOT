using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Polyglot.Core;

using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace Polyglot.Interactive
{
    public class KernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            return RegisterFormatters();
        }

        private Task RegisterFormatters()
        {
            Formatter.Register<GameStateReport>((report, writer) =>
            {
                var scoreEmoji= (int)report.AssignmentPoints switch
                {

                    var n when (0 <= n && n <= 10) => "🙂",
                    var n when (10 < n && n <= 20) => "😊",
                    var n when (20 < n && n <= 30) => "🤗",
                    var n when (30 < n && n <= 40) => "😍",
                    var n when (40 < n && n <= 50) => "🤩",
                    _ => "😑"
                };

                var feedbackDisplay = report.Feedbacks.Count() == 0 ? "display:none" : "";
                var feedbacks = report.Feedbacks.Select(f =>
                    tr[style: feedbackDisplay](
                        // td[style: "width: 50px"]("Feedback"),
                        td["colspan='8'"](f)
                    )
                );

                var divStyle = "font-size: 2em; display: flex; justify-content: center; align-items: center";
                var flames = string.Join("", Enumerable.Range(0, (int)report.AssignmentGoldCoins).Select(_ => "🥇"));
                var html = div[style: "width:800px; border: 1px solid black; padding: 5px"](
                    h1[style: "margin-left: 10px"]("Report"),
                    table(
                        tr(
                            td[style: "width: 50px"]("Level:"), td[style: "width:150px"](div[style: divStyle](report.CurrentLevel)),
                            td[style: "width: 50px"]("Exercise Points:"), td[style: "width:150px"](div[style: divStyle](report.ExercisePoints)),
                            td[style: "width: 50px"]("Assignment Score:"), td[style: "width:150px"](p[style: "font-size:3em"](scoreEmoji)),
                            td[style: "width: 150px"]("Medals:"), td[style: "width:150px"](p[style: "font-size:3em"](flames))
                        )
                    ),
                    h2[style: ("margin-left: 10px;" + feedbackDisplay)]("Feedbacks"),
                    table(
                        feedbacks
                    )
                );
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

            return Task.CompletedTask;
        }
    }
}
