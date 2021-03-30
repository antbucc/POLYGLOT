using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Interactive.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Polyglot.Interactive
{
    public class ClassesStructureMetric : IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "declaredMethods";

        public ClassesStructureMetric()
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);
        }

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null, IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            var tree =
                CSharpSyntaxTree.ParseText(command.Code, _parserOptions);

            var classWalker = new ClassWalker();

            classWalker.Visit(tree.GetRoot());

            return Task.FromResult<object>(classWalker.ClassesStructure.ToArray());
        }

        private class ClassWalker : CSharpSyntaxWalker
        {
            private List<FieldStructure> _fields;
            private List<MethodStructure> _methods;
            private string _name;
            private string _accessModifier;

            private readonly HashSet<ClassStructure> _classesStructure = new();

            public IEnumerable<ClassStructure> ClassesStructure => _classesStructure;

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                _name = node.Identifier.ValueText;
                _accessModifier = node.Modifiers[0].ValueText;
                _fields = new();
                _methods = new();

                base.VisitClassDeclaration(node);

                var @class = new ClassStructure(_name, _accessModifier, _fields.AsReadOnly(), _methods.AsReadOnly());
                _classesStructure.Add(@class);
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                var type = node.Declaration.Type.ToString();
                var accessModifier = node.Modifiers[0].ValueText;
                foreach (var variable in node.Declaration.Variables)
                {
                    var declaredVariable = new VariableStructure(variable.Identifier.ValueText, type);
                    var field = new FieldStructure(declaredVariable, accessModifier);

                    _fields.Add(field);
                }

                base.VisitFieldDeclaration(node);
            }

            //public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            //{
            //    base.VisitPropertyDeclaration(node);
            //}

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var returnType = node.ReturnType.ToString();
                var accessModifier = node.Modifiers[0].ValueText;
                var name = node.Identifier.ValueText;

                var parameters = new List<VariableStructure>();

                foreach (var parameter in node.ParameterList.Parameters)
                {
                    var declaredVariable = new VariableStructure(parameter.Identifier.ValueText, parameter.Type.ToFullString());
                    parameters.Add(declaredVariable);
                }

                var method = new MethodStructure(name, returnType, accessModifier, parameters.AsReadOnly());
                _methods.Add(method);
                base.VisitMethodDeclaration(node);
            }
        }

    }
}