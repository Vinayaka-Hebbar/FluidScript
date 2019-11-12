using FluidScript.Compiler;
using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.Metadata;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Core;
using System.Collections.Generic;
using System.Linq;

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

        public TypeGenerator CreateTypeGenerator(string text)
        {
            return new TypeGenerator(this, new StringSource(text), new GlobalScope("Dummy"));
        }

        public TypeGenerator CreateTypeGenerator(System.IO.FileInfo fileInfo)
        {
            return new TypeGenerator(this, new FileSource(fileInfo), new GlobalScope("Dummy"));
        }
        public MethodGenerator CreateMethodGenerator()
        {
            var global = new GlobalScope("Dummy");
            return new MethodGenerator(this, new ObjectScope(global, "Dummy"));
        }
    }
}
