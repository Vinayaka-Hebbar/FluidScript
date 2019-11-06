using FluidScript.Compiler;
using FluidScript.Compiler.Scopes;
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

        public MethodGenerator CreateMethodGenerator()
        {
            return new MethodGenerator(this, new ObjectScope((Scope)null));
        }
    }
}
