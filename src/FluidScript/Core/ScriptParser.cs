using FluidScript.Compiler;
using FluidScript.Library;

namespace FluidScript
{
    /// <summary>
    /// Generates SyntaxTree for text or file
    /// </summary>
    public abstract class ScriptParser
    {
        /// <summary>
        /// Parse settings
        /// </summary>
        public readonly ParserSettings Settings;

        /// <summary>
        /// New <see cref="ScriptParser"/> instance
        /// </summary>
        public ScriptParser()
        {
            Settings = ParserSettings.Default;
        }

        /// <summary>
        /// Initializes a new <see cref="ScriptParser"/>
        /// </summary>
        public ScriptParser(ParserSettings settings)
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
            using (Parser visitor = new Parser(new StringSource(text), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitStatement();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Expression"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parse <see cref="Compiler.SyntaxTree.Expression"/></returns>
        public Compiler.SyntaxTree.Expression GetExpression(string text)
        {
            using (Parser visitor = new Parser(new StringSource(text), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitExpression();
            }
            return Compiler.SyntaxTree.Expression.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Statement"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parse <see cref="Compiler.SyntaxTree.Statement"/></returns>
        public static Compiler.SyntaxTree.Statement GetStatement(string text, ParserSettings settings = null)
        {
            using (Parser visitor = new Parser(new StringSource(text), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.VisitStatement();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Expression"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parse <see cref="Compiler.SyntaxTree.Expression"/></returns>
        public static Compiler.SyntaxTree.Expression GetExpression(string text, ParserSettings settings = null)
        {
            using (Parser visitor = new Parser(new StringSource(text), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.VisitExpression();
            }
            return Compiler.SyntaxTree.Expression.Empty;
        }

        /// <summary>
        /// Creates <see cref="Compiler.SyntaxTree.Node"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <returns>Parsed <see cref="Compiler.SyntaxTree.Node"/></returns>
        public Compiler.SyntaxTree.Node ParseText(string text)
        {
            using (Parser visitor = new Parser(new StringSource(text), Settings))
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
            using (Parser visitor = new Parser(new FileSource(new System.IO.FileInfo(path)), Settings))
            {
                if (visitor.MoveNext())
                    return visitor.VisitMember();
            }
            return Compiler.SyntaxTree.Statement.Empty;
        }

        /// <summary>
        /// Invoke text expression or statement
        /// </summary>
        public abstract object Invoke(string text);
    }
}
