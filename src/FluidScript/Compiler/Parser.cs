using FluidScript.Compiler.Debugging;
using FluidScript.Compiler.Lexer;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Library;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Syntax Visiitor which will tokenizes the text
    /// </summary>
    public class Parser : System.IDisposable
    {
        private const char DotChar = '.';

        /// <summary>
        /// Source text
        /// </summary>
        public readonly IScriptSource Source;
        private readonly IList<string> _currentLabels = new List<string>();
        /// <summary>
        /// Token type
        /// </summary>
        protected internal TokenType TokenType;
        private char c;
        /// <summary>
        /// current modifiers
        /// </summary>
        private Modifiers modifiers;

        /// <summary>
        /// Parse settings
        /// </summary>
        public readonly ParserSettings Settings;

        /// <summary>
        /// Initializes new <see cref="Parser"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="settings"></param>
        public Parser(IScriptSource source, ParserSettings settings)
        {
            Source = source;
            Settings = settings;
        }

        /// <summary>
        /// labels like start: and goto start
        /// </summary>
        public string[] CurrentLabels
        {
            //todo for inner block and forgotted
            get => _currentLabels.ToArray();
        }

        #region Iterator
        /// <summary>
        /// Current Token
        /// </summary>
        public TokenType Current => TokenType;

        void System.IDisposable.Dispose()
        {
            Source.Dispose();
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Move to next <see cref="Parser.TokenType"/>
        /// </summary>
        /// <param name="skipLine">Indicates line to be skipped</param>
        public bool MoveNext(bool skipLine = true)
        {
            c = Source.ReadChar();
            TokenType = GetTokenType(skipLine);
            return c != char.MinValue;
        }

        private bool MoveNextThenIf(TokenType token)
        {
            return MoveNext() && TokenType == token;
        }

        private bool MoveNextThenIfNot(TokenType token)
        {
            return MoveNext() && TokenType != token;
        }

        public bool MoveNextIf(TokenType token)
        {
            if (TokenType == token)
                return MoveNext();
            throw new System.Exception(string.Concat("Invalid token ", c, " at ", Source.Position));
        }

        /// <summary>
        /// Reset source position
        /// </summary>
        public void Reset()
        {
            Source.Reset();
        }

        /// <summary>
        /// Token type
        /// </summary>
        public TokenType GetTokenType(bool skipLine)
        {
            char n = Source.PeekChar();
            switch (c)
            {
                case '(':
                    return TokenType.LeftParenthesis;
                case ')':
                    return TokenType.RightParenthesis;
                case '{':
                    return TokenType.LeftBrace;
                case '}':
                    return TokenType.RightBrace;
                case '[':
                    return TokenType.LeftBracket;
                case ']':
                    return TokenType.RightBracket;
                case '@':
                    if (char.IsLetterOrDigit(n))
                    {
                        c = Source.ReadChar();
                        return TokenType.SpecialVariable;
                    }
                    break;
                case '?':
                    if (n == '?')
                    {
                        c = Source.ReadChar();
                        return TokenType.NullPropagator;
                    }
                    return TokenType.Question;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return TokenType.Numeric;
                case '+':
                    if (n == '+')
                    {
                        c = Source.ReadChar();
                        return TokenType.PlusPlus;
                    }
                    return TokenType.Plus;
                case '-':
                    if (n == '-')
                    {
                        c = Source.ReadChar();
                        return TokenType.MinusMinus;
                    }
                    return TokenType.Minus;
                case '*':
                    if (n == '*')
                    {
                        c = Source.ReadChar();
                        return TokenType.StarStar;
                    }
                    return TokenType.Multiply;
                case '/':
                    return TokenType.Divide;
                case '%':
                    return TokenType.Percent;
                case '^':
                    if (n == '=')
                    {
                        //Todo
                    }
                    return TokenType.Circumflex;
                case '&':
                    if (n == '&')
                    {
                        c = Source.ReadChar();
                        return TokenType.AndAnd;
                    }
                    return TokenType.And;
                case '|':
                    if (n == '|')
                    {
                        c = Source.ReadChar();
                        return TokenType.OrOr;
                    }
                    return TokenType.Or;
                case '`':
                    c = Source.ReadChar();
                    return TokenType.String;
                case ',':
                    return TokenType.Comma;
                case ';':
                    return TokenType.SemiColon;
                case ':':
                    if (n == ':')
                    {
                        c = Source.ReadChar();
                        return TokenType.Qualified;
                    }
                    return TokenType.Colon;
                case '.':
                    return TokenType.Dot;
                case '=':
                    if (n == '>')
                    {
                        c = Source.ReadChar();
                        return TokenType.AnonymousMethod;
                    }
                    if (n == '=')
                    {
                        c = Source.ReadChar();
                        return TokenType.EqualEqual;
                    }
                    return TokenType.Equal;
                case '<':
                    if (n == '=')
                    {
                        c = Source.ReadChar();
                        return TokenType.LessEqual;
                    }
                    if (n == '<')
                    {
                        c = Source.ReadChar();
                        return TokenType.LessLess;
                    }
                    return TokenType.Less;
                case '>':
                    if (n == '=')
                    {
                        c = Source.ReadChar();
                        return TokenType.GreaterEqual;
                    }
                    if (n == '>')
                    {
                        c = Source.ReadChar();
                        return TokenType.GreaterGreater;
                    }
                    return TokenType.Greater;
                case '!':
                    if (n == '=')
                    {
                        c = Source.ReadChar();
                        return TokenType.BangEqual;
                    }
                    return TokenType.Bang;
                case '_':
                    if (char.IsLetter(n))
                    {
                        return TokenType.Identifier;
                    }
                    break;
                case '\\':
                    if (n == 'u')
                    {
                        //Unicode
                    }
                    break;
                case '\t':
                case ' ':
                    if (Source.CanAdvance)
                    {
                        c = Source.ReadChar();
                        return GetTokenType(skipLine);
                    }
                    return TokenType.End;
                case '\n':
                    Source.NextLine();
                    if (skipLine)
                    {
                        c = Source.ReadChar();
                        return GetTokenType(skipLine);
                    }
                    return TokenType.NewLine;
                case '\r':
                    if (n == '\n')
                    {
                        if (skipLine)
                        {
                            c = Source.ReadChar();
                            return GetTokenType(skipLine);
                        }
                    }
                    return TokenType.NewLine;
                default:
                    if (char.IsLetter(c))
                    {
                        return TokenType.Identifier;
                    }
                    break;
            }
            if (Source.CanAdvance == false)
                return TokenType.End;
            return TokenType.Bad;
        }
        #endregion

        /// <summary>
        /// Visit members declaration
        /// </summary>
        public NodeList<MemberDeclaration> VisitMembers()
        {
            var list = new NodeList<MemberDeclaration>();
            while (MoveNext())
            {
                list.Add(VisitMember());
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.SemiColon);
            }
            return list;
        }

        /// <summary>
        /// Visit member declaration
        /// </summary>
        public MemberDeclaration VisitMember()
        {
            //reset modifier
            modifiers = Compiler.Modifiers.None;
            MemberDeclaration declaration;
            switch (TokenType)
            {
                case TokenType.Identifier:
                    declaration = VisitIdentifierMember();
                    break;
                default:
                    throw new System.Exception(string.Format("Invalid Token type {0} at {1}", TokenType, Source.CurrentPosition));
            }
            declaration.Modifiers = modifiers;
            return declaration;
        }

        private MemberDeclaration VisitIdentifierMember()
        {
            var name = ReadVariableName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Class:
                        MoveNext();
                        return VisitTypeDeclaration();
                    case IdentifierType.Function:
                        MoveNext();
                        return VisitFunctionDeclaration();
                    case IdentifierType.Var:
                        MoveNext();
                        return VisitFieldDeclaration();
                    case IdentifierType.Val:
                        MoveNext();
                        modifiers |= Compiler.Modifiers.ReadOnly;
                        return VisitFieldDeclaration();
                    case IdentifierType.Implement:
                        //todo has set
                        modifiers |= Compiler.Modifiers.Implement;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Static:
                        modifiers |= Compiler.Modifiers.Static;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Private:
                        modifiers |= Compiler.Modifiers.Private;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Get:
                        MoveNext();
                        modifiers |= Compiler.Modifiers.Getter;
                        return VisitFunctionDeclaration();
                    case IdentifierType.Set:
                        MoveNext();
                        modifiers |= Compiler.Modifiers.Setter;
                        return VisitFunctionDeclaration();
                }
            }
            //todo move next() common
            throw new System.Exception(string.Concat("Unexpected Keyword ", name));
        }

        /// <summary>
        /// Type declaration
        /// </summary>
        public TypeDeclaration VisitTypeDeclaration()
        {
            if (TokenType == TokenType.Identifier)
            {
                var name = ReadVariableName();
                MoveNext();
                //todo extends
                if (TokenType == TokenType.LeftBrace)
                {
                    var members = VisitMembers();
                    MoveNextIf(TokenType.RightBrace);
                    return new TypeDeclaration(name, null, new NodeList<TypeSyntax>(), members)
                    {
                        Source = Source
                    };
                }
            }
            throw new System.Exception("Unexpected Keyword");
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
            _currentLabels.Clear();
            if (TokenType == TokenType.Identifier)
            {
                var name = ReadVariableName();
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

        /// <summary>
        /// Statement 
        /// </summary>
        public Statement VisitStatement()
        {
            TextPosition start = Source.CurrentPosition;
            Statement statement;
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    var statements = VisitStatementList();
                    // Consider new line also
                    if (TokenType == TokenType.RightBrace)
                        MoveNext(false);
                    statement = new BlockStatement(statements, CurrentLabels);
                    break;
                case TokenType.Identifier:
                    statement = VisitIdentifierStatement();
                    break;
                case TokenType.SemiColon:
                    MoveNext();
                    statement = Statement.Empty;
                    break;
                default:
                    statement = VisitExpressionStatement();
                    break;
            }
            statement.Span = new TextSpan(start, Source.CurrentPosition);
            return statement;
        }

        private Statement VisitIdentifierStatement()
        {
            long start = Source.Position;
            var name = ReadVariableName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                MoveNext();
                switch (type)
                {
                    case IdentifierType.Return:
                        Expression expression = null;
                        if (TokenType != TokenType.SemiColon)
                        {
                            expression = VisitExpression();
                        }
                        return new ReturnOrThrowStatement(expression, StatementType.Return);
                    case IdentifierType.Var:
                        //any type
                        var declarations = VisitVarDeclarations();
                        return new LocalDeclarationStatement(declarations);
                    case IdentifierType.Function:
                        return VisitLocalFunction();
                    case IdentifierType.While:
                        return VisitWhileStatement();
                    case IdentifierType.For:
                        return VisitForStatement();
                    case IdentifierType.Do:
                        return VisitDoWhileStatement();
                    case IdentifierType.If:
                        return VisitIfStatement();
                    case IdentifierType.Else:
                        return VisitStatement();
                    case IdentifierType.Break:
                        return Statement.Break;
                    case IdentifierType.Continue:
                        string target = null;
                        if (TokenType == TokenType.Identifier)
                        {
                            target = ReadVariableName();
                            MoveNext();
                        }

                        return new ContinueStatement(target);
                }
            }
            //default label statment
            //restore to prev
            Source.SeekTo(start - 1);
            //skips if new line
            MoveNext();
            return VisitExpressionStatement();
        }

        /// <summary>
        /// Loop Statement for 
        /// </summary>
        public Statement VisitForStatement()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                MoveNext();
                var initialization = VisitStatement();
                //todo throw error if others
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                var condition = VisitExpression();
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                var increment = VisitExpressionList(TokenType.Comma, TokenType.RightParenthesis);
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                Statement statement;
                if (TokenType == TokenType.LeftBrace)
                {
                    statement = VisitBlock();
                }
                else
                {
                    statement = VisitStatement();
                }
                return new LoopStatement(statement, StatementType.For)
                {
                    Initialization = initialization,
                    Condition = condition,
                    Increments = increment
                };
            }
            return Statement.Empty;
        }

        /// <summary>
        /// Loop Statement while
        /// </summary>
        public Statement VisitWhileStatement()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                MoveNext();
                var condition = VisitExpression();
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                Statement statement;
                if (TokenType == TokenType.LeftBrace)
                {
                    statement = VisitBlock();
                }
                else
                {
                    statement = VisitStatement();
                }
                return new LoopStatement(statement, StatementType.While)
                {
                    Condition = condition
                };
            }
            return Statement.Empty;
        }

        /// <summary>
        /// Loop Statement do while
        /// </summary>
        public Statement VisitDoWhileStatement()
        {
            Statement statement;
            if (TokenType == TokenType.LeftBrace)
            {
                statement = VisitBlock();
            }
            else
            {
                statement = VisitStatement();
            }
            if (TokenType == TokenType.RightBrace)
                MoveNext();
            var name = ReadVariableName();
            MoveNext();
            if (Keywords.Match(name, IdentifierType.While))
            {
                if (TokenType == TokenType.LeftParenthesis)
                {
                    MoveNext();
                    var condition = VisitExpression();
                    //{
                    //    Span = new TextSpan(start, Source.CurrentPosition)
                    //};
                    if (TokenType == TokenType.RightParenthesis)
                        MoveNext();
                    if (TokenType == TokenType.SemiColon)
                        MoveNext();
                    return new LoopStatement(statement, StatementType.DoWhile)
                    {
                        Condition = condition
                    };
                }
            }
            return Statement.Empty;
        }

        private Node VisitLabeledNode(string name)
        {
            MoveNext();
            //Todo for labeled Node
            return Expression.Empty;
        }

        /// <summary>
        /// Visit if statement
        /// </summary>
        /// <returns></returns>
        protected Statement VisitIfStatement()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                // (
                MoveNext();
                var expression = VisitExpression();
                //)
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                Statement body = VisitStatement();
                Statement other = null;
                if (CheckExpectedIdentifier(IdentifierType.Else))
                    other = VisitStatement();
                return new IfStatement(expression, body, other);
            }
            throw new System.Exception($"Syntax Error at line {Source.CurrentPosition}");
        }

        /// <summary>
        /// Visit local function
        /// </summary>
        /// <returns></returns>
        public Statement VisitLocalFunction()
        {
            if (TokenType == TokenType.Identifier)
            {
                string name = ReadVariableName();
                MoveNext();
                TypeParameter[] parameterList;
                IEnumerable<TypeParameter> parameters = Enumerable.Empty<TypeParameter>();
                if (TokenType == TokenType.LeftParenthesis)
                    parameters = VisitFunctionParameters();
                parameterList = parameters.ToArray();
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                TypeSyntax returnType = null;
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    returnType = VisitType();
                }
                //todo abstract, virtual functions
                //To avoid block function
                return new LocalFunctionStatement(name, parameterList, returnType, VisitBlock());
            }
            return Statement.Empty;

        }

        private Statement VisitExpressionStatement()
        {
            Expression expression = VisitExpression();
            return new ExpressionStatement(expression);
        }

        /// <summary>
        /// Visit block {}
        /// </summary>
        public BlockStatement VisitBlock()
        {
            //clear labels
            var start = Source.CurrentPosition;
            BlockStatement statement;
            if (TokenType == TokenType.LeftBrace)
            {
                var statements = VisitStatementList();
                // don't skip new line
                if (TokenType == TokenType.RightBrace)
                    MoveNext(false);
                statement = new BlockStatement(statements, CurrentLabels);
            }
            else if (TokenType == TokenType.AnonymousMethod)
            {
                MoveNext();
                statement = VisitAnonymousBlock();
            }
            else
                throw new System.Exception("Invalid Function declaration");
            statement.Span = new TextSpan(start, Source.CurrentPosition);
            return statement;
        }

        private BlockStatement VisitAnonymousBlock()
        {
            if (TokenType == TokenType.LeftBrace)
            {
                var statements = VisitStatementList();
                // skip new line
                MoveNextIf(TokenType.RightBrace);
                return new BlockStatement(statements, CurrentLabels);
            }
            NodeList<Statement> list = new NodeList<Statement>
            {
                new ReturnOrThrowStatement(VisitConditionalExpression(), StatementType.Return)
            };
            BlockStatement blockStatement = new BlockStatement(list, CurrentLabels);
            return blockStatement;
        }

        public NodeList<Statement> VisitStatementList()
        {
            var list = new NodeList<Statement>();
            while (MoveNextThenIfNot(TokenType.RightBrace))
            {
                list.Add(VisitStatement());
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.SemiColon, TokenType.NewLine);
            }
            return list;
        }

        public NodeList<Statement> VisitStatementList(TokenType splitToken, TokenType endToken)
        {
            var list = new NodeList<Statement>();
            do
            {
                list.Add(VisitStatement());
                if (TokenType == endToken)
                    break;
                CheckSyntaxExpected(splitToken, TokenType.NewLine);
            } while (MoveNext());
            return list;
        }

        #region Visitor
        public Expression VisitExpression()
        {
            Expression exp = VisitAssignmentExpression();
            while (TokenType == TokenType.Comma)
            {
                MoveNext();
                Expression right = VisitAssignmentExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.Comma);
            }
            return exp;
        }

        private Expression VisitAssignmentExpression()
        {
            Expression exp = VisitConditionalExpression();
            TokenType type = TokenType;
            if (type == TokenType.Equal)
            {
                MoveNext();
                Expression right = VisitAssignmentExpression();
                return new AssignmentExpression(exp, right);
            }
            return exp;
        }

        public Expression VisitConditionalExpression()
        {
            Expression exp = VisitLogicalORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Question;
                type = TokenType)
            {
                MoveNext();
                Expression second = VisitConditionalExpression();
                MoveNext();
                Expression third = VisitConditionalExpression();
                exp = new TernaryExpression(exp, second, third);
            }
            return exp;
        }

        public Expression VisitLogicalORExpression()
        {
            Expression exp = VisitLogicalAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.OrOr;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitLogicalAndExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.OrOr);
            }
            return exp;
        }

        public Expression VisitLogicalAndExpression()
        {
            Expression exp = VisitBitwiseORExpression();
            for (TokenType type = TokenType;
                type == TokenType.AndAnd;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitBitwiseORExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.AndAnd);
            }
            return exp;
        }

        public Expression VisitBitwiseORExpression()
        {
            Expression exp = VisitBitwiseXORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Or;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitBitwiseXORExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.Or);
            }
            return exp;
        }

        public Expression VisitBitwiseXORExpression()
        {
            Expression exp = VisitBitwiseAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.Circumflex;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitBitwiseAndExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.Circumflex);
            }
            return exp;
        }

        public Expression VisitBitwiseAndExpression()
        {
            Expression exp = VisitEqualityExpression();
            for (TokenType type = TokenType;
                type == TokenType.And;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitEqualityExpression();
                exp = new BinaryExpression(exp, right, ExpressionType.And);
            }
            return exp;
        }

        public Expression VisitEqualityExpression()
        {
            Expression exp = VisitRelationalExpression();
            for (TokenType type = TokenType;
                type == TokenType.EqualEqual || type == TokenType.BangEqual;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitRelationalExpression();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitRelationalExpression()
        {
            Expression exp = VisitShiftExpression();
            for (TokenType type = TokenType;
                type == TokenType.Greater || type == TokenType.GreaterEqual
                || type == TokenType.Less || type == TokenType.LessEqual;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitShiftExpression();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitShiftExpression()
        {
            Expression exp = VisitAdditionExpression();
            for (TokenType type = TokenType;
                type == TokenType.LessLess || type == TokenType.GreaterGreater;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitAdditionExpression();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitAdditionExpression()
        {
            Expression exp = VisitMultiplicationExpression();
            for (TokenType type = TokenType;
                type == TokenType.Plus || type == TokenType.Minus;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitMultiplicationExpression();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitMultiplicationExpression()
        {
            Expression exp = VisitExponentiation();
            for (TokenType type = TokenType;
                type == TokenType.Multiply || type == TokenType.Divide || type == TokenType.Percent;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitExponentiation();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitExponentiation()
        {
            Expression exp = VisitUnaryExpression();
            for (TokenType type = TokenType;
                type == TokenType.StarStar;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitExponentiation();
                exp = new BinaryExpression(exp, right, ExpressionType.StarStar);
            }
            return exp;
        }

        public Expression VisitUnaryExpression()
        {
            // todo await, typeof, delete 
            Expression exp;
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.PrefixPlusPlus);
                    break;
                case TokenType.MinusMinus:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.PrefixMinusMinus);
                    break;
                case TokenType.Bang:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.Bang);
                    break;
                case TokenType.Plus:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.Plus);
                    break;
                case TokenType.Minus:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.Minus);
                    break;
                default:
                    exp = VisitPostfixExpression();
                    break;
            }
            return exp;
        }

        /// <summary>
        /// Postfix Expression
        /// </summary>
        public Expression VisitPostfixExpression()
        {
            Expression exp = VisitLeftHandSideExpression();
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    exp = new UnaryExpression(exp, ExpressionType.PostfixPlusPlus);
                    MoveNext();
                    break;

                case TokenType.MinusMinus:
                    exp = new UnaryExpression(exp, ExpressionType.PostfixMinusMinus);
                    MoveNext();
                    break;
            }
            return exp;
        }

        /// <summary>
        /// Left side of expression
        /// </summary>
        public Expression VisitLeftHandSideExpression()
        {
            Expression exp = null;
            switch (TokenType)
            {
                case TokenType.Numeric:
                    exp = new LiteralExpression(GetNumeric());
                    break;
                case TokenType.String:
                    exp = new LiteralExpression(ReadString());
                    break;
                case TokenType.Identifier:
                    exp = VisitIdentifier();
                    break;
                case TokenType.SpecialVariable:
                    exp = new NameExpression(string.Concat("@", ReadVariableName()), ExpressionType.Identifier);
                    break;
                case TokenType.LeftBrace:
                    var list = VisitAnonymousObjectMembers();
                    CheckSyntaxExpected(TokenType.RightBrace);
                    exp = new AnonymousObjectExpression(list);
                    break;
                case TokenType.LeftParenthesis:
                    MoveNext();
                    exp = new UnaryExpression(VisitAssignmentExpression(), ExpressionType.Parenthesized);
                    CheckSyntaxExpected(TokenType.RightParenthesis);
                    break;
                case TokenType.LeftBracket:
                case TokenType.Less:
                    //Might be array
                    exp = VisitArrayLiteral();
                    break;
                case TokenType.RightParenthesis:
                case TokenType.RightBracket:
                    //skip end of expression
                    return exp;
            }
            MoveNext(false);
            //End of left
            return VisitRightExpression(exp);
        }

        /// <summary>
        /// Visit right side of expression
        /// </summary>
        public Expression VisitRightExpression(Expression exp)
        {
            for (TokenType type = TokenType; ; type = TokenType)
            {
                switch (type)
                {
                    case TokenType.LeftParenthesis:
                        var args = VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis);
                        exp = new InvocationExpression(exp, args);
                        CheckSyntaxExpected(TokenType.RightParenthesis);
                        break;
                    case TokenType.NullPropagator:
                        MoveNext();
                        exp = new NullPropegatorExpression(exp, VisitPostfixExpression());
                        return exp;
                    case TokenType.Qualified:
                    case TokenType.Dot:
                        MoveNext();
                        exp = new MemberExpression(exp, ReadVariableName(), (ExpressionType)type);
                        break;
                    case TokenType.LeftBracket:
                        args = VisitArgumentList(TokenType.Comma, TokenType.RightBracket);
                        exp = new IndexExpression(exp, args);
                        CheckSyntaxExpected(TokenType.RightBracket);
                        break;
                    default:
                        return exp;
                }
                MoveNext();
            }
        }

        /// <summary>
        /// Identifier expression like name, this, new etc
        /// </summary>
        /// <returns><see cref="NameExpression"/></returns>
        public Expression VisitIdentifier()
        {
            var name = ReadVariableName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.New:
                        MoveNext();
                        Expression[] arguments;
                        if (TokenType == TokenType.LeftParenthesis)
                        {
                            arguments = VisitArgumentList(TokenType.Comma, TokenType.LeftParenthesis).ToArray();
                            CheckSyntaxExpected(TokenType.RightParenthesis);
                        }
                        else
                        {
                            arguments = new Expression[0];
                        }
                        return new NewExpression(name, new NodeList<Expression>(arguments));
                    case IdentifierType.True:
                        return new LiteralExpression(true);
                    case IdentifierType.False:
                        return new LiteralExpression(false);
                    case IdentifierType.Null:
                        return Expression.Null;
#if Runtime
                case IdentifierType.Undefined:
                    return Expression.Undefined;
#endif
                    case IdentifierType.Function:
                        // ignore func
                        MoveNext();
                        var lamda = VisitLamdaExpression();
                        //to make last char not semicolon ex:func()=>1;
                        Source.FallBack();
                        return lamda;
                    case IdentifierType.This:
                        return new ThisExpression();
                    case IdentifierType.SizeOf:
                        //skip sizeof
                        if (MoveNextThenIf(TokenType.LeftParenthesis))
                        {
                            return new SizeOfExpression(VisitAssignmentExpression());
                        }
                        CheckSyntaxExpected(TokenType.RightParenthesis);
                        break;
                }
            }
            return new NameExpression(name, ExpressionType.Identifier);
        }

        /// <summary>
        /// format &lt;int&gt;(size)[1,2]
        /// </summary>
        /// <returns></returns>
        private Expression VisitArrayLiteral()
        {
            TypeSyntax type = null;
            Expression size = null;
            var list = new NodeList<Expression>();
            if (TokenType == TokenType.Less)
            {
                //Next <
                MoveNext();
                type = VisitType();
                //>
                MoveNextIf(TokenType.Greater);
                //(
                if (TokenType == TokenType.LeftParenthesis)
                {
                    MoveNext();
                    size = VisitAssignmentExpression();
                    MoveNextIf(TokenType.RightParenthesis);
                }

            }
            if (TokenType == TokenType.LeftBracket)
            {
                //[
                list = VisitArgumentList(TokenType.Comma, TokenType.RightBracket);
                CheckSyntaxExpected(TokenType.RightBracket);
                // next will go when enters right side

            }
            return new ArrayLiteralExpression(list, type, size);
        }

        /// <summary>
        /// Indicates <see cref="Parser.TokenType"/> matches the <paramref name="expected"/>
        /// </summary>
        protected bool CheckExpectedIdentifier(IdentifierType expected)
        {
            long start = Source.Position;
            var name = ReadVariableName();
            MoveNext();
            if (Keywords.Match(name, expected))
            {
                return true;
            }
            //Restore
            Source.SeekTo(start - 1);
            //do not skip line info
            MoveNext(false);
            return false;
        }

        /// <summary>
        /// Visit lamda expression ex:lamda()=>1;
        /// </summary>
        protected Expression VisitLamdaExpression()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                var args = VisitFunctionParameters();
                MoveNextIf(TokenType.RightParenthesis);
                TypeSyntax returnType = null;
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    returnType = VisitType();
                }
                return new AnonymousFunctionExpression(args, returnType, VisitBlock());
            }
            return Expression.Empty;
        }

        /// <summary>
        /// Don't Forget to Call ToArray
        /// </summary>
        /// <returns></returns>
        public NodeList<Expression> VisitArgumentList(TokenType splitToken, TokenType endToken)
        {
            var list = new NodeList<Expression>();
            while (MoveNextThenIfNot(endToken))
            {
                list.Add(VisitAssignmentExpression());
                if (TokenType == endToken)
                    break;
                CheckSyntaxExpected(splitToken);
            }
            return list;
        }

        /// <summary>
        /// Don't Forget to Call ToArray
        /// </summary>
        /// <returns></returns>
        public NodeList<Expression> VisitExpressionList(TokenType splitToken, TokenType endToken)
        {
            var list = new NodeList<Expression>();
            do
            {
                list.Add(VisitAssignmentExpression());
                if (TokenType == endToken)
                    break;
                CheckSyntaxExpected(splitToken);
            } while (MoveNext());
            return list;
        }

        /// <summary>
        /// Dont Forget to Call ToArray x:int
        /// </summary>
        /// <returns></returns>
        public NodeList<TypeParameter> VisitFunctionParameters()
        {
            var list = new NodeList<TypeParameter>();
            int index = 0;
            while (MoveNext())
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = ReadVariableName();
                    TypeSyntax type = null;
                    //var args
                    bool isVar = false;
                    MoveNext();
                    //a:int
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        type = VisitType();
                    }
                    //a...
                    if (TokenType == TokenType.Dot)
                    {
                        Skip(3);
                        isVar = true;
                    }
                    var parameter = new TypeParameter(name, type, index++, isVar);
                    Expression expression = null;
                    if (TokenType == TokenType.Equal)
                    {
                        MoveNext();
                        expression = VisitConditionalExpression();
                    }
                    parameter.DefaultValue = expression;

                    list.Add(parameter);
                }
                if (TokenType == TokenType.RightParenthesis)
                    break;
                //todo check if method arguments needs any scopes
                CheckSyntaxExpected(TokenType.Comma);
            }
            return list;
        }

        /// <summary>
        /// Visit type name
        /// </summary>
        public TypeSyntax VisitType()
        {
            TypeSyntax type = null;
            if (TokenType == TokenType.Identifier)
            {
                var typeName = ReadTypeName();
                //after type name next
                MoveNext();
                type = new RefTypeSyntax(typeName);
                //array
                if (TokenType == TokenType.LeftBracket)
                {
                    var sizes = VisitArrayRanks();
                    return new ArrayTypeSyntax(type, sizes);
                }
            }
            return type;
        }

        /// <summary>
        /// Visit Array indexes
        /// </summary>
        public NodeList<Expression> VisitArrayRanks()
        {
            var list = new NodeList<Expression>();
            while (TokenType == TokenType.LeftBracket)
            {
                MoveNext();
                list.Add(VisitExpression());
                if (TokenType == TokenType.RightBracket)
                    MoveNext();
            }
            return list;

        }

        /// <summary>
        /// Skip tokens
        /// </summary>
        public void Skip(int count)
        {
            for (int i = 0; i < count; i++)
            {
                MoveNext();
            }
        }

        /// <summary>
        /// {x:1, add()=> {}}
        /// </summary>
        /// <returns></returns>
        public NodeList<AnonymousObjectMember> VisitAnonymousObjectMembers()
        {
            var list = new NodeList<AnonymousObjectMember>();
            CheckSyntaxExpected(TokenType.LeftBrace);
            while (MoveNext())
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = ReadVariableName();
                    MoveNext();
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        Expression exp = TokenType == TokenType.LeftParenthesis ? VisitLamdaExpression() : VisitConditionalExpression();
                        list.Add(new AnonymousObjectMember(name, exp));
                    }
                }
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.Comma);
            }
            return list;
        }

        /// <summary>
        /// Visit local variable declaration
        /// </summary>
        public NodeList<VariableDeclarationExpression> VisitVarDeclarations()
        {
            var list = new NodeList<VariableDeclarationExpression>();
            //var or field
            do
            {
                CheckSyntaxExpected(TokenType.Identifier);
                string name = ReadVariableName();
                MoveNext();
                TypeSyntax type = null;
                Expression expression = null;
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    type = VisitType();
                }
                if (TokenType == TokenType.Equal)
                {
                    MoveNext();
                    expression = VisitAssignmentExpression();
                }
                list.Add(new VariableDeclarationExpression(name, type, expression));
                if (TokenType == TokenType.SemiColon || TokenType == TokenType.NewLine)
                    break;
                CheckSyntaxExpected(TokenType.Comma);
            } while (MoveNext());
            return list;
        }

        internal void CheckSyntaxExpected(TokenType type)
        {
            if (TokenType == type)
                return;
            throw new System.Exception(string.Concat("Invalid token ", c, " at ", Source.Position));
        }

        internal void CheckSyntaxExpected(TokenType type1, TokenType type2)
        {
            if (TokenType == type1 || TokenType == type2)
                return;
            throw new System.Exception(string.Concat("Invalid token ", c, " at ", Source.Position));
        }
        #endregion

        #region Interger
        private object GetNumeric()
        {
            char first = c;
            char next = Source.PeekChar();
            if (first == '0')
            {
                if (next == 'x' || next == 'X')
                    return CreateHexIntegerLiteral(first);
                else
                {
                    switch (next)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                            return CreateOctalIntegerLiteral(first);
                    }//else continue to make a numerical literal token
                }
            }
            return ReadNumber(first);
        }

        private object CreateOctalIntegerLiteral(char first)
        {
            var cb = new Utils.CharBuilder();
            cb.Append(first);//0
            double val = 0;

            while (Source.CanAdvance)
            {
                char next = Source.PeekChar();
                switch (next)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                        {
                            cb.Append(next);
                            val = val * 8 + next - '0';
                            Source.ReadChar();
                            continue;
                        }
                }
                break;
            }
            return int.Parse(cb.ToString());
        }

        private object CreateHexIntegerLiteral(char first)
        {
            Utils.CharBuilder cb = new Utils.CharBuilder();
            cb.Append(first);
            cb.Append(Source.ReadChar());//x or X (ever tested before)
            double val = 0;
            while (Source.CanAdvance)
            {
                char next = Source.PeekChar();
                switch (next)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            cb.Append(next);
                            val = val * 16 + next - '0';
                            Source.ReadChar();
                            continue;
                        }
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        {
                            cb.Append(next);
                            val = val * 16 + next - 'a' + 10;
                            Source.ReadChar();
                            continue;
                        }
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        {
                            cb.Append(next);
                            val = val * 16 + next - 'A' + 10;
                            Source.ReadChar();
                            continue;
                        }
                }
                break;
            }
            return int.Parse(cb.ToString());
        }

        private object ReadNumber(char first)
        {
            Utils.CharBuilder builder = new Utils.CharBuilder();
            builder.Append(first);
            int dot = 0;
            int exp = 0;
            while (Source.CanAdvance)
            {
                char next = Source.PeekChar();
                switch (next)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            builder.Append(next);
                            Source.ReadChar();
                            continue;
                        }
                    case DotChar:
                        //skip .
                        Source.ReadChar();
                        next = Source.PeekChar();
                        if (char.IsDigit(next))
                        {
                            dot++;
                            //add .
                            builder.Append(DotChar);
                            builder.Append(next);
                            //skip digit
                            Source.ReadChar();
                            if (dot > 1)
                            {
                                break;
                            }
                            continue;
                        }
                        Source.FallBack();
                        break;
                    case 'e':
                    case 'E':
                        {
                            builder.Append(next);
                            c = Source.ReadChar();
                            exp++;
                            if (exp > 1)
                            {
                                break;
                            }
                            next = Source.PeekChar();
                            if (next == '+' || next == '-')
                            {
                                builder.Append(next);
                                Source.ReadChar();
                            }
                            continue;
                        }
                }
                break;
            }
            if (dot > 0)
            {
                return double.Parse(builder.ToString(), Settings.NumberStyle, Settings.FormatProvider);
            }
            else
            {
                return int.Parse(builder.ToString(), System.Globalization.NumberStyles.Integer, Settings.FormatProvider);
            }
        }
        #endregion

        #region String

        public string ReadTypeName()
        {
            Utils.CharBuilder cb = new Utils.CharBuilder();
            //todo use do while 
            for (; char.IsLetterOrDigit(c) || c == '_' || c == '.'; c = Source.ReadChar())
            {
                cb.Append(c);
            }
            c = Source.FallBack();
            return cb.ToString();
        }

        public string ReadVariableName()
        {
            Utils.CharBuilder cb = new Utils.CharBuilder();
            for (; char.IsLetterOrDigit(c) || c == '_'; c = Source.ReadChar())
            {
                cb.Append(c);
            }
            c = Source.FallBack();
            return cb.ToString();
        }

        public string ReadString()
        {
            Utils.CharBuilder cb = new Utils.CharBuilder();
            for (; c != '`'; c = Source.ReadChar())
            {
                cb.Append(c);
            }
            return cb.ToString();
        }

        #endregion
    }
}
