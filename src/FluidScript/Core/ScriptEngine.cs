using FluidScript.Compiler;
using FluidScript.Library;

namespace FluidScript
{
    /// <summary>
    /// Generates SyntaxTree for text or file
    /// </summary>
    public class ScriptEngine
    {
        /// <summary>
        /// Parse settings
        /// </summary>
        public readonly ParserSettings Settings;

        /// <summary>
        /// New <see cref="ScriptEngine"/> instance
        /// </summary>
        public ScriptEngine()
        {
            Settings = new ParserSettings();
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptEngine"/>
        /// </summary>
        public ScriptEngine(ParserSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Statement"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parse <see cref="Compiler.SyntaxTree.Statement"/></returns>
        public Compiler.SyntaxTree.Statement GetStatement(string text)
        {
            using (SyntaxVisitor visitor = new SyntaxVisitor(new StringSource(text), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitStatement();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Node"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parsed <see cref="Compiler.SyntaxTree.Node"/></returns>
        public Compiler.SyntaxTree.Node ParseText(string text)
        {
            using (SyntaxVisitor visitor = new SyntaxVisitor(new StringSource(text), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitMember();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Node"/> for file <paramref name="path"/>
        /// </summary>
        /// <param name="path">Text to parse</param>
        /// <returns>Parsed <see cref="Compiler.SyntaxTree.Node"/></returns>
        public Compiler.SyntaxTree.Node ParseFile(string path)
        {
            using (SyntaxVisitor visitor = new SyntaxVisitor(new FileSource(new System.IO.FileInfo(path)), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitMember();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }
    }
}
