using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Polyglot.Metrics.CSharp
{
    public class DeclaredClassesMetric
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "declaredClasses";

        public DeclaredClassesMetric(SourceCodeKind sourceCodeKind = SourceCodeKind.Script)
        {
           _parserOptions = CSharpParseOptions.Default.WithKind(sourceCodeKind);
        }

        public string[] Calculate(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code, _parserOptions);
            var walkerTexasRanger = new ClassDeclarationWalker();

            walkerTexasRanger.Visit(tree.GetRoot());
            return walkerTexasRanger.DeclaredClasses.ToArray();
        }

        public Task<string[]> CalculateAsync(string code)
        {
            return Task.FromResult(Calculate(code));
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