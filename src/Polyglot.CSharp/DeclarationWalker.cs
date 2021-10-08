using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Polyglot.Gamification;

namespace Polyglot.Metrics.CSharp
{
    public enum DeclarationContextKind { Root, TopLevel, Type, Method };

    internal static class CSharpSyntaxHelper
    {
        public static CodeString ToCodeString(this SyntaxToken t) => new(t.ValueText, new(t.Span.Start, t.Span.End));

        public static CodeString ToCodeString(this TypeSyntax t) => new(t.ToString(), new(t.Span.Start, t.Span.End));
    }

    internal class DeclarationWalker : CSharpSyntaxWalker
    {

        public DeclarationWalker()
        {
            var toplevel = new DeclarationContext
            {
                Name = new CodeString("Top Level", new StringSpan(0, 0)),
                Kind = DeclarationContextKind.Root
            };
            _current = toplevel;
            _declarationsStructure.Add(_current.Name.Value, _current);
        }

        private readonly Dictionary<string, DeclarationContext> _declarationsStructure = new();
        public ClassStructure DeclarationsRoot =>
            _declarationsStructure.Values.Where(c => c.Kind == DeclarationContextKind.Root).FirstOrDefault()?.ClassStructure;

        private Stack<DeclarationContext> _declarationStack = new();
        private DeclarationContext _current;
        private MethodContext _currentMethod;

        private class DeclarationContext
        {
            public CodeString Name;
            public DeclarationContextKind Kind;
            public HashSet<CodeString> Modifiers = new();
            public HashSet<FieldStructure> Fields = new();
            public HashSet<PropertyStructure> Properties = new();
            public HashSet<MethodStructure> Methods = new();
            public HashSet<ConstructorStructure> Constructors = new();
            public HashSet<DeclarationContext> NestedClasses = new();

            public DeclarationContextKind ChildKind => Kind == DeclarationContextKind.Root ? DeclarationContextKind.TopLevel : DeclarationContextKind.Type;

            public ClassStructure ClassStructure => new ClassStructure(Name, Kind, Modifiers, Fields, Properties, Methods, Constructors, NestedClasses.Select(c => c.ClassStructure));
        }

        private class MethodContext
        {
            public HashSet<VariableStructure> LocalVariables = new();
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // get/set class dictionary + stack push
            var className = node.Identifier.ValueText;
            DeclarationContext classDeclaration;
            if(!_declarationsStructure.TryGetValue(className, out classDeclaration))
            {
                classDeclaration = new();
                classDeclaration.Name = node.Identifier.ToCodeString();
                var declarationKind = _current.ChildKind;
                classDeclaration.Kind = declarationKind;
                _declarationsStructure.Add(className, classDeclaration);
                _current.NestedClasses.Add(classDeclaration);
            }
            _declarationStack.Push(_current);
            _current = classDeclaration;

            _current.Modifiers.UnionWith(node.Modifiers.Select(m => m.ToCodeString()));
            
            base.VisitClassDeclaration(node);

            _current = _declarationStack.Pop();
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = node.Declaration.Type.ToCodeString();

            var modifiers = node.Modifiers.Select(m => m.ToCodeString());

            _current.Fields.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier)
                                                                      .Select(identifierToken => new VariableStructure(identifierToken.ToCodeString(), _current.ChildKind, type))
                                                                      .Select(@var => new FieldStructure(@var, modifiers)));

            base.VisitFieldDeclaration(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var type = node.Declaration.Type.ToCodeString();

            if (_currentMethod is not null)
            {
                _currentMethod.LocalVariables.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier)
                                                                                        .Select(identifierToken => new VariableStructure(identifierToken.ToCodeString(), DeclarationContextKind.Method, type)));
            }

            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            node.GetLocation();
            var name = node.Identifier.ToCodeString();
            var type = node.Type.ToCodeString();
            var modifiers = node.Modifiers.Select(m => m.ToCodeString());

            var accessors = new List<CodeString>();
            accessors.AddRange(node?.AccessorList?.Accessors.Select(a => a.Keyword.ToCodeString()) ?? new List<CodeString>());
            
            // the property arrow syntax
            // public float Property => 5;
            // doesn't have accessors but has ExpressionBody != null
            if(node.ExpressionBody is not null)
            {
                accessors.Add(new("get", new(node.ExpressionBody.Span.Start, node.ExpressionBody.Span.End)));
            }

            _current.Properties.Add(new PropertyStructure(new VariableStructure(name, DeclarationContextKind.Type, type), modifiers, accessors));

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var name = node.Identifier.ToCodeString();
            var returnType = node.ReturnType.ToCodeString();
            var modifiers = node.Modifiers.Select(m => m.ToCodeString());

            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(param.Identifier.ToCodeString(), DeclarationContextKind.Method, param.Type.ToCodeString()));

            _currentMethod = new MethodContext();
            base.VisitMethodDeclaration(node);
            var body = new MethodBodyStructure(_currentMethod.LocalVariables);
            _currentMethod = null;

            var method = new MethodStructure(name, _current.ChildKind, returnType, modifiers, parameters, body);
            _current.Methods.Add(method);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure((param.Identifier.ToCodeString()), DeclarationContextKind.Method, param.Type.ToCodeString()));
            var modifiers = node.Modifiers.Select(m => m.ToCodeString());

            var constructor = new ConstructorStructure(modifiers, parameters);
            _current.Constructors.Add(constructor);

            base.VisitConstructorDeclaration(node);
        }
    }
}