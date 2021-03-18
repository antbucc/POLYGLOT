using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using Xunit;

namespace Polyglot.Interactive.Tests
{
    public class MetricCalculatorsTests
    {
        [Fact]
        public async Task can_find_declared_classes()
        {
            var metric = new DeclaredClassesMetric();
            
            var command = new SubmitCode(@"
public class Schana {}

public class Wana {}

public class Blues {}");

            var values = (await metric.CalculateAsync(command)) as string[];
            
            values.Should()
                .NotBeNullOrEmpty()
                .And
                .BeEquivalentTo("Schana", "Wana", "Blues");
        }
    }
}