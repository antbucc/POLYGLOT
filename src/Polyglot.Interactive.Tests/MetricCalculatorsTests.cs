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
public class Maremma {}");
            var values = await metric.CalculateAsync(command);
            
            values.Should().Be(1);
        }
    }
}