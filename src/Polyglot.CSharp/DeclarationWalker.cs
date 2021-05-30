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

        private static CodeString Dummyfy(string str) => new CodeString(str, new StringSpan(1, 2));

        private class DeclarationContext
        {
            public string Name;
            public DeclarationContextKind Kind;
            public HashSet<string> Modifiers = new();
            public HashSet<FieldStructure> Fields = new();
            public HashSet<PropertyStructure> Properties = new();
            public HashSet<MethodStructure> Methods = new();
            public HashSet<ConstructorStructure> Constructors = new();
            public HashSet<DeclarationContext> NestedClasses = new();

            public DeclarationContextKind ChildKind => Kind == DeclarationContextKind.Root ? DeclarationContextKind.TopLevel : DeclarationContextKind.Type;

            public ClassStructure ClassStructure => new ClassStructure(Dummyfy(Name), Kind, Modifiers.Select(m => Dummyfy(m)), Fields, Properties, Methods, Constructors, NestedClasses.Select(c => c.ClassStructure));
            // public ClassStructure ClassStructure => new ClassStructure(Name, Kind, Modifiers, Fields, Methods, Constructors, NestedClasses.Select(c => c.ClassStructure));
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
                                                                      .Select(name => new VariableStructure(Dummyfy(name), _current.ChildKind, Dummyfy(type)))
                                                                      .Select(@var => new FieldStructure(@var, modifiers.Select(m => Dummyfy(m)))));

            base.VisitFieldDeclaration(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var type = node.Declaration.Type.ToString();

            if (_currentMethod is not null)
            {
                _currentMethod.LocalVariables.UnionWith(node.Declaration.Variables.Select(varSyntax => varSyntax.Identifier.ValueText)
                                                                                        .Select(name => new VariableStructure(Dummyfy(name), DeclarationContextKind.Method, Dummyfy(type))));
            }

            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var name = node.Identifier.ValueText;
            var type = node.Type.ToString();
            var modifiers = node.Modifiers.Select(m => m.ValueText);

            var accessors = node?.AccessorList?.Accessors.Select(a => a.Keyword.ValueText);
            
            if(node.ExpressionBody is not null)
            {
                accessors = new[] { "set" };
            }

            _current.Properties.Add(new PropertyStructure(new VariableStructure(Dummyfy(name), DeclarationContextKind.Type, Dummyfy(type)), modifiers.Select(m => Dummyfy(m)), accessors.Select(a => Dummyfy(a))));

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var name = node.Identifier.ValueText;
            var returnType = node.ReturnType.ToString();
            var modifiers = node.Modifiers.Select(m => m.ValueText);

            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(Dummyfy(param.Identifier.ValueText), DeclarationContextKind.Method, Dummyfy(param.Type.ToString())));

            _currentMethod = new MethodContext();
            base.VisitMethodDeclaration(node);
            var body = new MethodBodyStructure(_currentMethod.LocalVariables);
            _currentMethod = null;

            var method = new MethodStructure(Dummyfy(name), _current.ChildKind, Dummyfy(returnType), modifiers.Select(m => Dummyfy(m)), parameters, body);
            _current.Methods.Add(method);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var parameters = node.ParameterList.Parameters.Select(param => new VariableStructure(Dummyfy(param.Identifier.ValueText), DeclarationContextKind.Method, Dummyfy(param.Type.ToString())));
            var modifiers = node.Modifiers.Select(m => m.ValueText);

            var constructor = new ConstructorStructure(modifiers, parameters);
            _current.Constructors.Add(constructor);

            base.VisitConstructorDeclaration(node);
        }
    }
}