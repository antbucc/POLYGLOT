using Polyglot.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Polyglot.CSharp
{
    public record CodeString(string Value, StringSpan Span)
    {
        public virtual bool Equals(CodeString other)
        {
            return new CodeStringComparer().Equals(this, other);
        }
    }

    public record ClassStructure(CodeString Name, DeclarationContextKind Kind, IEnumerable<CodeString> Modifiers, IEnumerable<FieldStructure> Fields, IEnumerable<PropertyStructure> Properties, IEnumerable<MethodStructure> Methods, IEnumerable<ConstructorStructure> Constructors, IEnumerable<ClassStructure> NestedClasses)
    {
        public virtual bool Equals(ClassStructure other)
        {
            return new ClassStructureComparer().Equals(this, other);
        }
    }

    public record VariableStructure(CodeString Name, DeclarationContextKind Kind, CodeString Type)
    {
        public virtual bool Equals(VariableStructure other)
        {
            return new VariableStructureComparer().Equals(this, other);
        }
    }

    public record FieldStructure(VariableStructure Variable, IEnumerable<CodeString> Modifiers)
    {
        // uses Variable.Kind to avoid duplicates
        public DeclarationContextKind Kind => Variable.Kind;

        public virtual bool Equals(FieldStructure other)
        {
            return new FieldStructureComparer().Equals(this, other);
        }
    }

    public record PropertyStructure(VariableStructure Variable, IEnumerable<CodeString> Modifiers, IEnumerable<CodeString> Accessors)
    {
        // uses Variable.Kind to avoid duplicates
        public DeclarationContextKind Kind => Variable.Kind;

        public virtual bool Equals(PropertyStructure other)
        {
            return new PropertyStructureComparer().Equals(this, other);
        }
    }

    public record MethodStructure(CodeString Name, DeclarationContextKind Kind, CodeString ReturnType, IEnumerable<CodeString> Modifiers, IEnumerable<VariableStructure> Parameters, MethodBodyStructure Body)
    {
        public virtual bool Equals(MethodStructure other)
        {
            return new MethodStructureComparer().Equals(this, other);
        }
    }

    public record ConstructorStructure(IEnumerable<CodeString> Modifiers, IEnumerable<VariableStructure> Parameters)
    {
        public DeclarationContextKind Kind => DeclarationContextKind.Type;

        public virtual bool Equals(ConstructorStructure other)
        {
            return new ConstructorStructureComparer().Equals(this, other);
        }
    }

    public record MethodBodyStructure(IEnumerable<VariableStructure> LocalVariables)
    {
        public DeclarationContextKind Kind => DeclarationContextKind.Method;

        public virtual bool Equals(MethodBodyStructure other)
        {
            return new MethodBodyStructureComparer().Equals(this, other);
        }
    }


    internal class CodeStringComparer : IEqualityComparer<CodeString>
    {
        public bool Equals(CodeString x, CodeString y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode([DisallowNull] CodeString obj)
        {
            throw new NotImplementedException();
        }
    }
    internal class ClassStructureComparer : IEqualityComparer<ClassStructure>
    {
        public bool Equals(ClassStructure x, ClassStructure y)
        {
            return x.Name == y.Name
                && x.Kind == y.Kind
                && x.Modifiers.SequenceEqual(y.Modifiers)
                && x.Fields.SequenceEqual(y.Fields, new FieldStructureComparer())
                && x.Properties.SequenceEqual(y.Properties, new PropertyStructureComparer())
                && x.Methods.SequenceEqual(y.Methods, new MethodStructureComparer())
                && x.Constructors.SequenceEqual(y.Constructors, new ConstructorStructureComparer())
                && x.NestedClasses.SequenceEqual(y.NestedClasses, new ClassStructureComparer());
        }

        public int GetHashCode([DisallowNull] ClassStructure obj)
        {
            throw new NotImplementedException();
        }
    }

    internal class VariableStructureComparer : IEqualityComparer<VariableStructure>
    {
        public bool Equals(VariableStructure x, VariableStructure y)
        {
            return x.Name == y.Name
                && x.Kind == y.Kind
                && x.Type == y.Type;
        }

        public int GetHashCode([DisallowNull] VariableStructure obj)
        {
            throw new NotImplementedException();
        }
    }

    internal class FieldStructureComparer : IEqualityComparer<FieldStructure>
    {
        public bool Equals(FieldStructure x, FieldStructure y)
        {
            return x.Variable.Equals(y.Variable)
                && x.Kind == y.Kind
                && x.Modifiers.SequenceEqual(y.Modifiers);
        }

        public int GetHashCode([DisallowNull] FieldStructure obj)
        {
            throw new NotImplementedException();
        }
    }
    
    internal class PropertyStructureComparer : IEqualityComparer<PropertyStructure>
    {
        public bool Equals(PropertyStructure x, PropertyStructure y)
        {
            return x.Variable.Equals(y.Variable)
                && x.Kind == y.Kind
                && x.Modifiers.SequenceEqual(y.Modifiers);
        }

        public int GetHashCode([DisallowNull] PropertyStructure obj)
        {
            throw new NotImplementedException();
        }
    }

    internal class MethodStructureComparer : IEqualityComparer<MethodStructure>
    {
        public bool Equals(MethodStructure x, MethodStructure y)
        {
            return x.Name == y.Name
                && x.Kind == y.Kind
                && x.ReturnType == y.ReturnType
                && x.Modifiers.SequenceEqual(y.Modifiers)
                && x.Parameters.SequenceEqual(y.Parameters, new VariableStructureComparer())
                && x.Body.Equals(y.Body);
        }

        public int GetHashCode([DisallowNull] MethodStructure obj)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConstructorStructureComparer : IEqualityComparer<ConstructorStructure>
    {
        public bool Equals(ConstructorStructure x, ConstructorStructure y)
        {
            return x.Modifiers.SequenceEqual(y.Modifiers)
                && x.Parameters.SequenceEqual(y.Parameters, new VariableStructureComparer())
                && x.Kind == y.Kind;
        }

        public int GetHashCode([DisallowNull] ConstructorStructure obj)
        {
            throw new NotImplementedException();
        }
    }
    
    internal class MethodBodyStructureComparer : IEqualityComparer<MethodBodyStructure>
    {
        public bool Equals(MethodBodyStructure x, MethodBodyStructure y)
        {
            return x.LocalVariables.SequenceEqual(y.LocalVariables, new VariableStructureComparer())
                && x.Kind == y.Kind;
        }

        public int GetHashCode([DisallowNull] MethodBodyStructure obj)
        {
            throw new NotImplementedException();
        }
    }
}
