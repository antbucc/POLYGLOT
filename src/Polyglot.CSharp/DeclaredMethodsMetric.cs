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
    public class DeclaredMethodsMetric : IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;

        public string Name => "declaredMethods";

        public DeclaredMethodsMetric()
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);
        }

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null, IReadOnlyDictionary<string, object> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            var tree =
                CSharpSyntaxTree.ParseText(command.Code, _parserOptions);

            var methodWalker = new MethodDeclarationWalker();

            methodWalker.Visit(tree.GetRoot());

            return Task.FromResult<object>(methodWalker.DeclaredMethods.ToArray());
        }

        private class MethodDeclarationWalker : CSharpSyntaxWalker
        {
            private readonly HashSet<string> _declaredMethods = new();

            public IEnumerable<string> DeclaredMethods => _declaredMethods;

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                _declaredMethods.Add(node.Identifier.ValueText);
                base.VisitMethodDeclaration(node);
            }
        }
    }
}