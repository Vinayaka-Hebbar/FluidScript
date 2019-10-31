using FluidScript.Compiler;
using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.Scopes;
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
            return new TypeGenerator(this, new StringSource(text), new GlobalScope());
        }

        public TypeGenerator CreateTypeGenerator(System.IO.FileInfo fileInfo)
        {
            return new TypeGenerator(this, new FileSource(fileInfo), new GlobalScope());
        }
        public MethodGenerator CreateMethodGenerator(string text)
        {
            var global = new GlobalScope();
            return new MethodGenerator(this, new StringSource(text), new ObjectScope(global));
        }

        public MethodGenerator CreateMethodGenerator(System.IO.FileInfo fileInfo)
        {
            var global = new GlobalScope();
            return new MethodGenerator(this, new FileSource(fileInfo), new ObjectScope(global));
        }
    }
}
