using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Polyglot.CSharp
{
    public class DeclarationsStructureMetric
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "declarationsStructure";

        public DeclarationsStructureMetric(SourceCodeKind sourceCodeKind = SourceCodeKind.Script)
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(sourceCodeKind);
        }

        public object Calculate(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code, _parserOptions);
            var declarationWalker = new DeclarationWalker();

            declarationWalker.Visit(tree.GetRoot());

            return declarationWalker.DeclarationsRoot;
        }

        public Task<object> CalculateAsync(string code)
        {
            return Task.FromResult<object>(Calculate(code));
        }
    }
}