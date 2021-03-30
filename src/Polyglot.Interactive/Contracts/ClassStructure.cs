using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Interactive.Contracts
{
    public record ClassStructure(string Name, string AccessModifier, IEnumerable<FieldStructure> Fields, IEnumerable<MethodStructure> Methods/*, IEnumerable<PropertyStructure> Properties*/);

    public record FieldStructure(VariableStructure Variable, string AccessModifier);

    public record MethodStructure(string Name, string ReturnType, string AccessModifier, IEnumerable<VariableStructure> Parameters);

    public record VariableStructure(string Name, string Type);

    //public record PropertyStructure(string Name, string )
    //{

    //}
}
