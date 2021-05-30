using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Polyglot.Core;

namespace Polyglot.CSharp
{
    public class TopLevelClassesStructureMetric : IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "topLevelClassesStructureMetric";

        public TopLevelClassesStructureMetric()
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);
        }

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null, IReadOnlyDictionary<string, object> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            var tree =
                CSharpSyntaxTree.ParseText(command.Code, _parserOptions);

            //var declarationWalker = new ClassWalker();
            var declarationWalker = new DeclarationWalker();

            declarationWalker.Visit(tree.GetRoot());

            var root = declarationWalker.DeclarationsRoot;

            var result = new List<ClassStructure>(root.NestedClasses);

            return Task.FromResult<object>(result);
        }
    }
}