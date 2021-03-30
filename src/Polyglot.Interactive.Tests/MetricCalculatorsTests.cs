using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using Polyglot.Interactive.Contracts;
using Xunit;
using System.Collections.Generic;

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

        [Fact]
        public async Task can_find_declared_methods()
        {
            var metric = new DeclaredMethodsMetric();

            var command = new SubmitCode(@"
public void IlVulcano() {
    Console.WriteLine(""IL VULCANO NOOOOOOOOOOO"");
}

public void InTheMassachusetts() {} // che e' lo spelling corretto
}");

            var values = (await metric.CalculateAsync(command)) as string[];

            values.Should()
                .NotBeNullOrEmpty()
                .And
                .BeEquivalentTo("IlVulcano", "InTheMassachusetts");
        }

        [Fact]
        public async Task can_get_class_structure()
        {
            var metric = new ClassesStructureMetric();

            var command = new SubmitCode(@"
public class Triangle
{
    private float _base;
    private float _height;

    public Triangle(float b, float h)
    {
        _base = b;
        _height = h;
    }

    public float Area() {
        return _base * _height / 2;
    }
}");

            var floatVar = new VariableStructure("_", "float");
            var _base = new FieldStructure(floatVar with { Name = "_base" }, "private");
            var _height = new FieldStructure(floatVar with { Name = "_height" }, "private");
            var Area = new MethodStructure("Area", "float", "public", new List<VariableStructure>());

            var expected = new ClassStructure(
                    "Triangle",
                    "public",
                    new List<FieldStructure> { _base, _height },
                    new List<MethodStructure> { Area }
                );

            var values = (await metric.CalculateAsync(command)) as ClassStructure[];

            values.Should()
                .NotBeNullOrEmpty()
                .And
                .HaveCount(1);

            values[0].Should().BeEquivalentTo(expected,
                options => options.ComparingByMembers<ClassStructure>().ComparingByMembers<MethodStructure>());
        }


        public class Triangle
        {
            private float _base;
            private float _height;

            public Triangle(float b, float h)
            {
                _base = b;
                _height = h;
            }

            public float Area(float mhanzolini)
            {
                return _base * _height / 2;
            }
            
        }
    }
}