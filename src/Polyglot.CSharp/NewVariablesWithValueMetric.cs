using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.CSharp
{
    public class NewVariablesWithValueMetric : IMetricCalculator
    {
        public string Name { get; } = "newVariablesWithValue";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null,
            List<KernelEvent> events = null, IReadOnlyDictionary<string, object> newVariables = null, TimeSpan runTime = default,
            DateTime? lastRun = null)
        {
            if (newVariables == null)
            {
                throw new ArgumentNullException(nameof(newVariables));
            }

            var result = newVariables.Select(v => new { Name = v.Key, Type = v.Value?.GetType().Name ?? "null", Value = v.Value });

            return Task.FromResult<object>(result);
        }
    }
}