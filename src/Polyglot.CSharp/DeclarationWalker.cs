using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Polyglot.CSharp
{
    public enum DeclarationContextKind { Root, TopLevel, Type, Method };


    internal class DeclarationWalker : CSharpSyntaxWalker
    {

        public DeclarationWalker()
        {
            var toplevel = new DeclarationContext
            {
                Name = "Top Level",
                Kind = DeclarationContextKind.Root
            };
            _current = toplevel;
            _declarationsStructure.Add(_current.Name, _current);
        }

        private readonly Dictionary<string, DeclarationContext> _declarationsStructure = new();
        public ClassStructure DeclarationsRoot =>
            _declarationsStructure.Values.Where(c => c.Kind == DeclarationContextKind.Root).FirstOrDefault()?.ClassStructure;

        private Stack<DeclarationContext> _declarationStack = new();
        private DeclarationContext _current;
        private MethodContext _currentMethod;

        private class DeclarationContext
        {
            public string Name;
            public DeclarationContextKind Kind;
            public HashSet<string> Modifiers = new();
            public HashSet<FieldStructure> Fields = new();
            public HashSet<MethodStructure> Methods = new();
            public HashSet<ConstructorStructure> Constructors = new();
            public HashSet<DeclarationContext> NestedClasses = new();

            public DeclarationContextKind ChildKind => Kind == DeclarationContextKind.Root ? DeclarationContextKind.TopLevel : DeclarationContextKind.Type;

            public ClassStructure ClassStructure => new ClassStructure(Name, Kind, Modifiers, Fields, Methods, Constructors, NestedClasses.Select(c => c.ClassStructure));
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
                classDeclaration.Name = className;
                var declarationKind = _current.ChildKind;
                classDeclaration.Kind = declarationKind;
                _declarationsStructure.Add(className, classDeclaration);
                _current.NestedClasses.Add(classDeclaration);
            }
            _declarationStack.Push(_current);
            _current = classDeclaration;

            _current.Modifiers.UnionWith(node.Modifiers.Select(m => m.ValueText));            
            
            base.VisitClassDeclaration(node);

            _current = _declarationStack.Pop();
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var type = node.Declaration.Type.ToString();

            var modifiers = node.Modifiers.Select(m => m.ValueText);

            _current.Fields.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier.ValueText)
                                                                      .Select(name => new VariableStructure(name, _current.ChildKind, type))
                                                                      .Select(@var => new FieldStructure(@var, modifiers)));

            base.VisitFieldDeclaration(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var type = node.Declaration.Type.ToString();

            if (_currentMethod is not null)
            {
                _currentMethod.LocalVariables.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier.ValueText)
                                                                                        .Select(name => new VariableStructure(name, DeclarationContextKind.Method, type)));
            }

            base.VisitLocalDeclarationStatement(node);
        }

        //public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        //{
        //    base.VisitPropertyDeclaration(node);
        //}


        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var name = node.Identifier.ValueText;
            var returnType = node.ReturnType.ToString();
            var modifiers = node.Modifiers.Select(m => m.ValueText);

            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(param.Identifier.ValueText, DeclarationContextKind.Method, param.Type.ToString()));

            _currentMethod = new MethodContext();
            base.VisitMethodDeclaration(node);
            var body = new MethodBodyStructure(_currentMethod.LocalVariables);
            _currentMethod = null;

            var method = new MethodStructure(name, _current.ChildKind, returnType, modifiers, parameters, body);
            _current.Methods.Add(method);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(param.Identifier.ValueText, DeclarationContextKind.Method ,param.Type.ToString()));

            var constructor = new ConstructorStructure(parameters);
            _current.Constructors.Add(constructor);

            base.VisitConstructorDeclaration(node);
        }
    }
}