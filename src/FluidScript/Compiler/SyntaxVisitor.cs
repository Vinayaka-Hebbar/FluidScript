using FluidScript.Compiler.SyntaxTree;
using FluidScript.Library;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Syntax Visiitor which will tokenizes the text
    /// </summary>
    public class SyntaxVisitor : System.IDisposable
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
        private Reflection.Modifiers modifiers;

        /// <summary>
        /// Parse settings
        /// </summary>
        public readonly ParserSettings Settings;

        /// <summary>
        /// Initializes new <see cref="SyntaxVisitor"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="settings"></param>
        public SyntaxVisitor(IScriptSource source, ParserSettings settings)
        {
            Source = source;
            Settings = settings;
        }

        /// <summary>
        /// labels like start: and goto start
        /// </summary>
        //todo for inner block
        public string[] CurrentLabels
        {
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
        /// Move to next <see cref="TokenType"/>
        /// </summary>
        public bool MoveNext(bool skipLine = true)
        {
            c = Source.ReadChar();
            TokenType = GetTokenType(skipLine);
            return c != char.MinValue;
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
#if Emit
                        c = Source.ReadChar();
                        return TokenType.Constant;
#else
                        return TokenType.Identifier;
#endif
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
        public IEnumerable<MemberDeclaration> VisitMembers()
        {
            while (TokenType != TokenType.RightBrace)
            {
                MemberDeclaration member = VisitMember();
                yield return member;
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
            }
        }

        /// <summary>
        /// Visit member declaration
        /// </summary>
        public MemberDeclaration VisitMember()
        {
            //reset modifier
            modifiers = Reflection.Modifiers.None;
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
            var name = GetName();
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
                        modifiers |= Reflection.Modifiers.ReadOnly;
                        return VisitFieldDeclaration();
                    case IdentifierType.Implement:
                        //todo has set
                        modifiers |= Reflection.Modifiers.Implement;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Static:
                        modifiers |= Reflection.Modifiers.Static;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Private:
                        modifiers |= Reflection.Modifiers.Private;
                        MoveNext();
                        return VisitIdentifierMember();
                    case IdentifierType.Get:
                        MoveNext();
                        modifiers |= Reflection.Modifiers.Getter;
                        return VisitFunctionDeclaration();
                    case IdentifierType.Set:
                        MoveNext();
                        modifiers |= Reflection.Modifiers.Setter;
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
                var name = GetName();
                MoveNext();
                //todo extends
                if (TokenType == TokenType.LeftBrace)
                {
                    MoveNext();
                    var members = VisitMembers().ToArray();
                    if (TokenType == TokenType.RightBrace)
                        MoveNext();
                    return new TypeDeclaration(name, null, new TypeSyntax[0], members)
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
            var declarations = VisitVarDeclarations().ToArray();
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
                var name = GetName();
                MoveNext();
                TypeParameter[] parameterList;
                //return type
                IEnumerable<TypeParameter> parameters = Enumerable.Empty<TypeParameter>();
                if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    parameters = VisitFunctionParameters();
                parameterList = parameters.ToArray();
                if (CheckSyntaxExpected(TokenType.RightParenthesis))
                    MoveNext();

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
            var start = Source.CurrentPosition;
            Statement statement;
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    MoveNext();
                    var statements = VisitListStatement().ToArray();
                    if (TokenType == TokenType.RightBrace)
                        MoveNext();
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
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                MoveNext();
                switch (type)
                {
                    case IdentifierType.Return:
                        Expression expression = null;
                        if (!CheckSyntaxExpected(TokenType.SemiColon))
                        {
                            expression = VisitExpression();
                        }
                        return new ReturnOrThrowStatement(expression, StatementType.Return);
                    case IdentifierType.Var:
                        //any type
                        var declarations = VisitVarDeclarations().ToArray();
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
                            target = GetName();
                            MoveNext();
                        }

                        return new ContinueStatement(target);
                }
            }
            //default label statment
            //restore to prev
            Source.SeekTo(start - 1);
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
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                var start = Source.CurrentPosition;
                var condition = new ExpressionStatement(VisitExpression())
                {
                    Span = new TextSpan(start, Source.CurrentPosition)
                };
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                var increment = VisitStatement();
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
                    InitStatement = initialization,
                    ConditionStatement = condition,
                    IncrementStatement = increment
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
                var start = Source.CurrentPosition;
                var condition = new ExpressionStatement(VisitExpression())
                {
                    Span = new TextSpan(start, Source.CurrentPosition)
                };
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
                    ConditionStatement = condition
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
            var name = GetName();
            MoveNext();
            if (Keywords.Match(name, IdentifierType.While))
            {
                if (TokenType == TokenType.LeftParenthesis)
                {
                    MoveNext();
                    var start = Source.CurrentPosition;
                    var condition = new ExpressionStatement(VisitExpression())
                    {
                        Span = new TextSpan(start, Source.CurrentPosition)
                    };
                    if (TokenType == TokenType.RightParenthesis)
                        MoveNext();
                    if (TokenType == TokenType.SemiColon)
                        MoveNext();
                    return new LoopStatement(statement, StatementType.DoWhile)
                    {
                        ConditionStatement = condition
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
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
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
                string name = GetName();
                MoveNext();
                TypeParameter[] parameterList;
                IEnumerable<TypeParameter> parameters = Enumerable.Empty<TypeParameter>();
                if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    parameters = VisitFunctionParameters();
                parameterList = parameters.ToArray();
                if (CheckSyntaxExpected(TokenType.RightParenthesis))
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
                MoveNext();
                Statement[] statements = VisitListStatement().ToArray();
                if (TokenType == TokenType.RightBrace)
                    MoveNext();
                statement = new BlockStatement(statements, CurrentLabels);
            }
            else if (CheckSyntaxExpected(TokenType.AnonymousMethod))
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
                MoveNext();
                Statement[] statements = VisitListStatement().ToArray();
                if (TokenType == TokenType.RightBrace)
                    MoveNext();
                return new BlockStatement(statements, CurrentLabels);
            }
            BlockStatement blockStatement = new BlockStatement(new Statement[] { new ReturnOrThrowStatement(VisitConditionalExpression(), StatementType.Return) }, CurrentLabels);
            return blockStatement;
        }

        public IEnumerable<Statement> VisitListStatement()
        {
            while (TokenType != TokenType.RightBrace && TokenType != TokenType.End)
            {
                yield return VisitStatement();
                if (TokenType == TokenType.SemiColon || TokenType == TokenType.NewLine)
                    MoveNext();
            }
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

        public Expression VisitAssignmentExpression()
        {
            Expression exp = VisitConditionalExpression();
            return VisitAssignmentExpression(exp);
        }

        private Expression VisitAssignmentExpression(Expression exp)
        {
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
            Expression exp = VisitUnaryExpression();
            for (TokenType type = TokenType;
                type == TokenType.Multiply || type == TokenType.Divide || type == TokenType.Percent;
                type = TokenType)
            {
                MoveNext();
                Expression right = VisitUnaryExpression();
                exp = new BinaryExpression(exp, right, (ExpressionType)type);
            }
            return exp;
        }

        public Expression VisitUnaryExpression()
        {
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
                    exp = new LiteralExpression(GetString());
                    break;
                case TokenType.Identifier:
                    exp = VisitIdentifier();
                    break;
                case TokenType.SpecialVariable:
                    exp = new NameExpression(string.Concat("@", GetName()), ExpressionType.Identifier);
                    break;
#if Emit
                case TokenType.Variable:
                    var name = GetName();
                    exp = new NameExpression(name, ExpressionType.Identifier);
                    break;
                case TokenType.Constant:
                    name = GetName();
                    exp = new NameExpression(name, ExpressionType.Identifier);
                    break;
#endif
                case TokenType.LeftBrace:
                    MoveNext();
                    var list = VisitAnonymousObjectMembers().ToArray();
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
            if (TokenType == TokenType.SemiColon)
                return exp;
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
                        MoveNext();
                        var args = VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis).ToArray();
                        exp = new InvocationExpression(exp, args);
                        break;
                    case TokenType.NullPropagator:
                        MoveNext();
                        exp = new NullPropegatorExpression(exp, VisitPostfixExpression());
                        return exp;
                    case TokenType.Qualified:
                    case TokenType.Dot:
                        MoveNext();
                        exp = new MemberExpression(exp, GetName(), (ExpressionType)type);
                        break;
                    case TokenType.LeftBracket:
                        MoveNext();
                        args = VisitArgumentList(TokenType.Comma, TokenType.RightBracket).ToArray();
                        exp = new IndexExpression(exp, args);
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
            var name = GetName();
            if (!Keywords.TryGetIdentifier(name, out IdentifierType type))
                return new NameExpression(name, ExpressionType.Identifier);
            switch (type)
            {
                case IdentifierType.New:
                    MoveNext();
                    Expression[] arguments;
                    if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    {
                        MoveNext();
                        arguments = VisitArgumentList(TokenType.Comma, TokenType.LeftParenthesis).ToArray();
                    }
                    else
                    {
                        arguments = new Expression[0];
                    }
                    return new NewExpression(name, arguments);
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
                case IdentifierType.Lamda:
                    ReadVariableName();
                    MoveNext();
                    return VisitLamdaExpression();
                case IdentifierType.This:
                    return new ThisExpression();
            }
            return Expression.Empty;
        }


        /// <summary>
        /// format &lt;int&gt;(size)[1,2]
        /// </summary>
        /// <returns></returns>
        private Expression VisitArrayLiteral()
        {
            TypeSyntax type = null;
            Expression size = null;
            Expression[] list = Expression.EmptyList;
            if (TokenType == TokenType.Less)
            {
                //Next <
                MoveNext();
                type = VisitType();
                //>
                if (TokenType == TokenType.Greater)
                    MoveNext();
                //(
                if (TokenType == TokenType.LeftParenthesis)
                {
                    MoveNext();
                    size = VisitAssignmentExpression();
                    if (TokenType == TokenType.RightParenthesis)
                        MoveNext();
                }

            }
            if (CheckSyntaxExpected(TokenType.LeftBracket))
            {
                //[
                MoveNext();
                list = VisitArgumentList(TokenType.Comma, TokenType.RightBracket).ToArray();

            }
            return new ArrayLiteralExpression(list, type, size);
        }

        /// <summary>
        /// Indicates <see cref="TokenType"/> matches the <paramref name="expected"/>
        /// </summary>
        protected bool CheckExpectedIdentifier(IdentifierType expected)
        {
            long start = Source.Position;
            var name = GetName();
            MoveNext();
            if (Keywords.Match(name, expected))
            {
                return true;
            }
            //Restore
            Source.SeekTo(start - 1);
            MoveNext();
            return false;
        }

        /// <summary>
        /// Visit lamda expression ex:lamda()=>1;
        /// </summary>
        protected Expression VisitLamdaExpression()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                MoveNext();
                var args = VisitFunctionParameters().ToArray();
                if (TokenType == TokenType.RightParenthesis)
                {
                    MoveNext();
                }
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
        public IEnumerable<Expression> VisitArgumentList(TokenType splitToken, TokenType endToken)
        {
            for (TokenType type = TokenType; type != endToken; type = TokenType)
            {
                Expression exp = VisitAssignmentExpression();
                yield return exp;
                if (TokenType == splitToken)
                    MoveNext();
            }
        }

        /// <summary>
        /// List of expression sperated by <paramref name="splitToken"/>
        /// </summary>
        public IEnumerable<Expression> VisitExpressionList(TokenType splitToken, TokenType endToken)
        {
            for (TokenType type = TokenType; type != endToken; type = TokenType)
            {
                Expression exp = type == splitToken ? Expression.Empty : VisitExpression();
                yield return exp;
                if (TokenType == splitToken)
                    MoveNext();
            }
        }

        /// <summary>
        /// Dont Forget to Call ToArray x:int
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TypeParameter> VisitFunctionParameters()
        {
            int index = 0;
            if (TokenType == TokenType.LeftParenthesis)
                MoveNext();
            while (TokenType != TokenType.RightParenthesis)
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = GetName();
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
                    //todo check if method arguments needs any scopes
                    if (TokenType == TokenType.Comma)
                        MoveNext();

                    yield return parameter;
                }
            }
        }

        /// <summary>
        /// Visit type name
        /// </summary>
        public TypeSyntax VisitType()
        {
            TypeSyntax type = null;
            if (TokenType == TokenType.Identifier)
            {
                var typeName = GetTypeName();
                //after type name next
                MoveNext();
                type = new RefTypeSyntax(typeName);
                //array
                if (TokenType == TokenType.LeftBracket)
                {
                    var sizes = VisitArrayRanks().ToArray();
                    return new ArrayTypeSyntax(type, sizes);
                }
            }
            return type;
        }

        /// <summary>
        /// Visit Array indexes
        /// </summary>
        public IEnumerable<Expression> VisitArrayRanks()
        {
            while (TokenType == TokenType.LeftBracket)
            {
                MoveNext();
                yield return VisitExpression();
                if (TokenType == TokenType.RightBracket)
                    MoveNext();
            }
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
        public IEnumerable<AnonymousObjectMember> VisitAnonymousObjectMembers()
        {
            while (TokenType != TokenType.RightBrace)
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = GetName();
                    MoveNext();
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        Expression exp = TokenType == TokenType.LeftParenthesis ? VisitLamdaExpression() : VisitConditionalExpression();
                        yield return new AnonymousObjectMember(name, exp);
                    }
                }
                if (TokenType == TokenType.Comma)
                    MoveNext();
            }
        }

        /// <summary>
        /// Visit local variable declaration
        /// </summary>
        public IEnumerable<VariableDeclarationExpression> VisitVarDeclarations()
        {
            while (TokenType != TokenType.SemiColon && TokenType != TokenType.NewLine)
            {
                if (CheckSyntaxExpected(TokenType.Identifier))
                {
                    string name = GetName();
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
                    yield return new VariableDeclarationExpression(name, type, expression);
                }
                if (TokenType == TokenType.Comma)
                    MoveNext();
            }
        }

        internal bool CheckSyntaxExpected(TokenType type)
        {
            return TokenType == type ? true : false;
        }
        #endregion

        #region Interger
        private object GetNumeric()
        {
            Source.FallBack();
            char first = Source.ReadChar();
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
            return ReadInterger(first);

        }

        private IEnumerable<char> CreateOctalIntegerLiteral(char first)
        {
            yield return first;//0
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
                            yield return next;
                            val = val * 8 + next - '0';
                            Source.ReadChar();
                            continue;
                        }
                }
                break;
            }
        }

        private IEnumerable<char> CreateHexIntegerLiteral(char first)
        {
            yield return first;
            yield return Source.ReadChar(); //x or X (ever tested before)
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
                            yield return next;
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
                            yield return next;
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
                            yield return next;
                            val = val * 16 + next - 'A' + 10;
                            Source.ReadChar();
                            continue;
                        }
                }
                break;
            }
        }

        private object ReadInterger(char first)
        {
            CharEnumerable builder = new CharEnumerable(first);
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
                if (double.TryParse(builder.ToString(), Settings.NumberStyle, Settings.FormatProvider, out double result))
                {
                    return result;
                }
            }
            else
            {
                if (int.TryParse(builder.ToString(), System.Globalization.NumberStyles.Integer, Settings.FormatProvider, out int result))
                {
                    return result;
                }
            }
            return double.NaN;
        }
        #endregion

        #region String

        /// <summary>
        /// Get current string 
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return new string(ReadString().ToArray());
        }

        /// <summary>
        /// get type name Like System.Datetime
        /// </summary>
        /// <returns></returns>
        public string GetTypeName()
        {
            return new string(ReadTypeName().ToArray());
        }

        /// <summary>
        /// Read variable name
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return new string(ReadVariableName().ToArray());
        }

        private IEnumerable<char> ReadTypeName()
        {
            //todo use do while 
            for (; c != char.MinValue; c = Source.ReadChar())
            {
                if (char.IsLetterOrDigit(c) || c == '_' || c == '.')
                {
                    yield return c;
                    continue;
                }
                Source.FallBack();
                yield break;
            }
        }

        private IEnumerable<char> ReadVariableName()
        {
            for (; c != char.MinValue; c = Source.ReadChar())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    yield return c;
                    continue;
                }
                Source.FallBack();
                yield break;
            }
        }

        private IEnumerable<char> ReadString()
        {
            for (; c != '`'; c = Source.ReadChar())
            {
                yield return c;
            }
        }

        #endregion

        private class CharEnumerable : IEnumerable<char>
        {
            private IEnumerable<char> _enumerable;
            private int length;

            public CharEnumerable(char c)
            {
                length = 1;
                _enumerable = new char[] { c };
            }

            public void Append(char c)
            {
                _enumerable = _enumerable.Concat(Concat(c));
                length++;
            }

            public IEnumerator<char> GetEnumerator()
            {
                return _enumerable.GetEnumerator();
            }

            private IEnumerable<char> Concat(char c)
            {
                yield return c;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _enumerable.GetEnumerator();
            }

            public override string ToString()
            {
                return new string(_enumerable.ToArray());
            }

            public int Length => length;
        }
    }
}
