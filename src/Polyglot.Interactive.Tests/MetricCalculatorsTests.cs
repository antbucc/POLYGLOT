using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Polyglot.CSharp;

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
            var metric = new TopLevelClassesStructureMetric();

            var command = new SubmitCode(@"
public class Triangle
{
    private float _height;
    private float _base;

    public Triangle(float b, float h)
    {
        _base = b;
        _height = h;
    }

    public float calculateArea() {
        return _base * _height / 2;
    }
}");

            var _base = new FieldStructure(new VariableStructure("_base", DeclarationContextKind.Type, "float"), new[] { "private" });
            var _height = new FieldStructure(new VariableStructure("_height", DeclarationContextKind.Type, "float"), new[] { "private" });
            var _constructor = new ConstructorStructure(
                new[] {
                    "public"
                },
                new[] { 
                    new VariableStructure("b", DeclarationContextKind.Method, "float"), 
                    new VariableStructure("h", DeclarationContextKind.Method, "float") 
                }
            );
            var calculateArea = new MethodStructure(
                "calculateArea",
                DeclarationContextKind.Type,
                "float", 
                new[] { "public" },
                new List<VariableStructure>(),
                new MethodBodyStructure(
                    new List<VariableStructure>()
                )
            );
            var expected = new ClassStructure(
                    "Triangle",
                    DeclarationContextKind.TopLevel,
                    new[] { "public" },
                    new[] { _base, _height },
                    new List<PropertyStructure>(),
                    new[] { calculateArea },
                    new[] { _constructor },
                    new List<ClassStructure>()
                );

            var values = (await metric.CalculateAsync(command)) as IEnumerable<ClassStructure>;

            values.Should()
                .NotBeNullOrEmpty()
                .And
                .HaveCount(1);

            values.Where(c => c.Name == "Triangle").FirstOrDefault().Should()
                .NotBeNull()
                .And
                .BeEquivalentTo(expected, 
                                options => options.ComparingByMembers<ClassStructure>().ComparingByMembers<MethodStructure>().ComparingByMembers<ConstructorStructure>());
        }

        [Fact]
        public async Task can_find_declared_properties()
        {
            var metric = new TopLevelClassesStructureMetric();

            var command = new SubmitCode(@"
public class Test
{
    public float PropertyF { get; set; }
    private string PropertyS => ""test"";
    public int PropertyI { set; }

    private double _privateField;
    public double PropertyD
    {
        get { return _privateField / 3600; }
        set { _privateField = value * 5; }
    }

}
");

            var expected = new[]
            {
                new PropertyStructure(new VariableStructure("PropertyF", DeclarationContextKind.Type, "float"), new[] { "public" }, new[] { "get", "set" }),
                new PropertyStructure(new VariableStructure("PropertyS", DeclarationContextKind.Type, "string"), new[] { "private" }, new[] { "get" }),
                new PropertyStructure(new VariableStructure("PropertyI", DeclarationContextKind.Type, "int"), new[] { "public" }, new[] { "set" }),
                new PropertyStructure(new VariableStructure("PropertyD", DeclarationContextKind.Type, "double"), new[] { "public" }, new[] { "get", "set" })
            };

            var values = (await metric.CalculateAsync(command)) as IEnumerable<ClassStructure>;

            values.Should()
                .NotBeNullOrEmpty()
                .And
                .HaveCount(1);

            var actual = values.Where(c => c.Name == "Test");

            actual.Should()
                .HaveCount(1)
                .And
                .Subject.First().As<ClassStructure>().Properties.Should()
                .NotBeNullOrEmpty()
                .And
                .BeEquivalentTo(expected);
        }

        [Fact]
        public async Task can_get_toplevel_variables()
        {
            var metric = new DeclarationsStructureMetric();

            var command = new SubmitCode(@"
string name = ""Domenico Bini"";
");

            var expected = new FieldStructure(new VariableStructure("name", DeclarationContextKind.TopLevel, "string"), new List<string>());

            var value = (await metric.CalculateAsync(command)) as ClassStructure;

            value.Should()
                .NotBeNull()
                .And
                .Subject.As<ClassStructure>().Fields.Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public async Task can_get_toplevel_methods_with_local_variables()
        {
            var metric = new DeclarationsStructureMetric();

            var command = new SubmitCode(@"
void printDomenicoBini()
{
    string name = ""Domenico Bini"";
    System.Console.WriteLine(name);
}
");

            var expected = new MethodStructure(
                "printDomenicoBini",
                DeclarationContextKind.TopLevel,
                "void",
                new List<string>(),
                new List<VariableStructure>(),
                new MethodBodyStructure(
                    new[]
                    {
                        new VariableStructure("name", DeclarationContextKind.Method, "string")
                    }
                )
            );

            var value = (await metric.CalculateAsync(command)) as ClassStructure;

            value.Should()
                .NotBeNull()
                .And
                .Subject.As<ClassStructure>().Methods.Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public async Task can_get_toplevel_variables_methods_and_classes()
        {
            var metric = new DeclarationsStructureMetric();

            var command = new SubmitCode(@"
class Schana {}

float input = 4;

float square(float x)
{
    return x*x;
}

square(input);
");

            var expectedField = new FieldStructure(new VariableStructure("input", DeclarationContextKind.TopLevel, "float"), new List<string>());

            var expectedMethod = new MethodStructure(
                "square",
                DeclarationContextKind.TopLevel,
                "float",
                new List<string>(),
                new[]
                {
                    new VariableStructure("x", DeclarationContextKind.Method, "float")
                },
                new MethodBodyStructure(
                    new List<VariableStructure>()
                )
            );
            
            var expectedClass = new ClassStructure(
                    "Schana",
                    DeclarationContextKind.TopLevel,
                    new List<string>(),
                    new List<FieldStructure>(),
                    new List<PropertyStructure>(),
                    new List<MethodStructure>(),
                    new List<ConstructorStructure>(),
                    new List<ClassStructure>()
                );

            var root = (await metric.CalculateAsync(command)) as ClassStructure;

            root.Should()
                .NotBeNull();

            root.Fields.Should()
                .BeEquivalentTo(expectedField);

            root.Methods.Should()
                .BeEquivalentTo(expectedMethod);

            root.NestedClasses.Should()
                .BeEquivalentTo(expectedClass);
        }
    }
}