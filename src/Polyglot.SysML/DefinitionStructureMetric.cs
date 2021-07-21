using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;
using SysML.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polyglot.SysML
{
    public class DefinitionStructureMetric : IMetricCalculator
    {
        public string Name => "definitionStructure";

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null, IReadOnlyDictionary<string, object> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            return Task.FromResult((object)(events.OfType<ReturnValueProduced>().FirstOrDefault()?.Value as SysMLInteractiveResult).Content);
        }
    }
}
