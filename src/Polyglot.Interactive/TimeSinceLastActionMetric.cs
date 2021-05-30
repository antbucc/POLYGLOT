using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.Interactive
{
    public class TimeSinceLastActionMetric : IMetricCalculator
    {
        public string Name { get; } = "timeSinceLastAction";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null,
            List<KernelEvent> events = null, IReadOnlyDictionary<string, object> newVariables = null, TimeSpan runTime = default,
            DateTime? lastRun = null)
        {
            return Task.FromResult<object>(lastRun is not null ? (DateTime.Now - lastRun.Value).TotalMilliseconds : 0);
        }
    }
}