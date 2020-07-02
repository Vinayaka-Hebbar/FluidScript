using FluidScript.Compiler.Debugging;
using FluidScript.Compiler.Lexer;
using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Compiler
{
    public class Parser : System.IDisposable
    {
        static readonly string[] Empty = new string[0];

        public const char OpenBrace = '{';
        public const char CloseBrace = '}';

        const char DotChar = '.';
        const char EscapeChar = '\\';
        const char QuoatedChar = '`';
        const char QuoatedCharString = '\'';
        readonly Utils.CharBuilder cb;

        /// <summary>
        /// Source text
        /// </summary>
        public readonly ITextSource Source;

        private List<string> _currentLabels;

        /// <summary>
        /// Token type
        /// </summary>
        protected TokenType TokenType;
        private char c;

        /// <summary>
        /// Parse settings
        /// </summary>
        public readonly ParserSettings Settings;

        /// <summary>
        /// Initializes new <see cref="ScriptParser"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="settings"></param>
        public Parser(ITextSource source, ParserSettings settings)
        {
            Source = source;
            Settings = settings;
            cb = new Utils.CharBuilder();
        }

        /// <summary>
        /// labels like start: and goto start
        /// </summary>
        public string[] CurrentLabels
        {
            //todo for inner block and forgotted
            get
            {
                if (_currentLabels == null)
                    return Empty;
                return _currentLabels.ToArray();
            }
        }

        public IList<string> Labels
        {
            get
            {
                if (_currentLabels == null)
                    _currentLabels = new List<string>();
                return _currentLabels;
            }
        }

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
            return TokenType != TokenType.Bad;
        }

        protected bool MoveNextThenIf(TokenType token)
        {
            return MoveNext() && TokenType == token;
        }

        protected bool MoveNextThenIfNot(TokenType token)
        {
            return MoveNext() && TokenType != token;
        }

        public bool MoveNextIf(TokenType token)
        {
            if (TokenType == token)
                return MoveNext();
            return false;
        }

        /// <summary>
        /// Reset source position
        /// </summary>
        public void Reset()
        {
            Source.Reset();
        }

        #region TokenType
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
                case OpenBrace:
                    return TokenType.LeftBrace;
                case CloseBrace:
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
                case QuoatedChar:
                    c = Source.ReadChar();
                    return TokenType.String;
                case QuoatedCharString:
                    c = Source.ReadChar();
                    return TokenType.StringQuoted;
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
                case DotChar:
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
                case '~':
                    return TokenType.Tilda;
                case '_':
                    if (char.IsLetter(n))
                    {
                        return TokenType.Identifier;
                    }
                    break;
                case '\\':
                    if (n == 'u')
                    {
                        // unicode char
                        Source.ReadChar();
                        string esp = System.Text.RegularExpressions.Regex.Unescape("\\u" + new string(Take(4)));
                        if (esp.Length == 1)
                        {
                            c = esp[0];
                            return GetTokenType(skipLine);
                        }
                        throw new System.Exception("expected unicode at " + Source.LineInfo);
                    }
                    return TokenType.BackSlash;
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
                        c = Source.ReadChar();
                        if (skipLine)
                        {
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

        #region Statement

        /// <summary>
        /// Creates <see cref="Statement"/> for <see cref="Source"/>
        /// </summary>
        /// <returns>Parse <see cref="Statement"/></returns>
        public Statement GetStatement()
        {
            if (MoveNext())
                return VisitStatement();
            return Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Expression"/> for <see cref="Source"/>
        /// </summary>
        /// <returns>Parse <see cref="Expression"/></returns>
        public Expression GetExpression()
        {
            if (MoveNext())
                return VisitExpression();
            return Expression.Empty;
        }
        /// <summary>
        /// Statement 
        /// </summary>
        public Statement VisitStatement()
        {
            TextPosition start = Source.LineInfo;
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
            statement.Span = new TextSpan(start, Source.LineInfo);
            return statement;
        }

        private Statement VisitIdentifierStatement()
        {
            long start = Source.Position;
            ReadVariableName(out string name);
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
                            ReadVariableName(out target);
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
            ReadVariableName(out string name);
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
        protected IfStatement VisitIfStatement()
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
                // stop the position if else position not found restore to this positon
                long start = Source.Position;
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                if (TokenType == TokenType.Identifier)
                {
                    ReadVariableName(out string name);
                    if (Keywords.Match(name, IdentifierType.Else))
                    {
                        MoveNext();
                        return new IfStatement(expression, body, VisitStatement());
                    }
                }

                //Restore
                Source.SeekTo(start - 1);
                //do not skip line info
                MoveNext(false);
                return new IfStatement(expression, body, null);
            }
            throw new System.Exception($"Syntax Error at line {Source.LineInfo}");
        }

        /// <summary>
        /// Visit local function
        /// </summary>
        /// <returns></returns>
        public Statement VisitLocalFunction()
        {
            if (TokenType == TokenType.Identifier)
            {
                ReadVariableName(out string name);
                MoveNext();
                TypeParameter[] parameterList;
                IEnumerable<TypeParameter> parameters = System.Linq.Enumerable.Empty<TypeParameter>();
                if (TokenType == TokenType.LeftParenthesis)
                    parameters = VisitFunctionParameters();
                parameterList = System.Linq.Enumerable.ToArray(parameters);
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

        public Statement VisitExpressionStatement()
        {
            Expression expression = VisitExpression();
            return new ExpressionStatement(expression);
        }

        public BlockStatement VisitAnonymousBlock()
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

        /// <summary>
        /// Visit block {}
        /// </summary>
        public BlockStatement VisitBlock()
        {
            //clear labels
            var start = Source.LineInfo;
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
            statement.Span = new TextSpan(start, Source.LineInfo);
            return statement;
        }
        #endregion

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
                case TokenType.Tilda:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.Tilda);
                    break;
                case TokenType.Minus:
                    MoveNext();
                    exp = new UnaryExpression(VisitLeftHandSideExpression(), ExpressionType.Minus);
                    break;
                case TokenType.Less:
                    MoveNext();
                    var type = VisitType();
                    if (TokenType == TokenType.Greater || TokenType == TokenType.GreaterGreater)
                        MoveNext();
                    exp = new ConvertExpression(type, VisitLeftHandSideExpression());
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
                    ReadString(out string s);
                    exp = new LiteralExpression(s);
                    break;
                case TokenType.StringQuoted:
                    ReadStringQuoted(out s);
                    exp = new LiteralExpression(s);
                    break;
                case TokenType.Identifier:
                    exp = VisitIdentifier();
                    break;
                case TokenType.SpecialVariable:
                    // char is string
                    ReadVariableName(out s);
                    exp = new NameExpression(string.Concat("@", s), ExpressionType.Identifier);
                    break;
                case TokenType.LeftBrace:
                    var list = VisitAnonymousObjectMembers();
                    exp = new AnonymousObjectExpression(list);
                    CheckSyntaxExpected(TokenType.RightBrace);
                    break;
                case TokenType.LeftParenthesis:
                    MoveNext();
                    exp = new UnaryExpression(VisitExpression(), ExpressionType.Parenthesized);
                    CheckSyntaxExpected(TokenType.RightParenthesis);
                    break;
                case TokenType.LeftBracket:
                    //Might be array
                    exp = VisitArrayLiteral();
                    return VisitRightExpression(exp);
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
                        // after . identifier
                        ReadVariableName(out string s);
                        exp = new MemberExpression(exp, s, (ExpressionType)type);
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
            ReadVariableName(out string name);
            if (Keywords.TryGetIdentifier(name, out IdentifierType identifier))
            {
                switch (identifier)
                {
                    case IdentifierType.New:
                        MoveNext();
                        TypeSyntax type = VisitType();
                        Expression[] arguments;
                        if (TokenType == TokenType.LeftParenthesis)
                        {
                            arguments = VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis).ToArray();
                            CheckSyntaxExpected(TokenType.RightParenthesis);
                        }
                        else
                        {
                            arguments = new Expression[0];
                        }
                        return new NewExpression(type, new NodeList<Expression>(arguments));
                    case IdentifierType.True:
                        return Expression.True;
                    case IdentifierType.False:
                        return Expression.False;
                    case IdentifierType.Null:
                        return Expression.Null;
                    case IdentifierType.NaN:
                        return Expression.NaN;
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
                    case IdentifierType.Super:
                        return new SuperExpression();
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
        /// format [1,2]&lt;int&gt;(size) or [1,2]&lt;int&gt;
        /// </summary>
        /// <returns></returns>
        private Expression VisitArrayLiteral()
        {
            TypeSyntax type = null;
            var list = new NodeList<Expression>();
            NodeList<Expression> arguments = null;
            //[
            if (TokenType == TokenType.LeftBracket)
            {
                //[
                list = VisitArgumentList(TokenType.Comma, TokenType.RightBracket);
                MoveNextIf(TokenType.RightBracket);
                // next will go when enters right side
            }
            if (TokenType == TokenType.Less)
            {
                //Next <
                MoveNext();
                type = VisitType();
                //>
                MoveNextIf(TokenType.Greater);
            }
            //(
            if (TokenType == TokenType.LeftParenthesis)
            {
                arguments = VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis);
                MoveNextIf(TokenType.RightParenthesis);
            }
            return new ArrayListExpression(list, type, arguments);
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
                    ReadVariableName(out string name);
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
                ReadTypeName(out string typeName);
                //after type name next
                MoveNext();
                type = new RefTypeSyntax(typeName, TokenType == TokenType.Less ? VisitTypes(TokenType.Comma, TokenType.GreaterGreater) : null);
                // array
                if (TokenType == TokenType.LeftBracket)
                {
                    var sizes = VisitArrayRanks();
                    return new ArrayTypeSyntax(type, sizes);
                }
            }
            return type;
        }

        protected INodeList<TypeSyntax> VisitTypes(TokenType seperator, TokenType end)
        {
            var list = new NodeList<TypeSyntax>();
            while (MoveNext())
            {
                list.Add(VisitType());
                if (TokenType == end)
                    break;
                CheckSyntaxExpected(seperator);
            }
            return list;
        }

        /// <summary>
        /// Visit Array indexes
        /// </summary>
        public int VisitArrayRanks()
        {
            int rank = 0;
            while (TokenType == TokenType.LeftBracket)
            {
                MoveNext();
                rank++;
                if (TokenType == TokenType.RightBracket)
                    MoveNext();
            }
            return rank;

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
        /// Take specified chars
        /// </summary>
        private char[] Take(int count)
        {
            char[] res = new char[count];
            for (int i = 0; i < count; i++)
            {
                res[i] = Source.ReadChar();
            }
            return res;
        }

        /// <summary>
        /// {x:1, add()=> {}}
        /// </summary>
        /// <returns></returns>
        public NodeList<AnonymousObjectMember> VisitAnonymousObjectMembers()
        {
            var list = new NodeList<AnonymousObjectMember>();
            while (MoveNext())
            {
                if (TokenType == TokenType.Identifier)
                {
                    ReadVariableName(out string name);
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
                ReadVariableName(out string name);
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
            } while (MoveNextIf(TokenType.Comma));
            return list;
        }

        internal void CheckSyntaxExpected(TokenType type)
        {
            if (TokenType == type)
                return;
            throw new System.Exception(string.Concat("Invalid token ", c, " at ", Source.LineInfo, " expected ", type));
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
            object value;
            char first = c;
            char next = Source.PeekChar();
            if (first == '0')
            {
                if (next == 'x' || next == 'X')
                {
                    CreateHexIntegerLiteral(first, out value);
                    return value;
                }
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
                            CreateOctalIntegerLiteral(first, out value);
                            return value;
                    }//else continue to make a numerical literal token
                }
            }
            ReadNumber(first, out value);
            return value;
        }

        private void CreateOctalIntegerLiteral(char first, out object value)
        {
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
            value = int.Parse(cb.ToString());
            cb.Length = 0;
        }

        private void CreateHexIntegerLiteral(char first, out object value)
        {
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
            value = int.Parse(cb.ToString());
            cb.Length = 0;
        }

        private void ReadNumber(char first, out object value)
        {
            cb.Append(first);
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
                        cb.Append(next);
                        Source.ReadChar();
                        continue;
                    case DotChar:
                        //skip .
                        Source.ReadChar();
                        next = Source.PeekChar();
                        if (char.IsDigit(next))
                        {
                            dot++;
                            //add .
                            cb.Append(DotChar);
                            cb.Append(next);
                            //skip digit
                            if (dot > 1)
                            {
                                break;
                            }
                            Source.ReadChar();
                            continue;
                        }
                        Source.FallBack();
                        break;
                    case 'e':
                    case 'E':
                        cb.Append(next);
                        c = Source.ReadChar();
                        exp++;
                        if (exp > 1)
                        {
                            break;
                        }
                        next = Source.PeekChar();
                        if (next == '+' || next == '-')
                        {
                            cb.Append(next);
                            Source.ReadChar();
                        }
                        continue;
                }
                break;
            }
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
                        cb.Append(next);
                        Source.ReadChar();
                        continue;
                    case DotChar:
                        //skip .
                        Source.ReadChar();
                        next = Source.PeekChar();
                        if (char.IsDigit(next))
                        {
                            dot++;
                            //add .
                            cb.Append(DotChar);
                            cb.Append(next);
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
                        cb.Append(next);
                        c = Source.ReadChar();
                        exp++;
                        if (exp > 1)
                        {
                            break;
                        }
                        next = Source.PeekChar();
                        if (next == '+' || next == '-')
                        {
                            cb.Append(next);
                            Source.ReadChar();
                        }
                        continue;
                }
                break;
            }
            if (dot > 0)
            {
                value = double.Parse(cb.ToString(), Settings.NumberStyle, Settings.FormatProvider);
            }
            else
            {
                if (cb.Length < 9)
                    value = int.Parse(cb.ToString(), System.Globalization.NumberStyles.Integer, Settings.FormatProvider);
                else
                    value = long.Parse(cb.ToString(), System.Globalization.NumberStyles.Integer, Settings.FormatProvider);
            }
            cb.Length = 0;
        }
        #endregion

        #region String

        public void ReadTypeName(out string name)
        {
            //already know c is identifier
            cb.Append(c);
            //todo use do while 
            for (; ; )
            {
                c = Source.PeekChar();
                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                {
                    cb.Append(c);
                    c = Source.ReadChar();
                    continue;
                }
                break;
            }
            name = cb.ToString();
            cb.Length = 0;
        }

        public void ReadVariableName(out string name)
        {
            // we already know c is identifier
            cb.Append(c);
            for (; ; )
            {
                // next char
                c = Source.PeekChar();
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    cb.Append(c);
                    Source.ReadChar();
                    continue;
                }
                break;
            }
            name = cb.ToString();
            cb.Length = 0;
        }

        public void ReadString(out string s)
        {
            for (; c != QuoatedChar; c = Source.ReadChar())
            {
                if (c == EscapeChar && Source.PeekChar() == QuoatedChar)
                    c = Source.ReadChar();
                cb.Append(c);
            }
            s = cb.ToString();
            cb.Length = 0;
        }

        public void ReadStringQuoted(out string s)
        {
            for (; c != QuoatedCharString; c = Source.ReadChar())
            {
                if (c == EscapeChar && Source.PeekChar() == QuoatedCharString)
                    c = Source.ReadChar();
                cb.Append(c);
            }
            s = cb.ToString();
            cb.Length = 0;
        }

        #endregion

        #region Static

        /// <summary>
        /// Creates <see cref="Statement"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parse <see cref="Statement"/></returns>
        public static Statement GetStatement(string text, ParserSettings settings = null)
        {
            using (Parser visitor = new Parser(new TextSource(text), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.VisitStatement();
            }
            return Statement.Empty;
        }

        /// <summary>
        /// Creates <see cref="Expression"/> for <paramref name="text"/>
        /// </summary>
        /// <param name="text">Text to parse</param>
        /// <param name="settings">Parser options if null will be default options</param>
        /// <returns>Parse <see cref="Expression"/></returns>
        public static Expression GetExpression(string text, ParserSettings settings = null)
        {
            using (Parser visitor = new Parser(new TextSource(text), settings ?? ParserSettings.Default))
            {
                if (visitor.MoveNext())
                    return visitor.VisitExpression();
            }
            return Expression.Empty;
        }
        #endregion
    }
}
