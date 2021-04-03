using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Interactive.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Polyglot.Interactive
{
    public class ClassesStructureMetric : IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "classesStructure";

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
            private readonly Dictionary<string, ClassDeclarationContext> _classesStructure = new();
            public IEnumerable<ClassStructure> ClassesStructure =>
                _classesStructure.Values.Select(c => new ClassStructure(c.Name, c.Modifiers, c.Fields, c.Methods, c.Constructors));

            private Stack<ClassDeclarationContext> _classDeclarationStack = new();
            private ClassDeclarationContext _current;

            private class ClassDeclarationContext
            {
                public string Name;
                public HashSet<string> Modifiers = new();
                public HashSet<FieldStructure> Fields = new();
                public HashSet<MethodStructure> Methods = new();
                public HashSet<ConstructorStructure> Constructors = new();
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // get/set class dictionary + stack push
                var className = node.Identifier.ValueText;
                var classDeclaration = new ClassDeclarationContext();
                if(_classesStructure.ContainsKey(className))
                {
                    classDeclaration = _classesStructure[className];
                }
                if(_current is not null)
                {
                    _classDeclarationStack.Push(_current);
                }
                _current = classDeclaration;
                _classesStructure[className] = _current;

                _current.Name = className;
                _current.Modifiers.UnionWith(node.Modifiers.Select(m => m.ValueText));

                base.VisitClassDeclaration(node);


                if (_classDeclarationStack.Count != 0)
                {
                    _current = _classDeclarationStack.Pop();
                } 
                else
                {
                    _current = null;
                }
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                //if(node.Declaration.IsKind(SyntaxKind.FieldDeclaration))
                //{
                var type = node.Declaration.Type.ToString();

                var modifiers = node.Modifiers.Select(m => m.ValueText);
                    
                _current?.Fields.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier.ValueText)
                                                                            .Select(name => new VariableStructure(name, type))
                                                                            .Select(@var => new FieldStructure(@var, modifiers)));
                //} 

                base.VisitFieldDeclaration(node);
            }

            //public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            //{
            //    base.VisitPropertyDeclaration(node);
            //}


            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                //Debugger.Launch();
                var name = node.Identifier.ValueText;
                var returnType = node.ReturnType.ToString();
                var modifiers = node.Modifiers.Select(m => m.ValueText);

                var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(param.Identifier.ValueText, param.Type.ToString()));

                var method = new MethodStructure(name, returnType, modifiers, parameters);
                _current?.Methods.Add(method);

                base.VisitMethodDeclaration(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(param.Identifier.ValueText, param.Type.ToString()));

                var constructor = new ConstructorStructure(parameters);
                _current?.Constructors.Add(constructor);

                base.VisitConstructorDeclaration(node);
            }
        }

    }
}