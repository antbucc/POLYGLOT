using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Polyglot.Metrics.CSharp
{
    public class DeclaredMethodsMetric
    {
        private readonly CSharpParseOptions _parserOptions;

        public string Name => "declaredMethods";

        public DeclaredMethodsMetric(SourceCodeKind sourceCodeKind = SourceCodeKind.Script)
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(sourceCodeKind);
        }

        public string[] Calculate(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code, _parserOptions);
            var methodWalker = new MethodDeclarationWalker();

            methodWalker.Visit(tree.GetRoot());
            return methodWalker.DeclaredMethods.ToArray();
        }

        public Task<string[]> CalculateAsync(string code)
        {
            return Task.FromResult(Calculate(code));
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