using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Formatting;
using Polyglot.Gamification;
using SysML.Interactive;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace Polyglot.Interactive
{
    public class KernelExtension : IKernelExtension
    {
        public Task OnLoadAsync(Kernel kernel)
        {
            (Kernel.Root as CompositeKernel).UseSysML();
            return Task.CompletedTask;
            //return RegisterFormattersAsync();
        }

        //private Task RegisterFormattersAsync()
        //{
        //    SysMLKernelExtension.RegisterFormatters();
        //    Formatter.Register<GameStateReport>((report, writer) =>
        //    {
        //        var scoreEmoji = (int)report.AssignmentPoints switch
        //        {

        //            var n when (0 <= n && n <= 10) => "🙂",
        //            var n when (10 < n && n <= 20) => "😊",
        //            var n when (20 < n && n <= 30) => "🤗",
        //            var n when (30 < n && n <= 40) => "😍",
        //            var n when (40 < n && n <= 50) => "🤩",
        //            _ => "😑"
        //        };

        //        var feedbackDisplay = report.Feedbacks.Count() == 0 ? "display:none" : "";
        //        var feedbacks = report.Feedbacks.Select(f =>
        //            tr[style: feedbackDisplay](
        //                // td[style: "width: 50px"]("Feedback"),
        //                td["colspan='8'"](f)
        //            )
        //        );

        //        var divStyle = "font-size: 2em; display: flex; justify-content: center; align-items: center";
        //        var flames = string.Join("", Enumerable.Range(0, (int)report.AssignmentGoldCoins).Select(_ => "🥇"));
        //        var html = div[style: "width:800px; border: 1px solid black; padding: 5px"](
        //            h1[style: "margin-left: 10px"]("Report"),
        //            table(
        //                tr(
        //                    td[style: "width: 50px"]("Level:"), td[style: "width:150px"](div[style: divStyle](report.CurrentLevel)),
        //                    td[style: "width: 50px"]("Exercise Points:"), td[style: "width:150px"](div[style: divStyle](report.ExercisePoints)),
        //                    td[style: "width: 50px"]("Assignment Score:"), td[style: "width:150px"](p[style: "font-size:3em"](scoreEmoji)),
        //                    td[style: "width: 150px"]("Medals:"), td[style: "width:150px"](p[style: "font-size:3em"](flames))
        //                )
        //            ),
        //            h2[style: ("margin-left: 10px;" + feedbackDisplay)]("Feedbacks"),
        //            table(
        //                feedbacks
        //            )
        //        );
        //        writer.Write(html);
        //    }, HtmlFormatter.MimeType);

        //    Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

        //    return Task.CompletedTask;
        //}

        public static Task RegisterFormattersCSharpAsync()
        {
            Formatter.Register<GameStatus>((report, writer) =>
            {
                var badgesDisplay = report.State.BadgeConcept.Count() == 0 ? "display:none;" : "";
                var badges = report.State.BadgeConcept.Where(b => b.Name == "C#")
                                                                        .SelectMany(b => b.BadgeEarned)
                                                                        .Select(b =>
                    tr[style: badgesDisplay](
                        td["colspan='8'"](b)
                    )
                );

                var exerciseScore = report.State.PointConcept.Where(p => p.Name == "assignmentPointsC").FirstOrDefault()?.Score ?? 0;
                var competencyScore = report.State.PointConcept.Where(p => p.Name == "competencePointsC").FirstOrDefault()?.Score ?? 0;

                var divStyle = "font-size: 2em; display: flex; justify-content: center; align-items: center";
                var html = div[style: "width:400px; border: 1px solid black; padding: 10px; padding-bottom: 25px"](
                    h1[style: "margin-left: 10px"]("Report"),
                    table(
                        tr(
                            td[style: "width: 50px"]("Assignment Points:"), td[style: "width:150px"](div[style: divStyle](exerciseScore)),
                            td[style: "width: 50px"]("Competency Points:"), td[style: "width:150px"](div[style: divStyle](competencyScore))
                        )
                    ),
                    h2[style: ("margin-left: 10px; " + badgesDisplay)]("Badges"),
                    table[style: "margin-left: 20px"](
                        badges
                    )
                );
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

            return Task.CompletedTask;
        }

        public static Task RegisterFormattersSysMLAsync()
        {
            SysMLKernelExtension.RegisterFormatters();
            Formatter.Register<GameStatus>((report, writer) =>
            {
                var badgesDisplay = report.State.BadgeConcept.Count() == 0 ? "display:none;" : "";
                var badges = report.State.BadgeConcept.Where(b => b.Name == "SYSML")
                                                                        .SelectMany(b => b.BadgeEarned)
                                                                        .Select(b =>
                    tr[style: badgesDisplay](
                        td["colspan='8'"](b)
                    )
                );

                var exerciseScore = report.State.PointConcept.Where(p => p.Name == "assignmentPointsS").FirstOrDefault()?.Score ?? 0;
                var competencyScore = report.State.PointConcept.Where(p => p.Name == "competencePointsS").FirstOrDefault()?.Score ?? 0;

                var divStyle = "font-size: 2em; display: flex; justify-content: center; align-items: center";
                var html = div[style: "width:400px; border: 1px solid black; padding: 10px; padding-bottom: 25px"](
                    h1[style: "margin-left: 10px"]("Report"),
                    table(
                        tr(
                            td[style: "width: 50px"]("Assignment Points:"), td[style: "width:150px"](div[style: divStyle](exerciseScore)),
                            td[style: "width: 50px"]("Competency Points:"), td[style: "width:150px"](div[style: divStyle](competencyScore))
                        )
                    ),
                    h2[style: ("margin-left: 10px; " + badgesDisplay)]("Badges"),
                    table[style: "margin-left: 20px"](
                        badges
                    )
                );
                writer.Write(html);
            }, HtmlFormatter.MimeType);

            Formatter.SetPreferredMimeTypeFor(typeof(GameStateReport), HtmlFormatter.MimeType);

            return Task.CompletedTask;
        }
    }
}
