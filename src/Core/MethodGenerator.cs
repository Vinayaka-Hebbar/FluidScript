using FluidScript.Compiler.Scopes;

namespace FluidScript.Core
{
    public class MethodGenerator
    {
        public readonly ObjectScope Scope;
        public readonly ScriptEngine Engine;

        public MethodGenerator(ScriptEngine scriptEngine, ObjectScope objectScope)
        {
            Scope = objectScope;
            Engine = scriptEngine;
        }

        public FluidScript.Compiler.SyntaxTree.FunctionDeclarationStatement Generate(IScriptSource source)
        {
            var syntaxVisitor = new Compiler.SyntaxVisitor(source, Scope, Engine.Settings);
            syntaxVisitor.MoveNext();
            var name = syntaxVisitor.GetName();
            if (name == "function")
            {
                var statement = syntaxVisitor.VisitFunctionDefinition();
                return statement;
            }
            throw new System.InvalidOperationException("cannot find function");
        }

        public void DefineField(string name, int value)
        {
            Compiler.SyntaxTree.Declaration declaration = new Compiler.SyntaxTree.FieldDelcaration(name, value.GetType());
            Compiler.SyntaxTree.ExpressionStatement valueAtTop = new Compiler.SyntaxTree.ExpressionStatement(new Compiler.SyntaxTree.LiteralExpression(value));
            Scope.DeclareMember(declaration, Compiler.Reflection.BindingFlags.Private, Compiler.Reflection.MemberTypes.Field, valueAtTop);
        }

        public FluidScript.Compiler.SyntaxTree.FunctionDeclarationStatement Generate(System.IO.FileInfo info)
        {
            return Generate(new FileSource(info));
        }

        public FluidScript.Compiler.SyntaxTree.FunctionDeclarationStatement Generate(string text)
        {
            return Generate(new StringSource(text));
        }

        public Compiler.SyntaxTree.Statement GetStatement(string text)
        {
            var syntaxVisitor = new Compiler.SyntaxVisitor(new StringSource(text), Scope, Engine.Settings);
            syntaxVisitor.MoveNext();
            return syntaxVisitor.VisitStatement();
        }

        public Compiler.SyntaxTree.Expression GetExpression(string text)
        {
            var syntaxVisitor = new Compiler.SyntaxVisitor(new StringSource(text), Scope, Engine.Settings);
            syntaxVisitor.MoveNext();
            return syntaxVisitor.VisitExpression();
        }
    }
}
