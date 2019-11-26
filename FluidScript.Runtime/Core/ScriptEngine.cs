﻿using FluidScript.Compiler;
using FluidScript.Compiler.Metadata;
using FluidScript.Core;

namespace FluidScript
{
    /// <summary>
    /// Generates SyntaxTree for text
    /// </summary>
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

        public Compiler.SyntaxTree.Statement GetStatement(string text)
        {
            using (SyntaxVisitor visitor = new SyntaxVisitor(new StringSource(text), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitStatement();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }
    }
}
