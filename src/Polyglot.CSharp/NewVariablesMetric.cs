using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.CSharp
{
    public class NewVariablesMetric : IMetricCalculator
    {
        public string Name { get; } = "newVariables";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null,
            List<KernelEvent> events = null, IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default,
            DateTime? lastRun = null)
        {
            if (newVariables == null)
            {
                throw new ArgumentNullException(nameof(newVariables));
            }

            return Task.FromResult<object>(newVariables);
        }
    }
}