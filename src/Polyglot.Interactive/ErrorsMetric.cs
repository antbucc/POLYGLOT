using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.Interactive
{
    public class ErrorsMetric : IMetricCalculator
    {
        public string Name { get; } = "errors";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null,
            List<KernelEvent> events = null, IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default,
            DateTime? lastRun = null)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            return Task.FromResult<object>(events.OfType<DiagnosticsProduced>().SelectMany(d => d.Diagnostics).Count(d => d.Severity == DiagnosticSeverity.Error));
        }
    }
}