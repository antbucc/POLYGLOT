using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Polyglot.Interactive
{
    public class DeclaredClassesMetric : IMetricCalculator
    {
        public string Name { get; } = "declaredClasses";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null,
            IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            throw new NotImplementedException();
        }
    }
}