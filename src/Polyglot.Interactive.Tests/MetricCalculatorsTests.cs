using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Commands;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Polyglot.CSharp;
using Polyglot.Core;

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

            var _base = new FieldStructure(new VariableStructure(new CodeString("_base", new StringSpan(1, 2)), DeclarationContextKind.Type, new CodeString("float", new StringSpan(1, 2))), new[] { new CodeString("private", new StringSpan(1, 2)) });
            var _height = new FieldStructure(new VariableStructure(new CodeString("_height", new StringSpan(1, 2)), DeclarationContextKind.Type, new CodeString("float", new StringSpan(1, 2))), new[] { new CodeString("private", new StringSpan(1, 2)) });
            var _constructor = new ConstructorStructure(
                new[] {
                    new CodeString("public", new StringSpan(1, 2))
                },
                new[] { 
                    new VariableStructure(new CodeString("b", new StringSpan(1, 2)), DeclarationContextKind.Method, new CodeString("float", new StringSpan(1, 2))), 
                    new VariableStructure(new CodeString("h", new StringSpan(1, 2)), DeclarationContextKind.Method, new CodeString("float", new StringSpan(1, 2))) 
                }
            );
            var calculateArea = new MethodStructure(
                new CodeString("calculateArea", new StringSpan(1, 2)),
                DeclarationContextKind.Type,
                new CodeString("float", new StringSpan(1, 2)), 
                new[] { new CodeString("public", new StringSpan(1, 2)) },
                new List<VariableStructure>(),
                new MethodBodyStructure(
                    new List<VariableStructure>()
                )
            );
            var expected = new ClassStructure(
                    new CodeString("Triangle", new StringSpan(1, 2)),
                    DeclarationContextKind.TopLevel,
                    new[] { new CodeString("public", new StringSpan(1, 2)) },
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

            values.Where(c => c.Name.Value == "Triangle").FirstOrDefault().Should()
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

    private string _name;  // the name field
    public string Name    // the Name property
    {
        get => _name;
        set => _name = value;
    }

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
                new PropertyStructure(new VariableStructure(new CodeString("PropertyF", new StringSpan(41, 50)), DeclarationContextKind.Type, new CodeString("float", new StringSpan(35, 40))), new[] { new CodeString("public", new StringSpan(28, 34)) }, new[] { new CodeString("get", new StringSpan(53, 56)), new CodeString("set", new StringSpan(57, 60)) }),
                new PropertyStructure(new VariableStructure(new CodeString("PropertyS", new StringSpan(85, 94)), DeclarationContextKind.Type, new CodeString("string", new StringSpan(78, 84))), new[] { new CodeString("private", new StringSpan(70, 77)) }, new[] { new CodeString("get", new StringSpan(98, 106)) }),
                new PropertyStructure(new VariableStructure(new CodeString("PropertyI", new StringSpan(122, 131)), DeclarationContextKind.Type, new CodeString("int", new StringSpan(118, 121))), new[] { new CodeString("public", new StringSpan(111, 117)) }, new[] { new CodeString("set", new StringSpan(134, 137)) }),
                new PropertyStructure(new VariableStructure(new CodeString("Name", new StringSpan(208, 212)), DeclarationContextKind.Type, new CodeString("string", new StringSpan(201, 207))), new[] { new CodeString("public", new StringSpan(194, 200)) }, new[] { new CodeString("get", new StringSpan(253, 256)), new CodeString("set", new StringSpan(276, 279)) }),
                new PropertyStructure(new VariableStructure(new CodeString("PropertyD", new StringSpan(361, 370)), DeclarationContextKind.Type, new CodeString("double", new StringSpan(354, 360))), new[] { new CodeString("public", new StringSpan(347, 353)) }, new[] { new CodeString("get", new StringSpan(387, 390)), new CodeString("set", new StringSpan(433, 436)) })
            };

            var values = (await metric.CalculateAsync(command)) as IEnumerable<ClassStructure>;

            values.Should()
                .NotBeNullOrEmpty()
                .And
                .HaveCount(1);

            var actual = values.Where(c => c.Name.Value == "Test");

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

            var expected = new FieldStructure(new VariableStructure(new CodeString("name", new StringSpan(1, 2)), DeclarationContextKind.TopLevel, new CodeString("string", new StringSpan(1, 2))), new List<CodeString>());

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
                new CodeString("printDomenicoBini", new StringSpan(1, 2)),
                DeclarationContextKind.TopLevel,
                new CodeString("void", new StringSpan(1, 2)),
                new List<CodeString>(),
                new List<VariableStructure>(),
                new MethodBodyStructure(
                    new[]
                    {
                        new VariableStructure(new CodeString("name", new StringSpan(1, 2)), DeclarationContextKind.Method, new CodeString("string", new StringSpan(1, 2)))
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

            var expectedField = new FieldStructure(new VariableStructure(new CodeString("input", new StringSpan(1, 2)), DeclarationContextKind.TopLevel, new CodeString("float", new StringSpan(1, 2))), new List<CodeString>());

            var expectedMethod = new MethodStructure(
                new CodeString("square", new StringSpan(1, 2)),
                DeclarationContextKind.TopLevel,
                new CodeString("float", new StringSpan(1, 2)),
                new List<CodeString>(),
                new[]
                {
                    new VariableStructure(new CodeString("x", new StringSpan(1, 2)), DeclarationContextKind.Method, new CodeString("float", new StringSpan(1, 2)))
                },
                new MethodBodyStructure(
                    new List<VariableStructure>()
                )
            );
            
            var expectedClass = new ClassStructure(
                    new CodeString("Schana", new StringSpan(1, 2)),
                    DeclarationContextKind.TopLevel,
                    new List<CodeString>(),
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