using FluidScript.Compiler;
using FluidScript.Compiler.Metadata;
using FluidScript.Core;

namespace FluidScript
{
    public class ScriptEngine
    {
        public readonly ParserSettings Settings;

        public ScriptEngine()
        {
            Settings = new ParserSettings();
        }

        public ScriptEngine(ParserSettings settings)
        {
            Settings = settings;
        }

        public FluidScript.Compiler.SyntaxTree.Statement GetStatement(string text, Prototype prototype)
        {
            var syntaxVisitor = new Compiler.SyntaxVisitor(new StringSource(text), prototype, Settings);
            if (syntaxVisitor.MoveNext())
                return syntaxVisitor.VisitStatement();
            return Compiler.SyntaxTree.Statement.Empty;
        }
    }
}
