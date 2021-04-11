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
    public class DeclarationsStructureMetric : IMetricCalculator
    {
        private readonly CSharpParseOptions _parserOptions;
        public string Name => "declarationsStructure";

        public DeclarationsStructureMetric()
        {
            _parserOptions = CSharpParseOptions.Default.WithKind(SourceCodeKind.Script);
        }

        public Task<object> CalculateAsync(SubmitCode command, Kernel kernel = null, List<KernelEvent> events = null, IReadOnlyDictionary<string, string> newVariables = null, TimeSpan runTime = default, DateTime? lastRun = null)
        {
            var tree =
                CSharpSyntaxTree.ParseText(command.Code, _parserOptions);

            //var declarationWalker = new ClassWalker();
            var declarationWalker = new DeclarationWalker();

            declarationWalker.Visit(tree.GetRoot());

            var root = declarationWalker.DeclarationsRoot;

            var result = new List<ClassStructure>(root.NestedClasses);
            result.Add(root);
            return Task.FromResult<object>(result);
        }
    }
}