using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Polyglot.CSharp
{
    public class DeclaredMethodsMetric
    {
        private readonly CSharpParseOptions _parserOptions;

        public string Name => "declaredMethods";

        public DeclaredMethodsMetric(SourceCodeKind sourceCodeKind = SourceCodeKind.Script)
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(sourceCodeKind);
        }

        public object Calculate(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code, _parserOptions);
            var methodWalker = new MethodDeclarationWalker();

            methodWalker.Visit(tree.GetRoot());
            return Task.FromResult<object>(methodWalker.DeclaredMethods.ToArray());
        }

        public Task<object> CalculateAsync(string code)
        {
            return Task.FromResult<object>(Calculate(code));
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