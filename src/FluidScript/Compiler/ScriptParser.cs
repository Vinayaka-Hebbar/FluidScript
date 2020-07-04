using FluidScript.Compiler.Lexer;
using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Syntax Visiitor which will tokenizes the text
    /// </summary>
    public class ScriptParser : Parser
    {
        /// <summary>
        /// current modifiers
        /// </summary>
        private Modifiers modifiers;

        public ScriptParser(ITextSource source, ParserSettings settings) : base(source, settings)
        {
        }

        public Node VisitNode()
        {
            switch (TokenType)
            {
                case TokenType.Identifier:
                    Node node = VisitMember();
                    if (node == null)
                        return VisitStatement();
                    return node;
                case TokenType.Numeric:
                    return VisitStatement();
                default:
                    throw new System.Exception(string.Format("Invalid Token type {0} at {1}", TokenType, Source.LineInfo));
            }
        }

        public ParsedProgram Parse()
        {
            ParsedProgram program = new ParsedProgram();
            do
            {
                if (TokenType == TokenType.Identifier)
                {
                    ReadVariableName(out string name);
                    if (Keywords.TryGetIdentifier(name, out IdentifierType value))
                    {
                        switch (value)
                        {
                            case IdentifierType.Class:
                                MoveNext();
                                program.Members.Add(VisitTypeDeclaration());
                                break;
                            case IdentifierType.Import:
                                MoveNextThenIf(TokenType.LeftBrace);
                                var imports = VisitTypeImports().ToArray();
                                MoveNextIf(TokenType.RightBrace);
                                ReadVariableName(out string s);
                                if (s.Equals(Keywords.From, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    MoveNextThenIf(TokenType.Identifier);
                                    ReadVariableName(out string lib);
                                    program.Import(lib, imports);
                                    MoveNextThenIf(TokenType.SemiColon);
                                    break;
                                }
                                else
                                {
                                    throw new System.Exception(string.Format("Invalid Token type {0} at {1}", TokenType, Source.LineInfo));
                                }
                            default:
                                return program;
                        }
                    }
                }
                if (TokenType == TokenType.End)
                    return program;
            } while (MoveNext());
            return program;
        }

        /// <summary>
        /// Visit member declaration
        /// </summary>
        public MemberDeclaration VisitMember()
        {
            //reset modifier
            modifiers = Modifiers.None;
            MemberDeclaration declaration;
            switch (TokenType)
            {
                case TokenType.Identifier:
                    declaration = VisitIdentifierMember();
                    break;
                default:
                    throw new System.Exception(string.Format("Invalid Token type {0} at {1}", TokenType, Source.LineInfo));
            }
            if (declaration != null)
                declaration.Modifiers = modifiers;
            return declaration;
        }

        /// <summary>
        /// Visit members declaration
        /// </summary>
        public NodeList<MemberDeclaration> VisitMembers()
        {
            var list = new NodeList<MemberDeclaration>();
            while (MoveNextThenIfNot(TokenType.RightBrace))
            {
                list.Add(VisitMember());
                if (TokenType == TokenType.RightBrace)
                    break;
            }
            return list;
        }

        private MemberDeclaration VisitIdentifierMember()
        {
            var start = Source.Position;
            ReadVariableName(out string name);
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Class:
                        MoveNext();
                        return VisitTypeDeclaration();
                    case IdentifierType.Ctor:
                        MoveNext();
                        return VisitConstructorDeclaration();
                    case IdentifierType.Function:
                        MoveNext();
                        return VisitFunctionDeclaration();
                    case IdentifierType.Var:
                        MoveNext();
                        return VisitFieldDeclaration();
                    case IdentifierType.Val:
                        MoveNext();
                        modifiers |= Modifiers.ReadOnly;
                        return VisitFieldDeclaration();
                    case IdentifierType.Implement:
                        //todo has set
                        modifiers |= Modifiers.Implement;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Static:
                        modifiers |= Modifiers.Static;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Private:
                        modifiers |= Modifiers.Private;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Get:
                        MoveNext();
                        modifiers |= Modifiers.Getter;
                        return VisitFunctionDeclaration();
                    case IdentifierType.Set:
                        MoveNext();
                        modifiers |= Modifiers.Setter;
                        return VisitFunctionDeclaration();
                }
            }
            Source.SeekTo(start - 1);
            MoveNext();
            return null;
        }


        public IEnumerable<TypeImport> VisitTypeImports()
        {
            while (MoveNext())
            {
                if (TokenType == TokenType.Identifier)
                {
                    ReadTypeName(out string name);
                    MoveNext();
                    if (TokenType == TokenType.Equal)
                    {
                        ReadTypeName(out string s);
                        yield return new AliasImport(name, s);
                    }
                    else
                    {
                        yield return new TypeImport(name);
                    }
                }
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.Comma);
            }
        }

        /// <summary>
        /// Type declaration
        /// </summary>
        public TypeDeclaration VisitTypeDeclaration()
        {
            if (TokenType == TokenType.Identifier)
            {
                ReadVariableName(out string name);
                MoveNext();
                TypeSyntax baseType = null;
                INodeList<TypeSyntax> implements = null;
                if (TokenType == TokenType.Identifier)
                {
                    ReadVariableName(out string s);
                    if (s.Equals(Keywords.Extends, System.StringComparison.OrdinalIgnoreCase))
                    {
                        MoveNext();
                        baseType = VisitType();
                        if (TokenType == TokenType.Identifier)
                        {
                            ReadVariableName(out s);
                        }
                    }
                    if (s.Equals(Keywords.Implements, System.StringComparison.OrdinalIgnoreCase))
                    {
                        implements = VisitTypes(TokenType.Comma, TokenType.LeftBrace);
                    }
                }
                // todo extends
                if (TokenType == TokenType.LeftBrace)
                {
                    var members = VisitMembers();
                    // end of class
                    CheckSyntaxExpected(TokenType.RightBrace);
                    return new TypeDeclaration(name, baseType, implements, members)
                    {
                        Source = Source
                    };
                }
            }
            throw new System.Exception("Unexpected Keyword");
        }



        public ConstructorDeclaration VisitConstructorDeclaration()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                NodeList<TypeParameter> parameterList;
                //return type
                TypeParameter[] parameters = new TypeParameter[0];
                if (TokenType == TokenType.LeftParenthesis)
                    parameters = VisitFunctionParameters().ToArray();
                parameterList = new NodeList<TypeParameter>(parameters);
                //todo throw if other
                MoveNextIf(TokenType.RightParenthesis);
                //todo abstract, virtual functions
                //To avoid block function
                BlockStatement body = VisitBlock();
                return new ConstructorDeclaration(parameterList, body);
            }
            throw new System.Exception("syntax error at " + Source.LineInfo);
        }

        /// <summary>
        /// Field declaration
        /// </summary>
        public FieldDelcaration VisitFieldDeclaration()
        {
            var declarations = VisitVarDeclarations();
            return new FieldDelcaration(declarations);
        }

        /// <summary>
        /// Function Declartion
        /// </summary>
        public FunctionDeclaration VisitFunctionDeclaration()
        {
            Labels.Clear();
            if (TokenType == TokenType.Identifier)
            {
                ReadVariableName(out string name);
                MoveNext();
                NodeList<TypeParameter> parameterList;
                //return type
                TypeParameter[] parameters = new TypeParameter[0];
                if (TokenType == TokenType.LeftParenthesis)
                    parameters = VisitFunctionParameters().ToArray();
                parameterList = new NodeList<TypeParameter>(parameters);
                //todo throw if other
                MoveNextIf(TokenType.RightParenthesis);
                TypeSyntax returnType = null;
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    returnType = VisitType();
                }
                //todo abstract, virtual functions
                //To avoid block function
                BlockStatement body = VisitBlock();
                return new FunctionDeclaration(name, parameterList, returnType, body);
            }
            throw new System.Exception("syntax error");
        }

        #region Static
        /// <summary>
        /// Creates <see cref="Node"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parsed <see cref="Node"/></returns>
        public static Node ParseText(string text, ParserSettings settings = null)
        {
            using (ScriptParser visitor = new ScriptParser(new TextSource(text), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.VisitNode();
            }
            return Expression.Empty;
        }

        /// <summary>
        /// Creates <see cref="Node"/> for file <paramref name="path"/>
        /// </summary>
        /// <param name="path">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parsed <see cref="Node"/></returns>
        public static ParsedProgram ParseProgram(string path, ParserSettings settings = null)
        {
            using (ScriptParser visitor = new ScriptParser(new StreamSource(new System.IO.FileInfo(path)), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.Parse();
            }
            return null;
        }
        #endregion
    }
}