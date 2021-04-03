using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polyglot.Interactive.Contracts
{
    public record ClassStructure(string Name, IEnumerable<string> Modifiers, IEnumerable<FieldStructure> Fields, IEnumerable<MethodStructure> Methods, IEnumerable<ConstructorStructure> Constructors/*, IEnumerable<PropertyStructure> Properties*/)
    {
        public virtual bool Equals(ClassStructure other)
        {
            return Name == other.Name
                && Modifiers.SequenceEqual(other.Modifiers)
                && Fields.SequenceEqual(other.Fields)
                && Methods.SequenceEqual(other.Methods);
        }
    }

    public record FieldStructure(VariableStructure Variable, IEnumerable<string> Modifiers)
    {
        public virtual bool Equals(FieldStructure other)
        {
            return Modifiers.SequenceEqual(other.Modifiers);
        }
    }

    public record MethodStructure(string Name, string ReturnType, IEnumerable<string> Modifiers, IEnumerable<VariableStructure> Parameters)
    {
        public virtual bool Equals(MethodStructure other)
        {
            return Name == other.Name
                && ReturnType == other.ReturnType
                && Modifiers.SequenceEqual(other.Modifiers)
                && Parameters.SequenceEqual(other.Parameters);
        }
    }

    public record VariableStructure(string Name, string Type);

    public record ConstructorStructure(IEnumerable<VariableStructure> Parameters)
    {
        public virtual bool Equals(ConstructorStructure other)
        {
            return Parameters.SequenceEqual(other.Parameters);
        }
    }
    //public record PropertyStructure(string Name, string )
    //{

    //}
}
