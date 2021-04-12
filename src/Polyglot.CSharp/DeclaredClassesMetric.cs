using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.CSharp
{
    public class DeclaredClassesMetric :  IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name { get; } = "declaredClasses";

        public DeclaredClassesMetric()
        {
           _parserOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);
        }

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null,
            IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            var tree =
                CSharpSyntaxTree.ParseText(command.Code, _parserOptions);

            var walkerTexasRanger = new ClassDeclarationWalker();

            walkerTexasRanger.Visit(tree.GetRoot());
            
            return Task.FromResult<object>(walkerTexasRanger.DeclaredClasses.ToArray());
        }

        private class ClassDeclarationWalker : CSharpSyntaxWalker
        {
            private readonly HashSet<string> _declaredClasses = new();

            public IEnumerable<string> DeclaredClasses => _declaredClasses;

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                _declaredClasses.Add(node.Identifier.ValueText);
                base.VisitClassDeclaration(node);
            }
        }
    }
}