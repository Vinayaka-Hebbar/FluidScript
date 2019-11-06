using System;

namespace FluidScript.Core
{
    public class MethodGenerator
    {
        public readonly ScriptEngine Engine;
        public readonly Compiler.Scopes.ObjectScope Scope;

        public MethodGenerator(ScriptEngine engine, Compiler.Scopes.ObjectScope scope)
        {
            Engine = engine;
            Scope = scope;
        }

        public Compiler.SyntaxTree.Expression GetExpression(string text)
        {
            var declrative = new Compiler.Scopes.DeclarativeScope(Scope);
            var syntaxVisitor = new Compiler.SyntaxVisitor(new StringSource(text), declrative, Engine.Settings);
            if (syntaxVisitor.MoveNext())
                return syntaxVisitor.VisitExpression();
            return Compiler.SyntaxTree.Expression.Empty;
        }

        public void DefineMethod(string name,PrimitiveType[] types, System.Func<RuntimeObject[], RuntimeObject> onInvoke)
        {
            Scope.DefineMethod(name,types, onInvoke);
        }

        public void DefineField(string name, RuntimeObject value)
        {
            Scope.DefineVariable(name, value);
        }
    }
}
