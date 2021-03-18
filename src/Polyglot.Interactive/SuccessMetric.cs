using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Polyglot.Interactive
{
    public class SuccessMetric : IMetricCalculator
    {
        public string Name { get; } = "success";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null,
            List<KernelEvent> events = null, IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default,
            DateTime? lastRun = null)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            return Task.FromResult<object>(events.FirstOrDefault(e => e is CommandFailed) is null);
        }
    }
}