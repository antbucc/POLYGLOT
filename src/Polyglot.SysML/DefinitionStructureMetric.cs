using Microsoft.DotNet.Interactive.Events;
using SysML.Interactive;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polyglot.Metrics.SysML
{
    public class DefinitionStructureMetric
    {
        public string Name => "definitionStructure";

        public IEnumerable<SysMLElement> Calculate(List<KernelEvent> events)
        {
            var sysMLInteractiveResult = events.OfType<ReturnValueProduced>().FirstOrDefault()?.Value as SysMLInteractiveResult;
            return sysMLInteractiveResult?.Content ?? new List<SysMLElement>();
        }
        
        public Task<IEnumerable<SysMLElement>> CalculateAsync(List<KernelEvent> events)
        {
            return Task.FromResult(Calculate(events));
        }
    }
}
