using System;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.FSharp;
using Xunit.Abstractions;

namespace Pocket
{
    internal partial class LogEvents
    {
        public static IDisposable SubscribeToPocketLogger(this ITestOutputHelper output) =>
            Subscribe(
                e => output.WriteLine(e.ToLogString()),
                new[]
                {
                    typeof(LogEvents).Assembly,
                    typeof(KernelEvent).Assembly,
                    typeof(CSharpKernel).Assembly,
                    typeof(FSharpKernel).Assembly,
                });
    }
}