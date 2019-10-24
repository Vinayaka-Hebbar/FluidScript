using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.Scopes;
using FluidScript.Compiler.SyntaxTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public class SyntaxVisitor : IEnumerable<TokenType>, IEnumerator<TokenType>
    {
        internal static readonly Keywords InbuiltKeywords = new Keywords();
        public readonly string Text;
        public readonly int Length;
        public TokenType TokenType;
        private char c;
        private int pos;
        private int column = 0;
        //default 1
        private int line = 1;
        public Scopes.Scope Scope;
        internal readonly Keywords Keywords;
        public readonly ParserSettings Settings;

        public SyntaxVisitor(string text, Scopes.Scope initialScope, ParserSettings settings)
        {
            Text = text;
            Length = text.Length;
            Scope = initialScope;
            Settings = settings;
            Keywords = InbuiltKeywords;
        }

        #region Iterator
        public TokenType Current => TokenType;

        object IEnumerator.Current => TokenType;

        public void Dispose()
        {
            System.GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            if (pos >= Length)
            {
                c = char.MinValue;
                TokenType = TokenType.End;
                return false;
            }
            c = Text[pos++];
            TokenType = GetTokenType();
            return true;
        }

        public void Reset()
        {
            pos = 0;
        }

        public TokenType GetTokenType()
        {
            char n = PeekChar();
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
                case '@':
                    c = ReadChar();
                    return TokenType.Variable;
                case '?':
                    if (n == '?')
                    {
                        c = ReadChar();
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
                        c = ReadChar();
                        return TokenType.PlusPlus;
                    }
                    return TokenType.Plus;
                case '-':
                    if (n == '>')
                    {
                        c = ReadChar();
                        return TokenType.Initializer;
                    }
                    if (n == '-')
                    {
                        c = ReadChar();
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
                        c = ReadChar();
                        return TokenType.AndAnd;
                    }
                    return TokenType.And;
                case '|':
                    if (n == '|')
                    {
                        c = ReadChar();
                        return TokenType.OrOr;
                    }
                    return TokenType.Or;
                case '`':
                    c = ReadChar();
                    return TokenType.String;
                case ',':
                    return TokenType.Comma;
                case ';':
                    return TokenType.SemiColon;
                case ':':
                    if (n == ':')
                    {
                        c = ReadChar();
                        return TokenType.Qualified;
                    }
                    return TokenType.Colon;
                case '.':
                    return TokenType.Dot;
                case '=':
                    if (n == '>')
                    {
                        c = ReadChar();
                        return TokenType.AnnonymousMethod;
                    }
                    if (n == '=')
                    {
                        c = ReadChar();
                        return TokenType.EqualEqual;
                    }
                    return TokenType.Equal;
                case '<':
                    if (n == '=')
                    {
                        c = ReadChar();
                        return TokenType.LessEqual;
                    }
                    if (n == '<')
                    {
                        c = ReadChar();
                        return TokenType.LessLess;
                    }
                    return TokenType.Less;
                case '>':
                    if (n == '=')
                    {
                        c = ReadChar();
                        return TokenType.GreaterEqual;
                    }
                    if (n == '>')
                    {
                        c = ReadChar();
                        return TokenType.GreaterGreater;
                    }
                    return TokenType.Greater;
                case '!':
                    if (n == '=')
                    {
                        c = ReadChar();
                        return TokenType.BangEqual;
                    }
                    return TokenType.Bang;
                case '_':
                    if (char.IsLetter(n))
                    {
                        c = ReadChar();
                        return TokenType.Constant;
                    }
                    break;
                case '\\':
                    if (n == 'u')
                    {
                        //Unicode
                    }
                    break;
                case '\t':
                case '\r':
                case ' ':
                    c = ReadChar();
                    return GetTokenType();
                case '\n':
                    column = 0;
                    line++;
                    c = ReadChar();
                    return GetTokenType();
                default:
                    if (char.IsLetter(c))
                    {
                        return TokenType.Identifier;
                    }
                    break;
            }
            if (pos == Length)
                return TokenType.End;
            return TokenType.Bad;
        }
        #endregion

        protected void RestoreTo(SyntaxVisitor other)
        {
            pos = other.pos;
            line = other.line;
            c = other.c;
            TokenType = other.TokenType;
        }

        public IEnumerable<Statement> VisitProgram()
        {
            while (TokenType != TokenType.End)
            {
                yield return VisitStatement(CodeScope.Class);
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
            }
        }

        public char ReadChar()
        {
            if (pos >= Length)
                return char.MinValue;
            column++;
            return Text[pos++];
        }

        public char PeekChar()
        {
            if (pos >= Length)
                return char.MinValue;
            return Text[pos];
        }

        public bool CanAdvance => pos < Length;

        public Statement VisitStatement(CodeScope scope)
        {
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    return VisitBlock();
                case TokenType.Identifier:
                    return VisitIdentifierStatement(scope);
                case TokenType.SemiColon:
                    //Rare case
                    throw new System.Exception($"unexpected semicolon at line {line} , pos {pos}");
                default:
                    return VisitExpressionStatement();
            }
        }

        private Statement VisitIdentifierStatement(CodeScope scope)
        {
            int start = pos;
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Out:
                        MoveNext();
                        Expression expression = null;
                        if (!CheckSyntaxExpected(TokenType.SemiColon))
                        {
                            expression = VisitExpression();
                        }
                        return new ReturnOrThrowStatement(NodeType.Return, expression);
                    case IdentifierType.Var:
                        var declarations = VisitVarDeclations(TypeInfo.Object).ToArray();
                        return new VariableDeclarationStatement(declarations);
                    case IdentifierType.Function:
                        return VisitFunctionDefinition();
                    case IdentifierType.If:
                        return VisitIfStatement();
                    case IdentifierType.Else:
                        MoveNext();
                        return VisitStatement(CodeScope.Local);
                        //default label statment
                }
            }
            pos = start - 1;
            MoveNext();
            return VisitExpressionStatement();
        }

        protected Statement VisitIfStatement()
        {
            MoveNext();
            if (TokenType == TokenType.LeftParenthesis)
            {
                var expression = VisitExpression();
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                Statement body = VisitStatement(CodeScope.Local);
                int start = pos;
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                Statement other = null;
                if (CheckExpectedIdentifier(IdentifierType.Else))
                    other = VisitStatement(CodeScope.Local);
                else
                {
                    pos = start - 1;
                    MoveNext();
                }
                return new IfStatement(expression, body, other);
            }
            throw new System.Exception($"Syntax Error at line {line}, pos {pos}");
        }

        private FunctionDefinitionStatement VisitFunctionDefinition()
        {
            MoveNext();
            string name = null;
            if (CheckSyntaxExpected(TokenType.Identifier))
                name = GetName();
            MoveNext();
            using (var scope = Scope.DeclareMethodScope(name, this))
            {
                var arguments = Enumerable.Empty<ParameterInfo>();
                if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    arguments = VisitFunctionArguments();
                var argumentsList = arguments.ToArray();
                if (CheckSyntaxExpected(TokenType.RightParenthesis))
                    MoveNext();
                FunctionDeclarationStatement declaration = new FunctionDeclarationStatement(name, argumentsList);

                //To avoid block function
                BlockStatement body = VisitBlock();
                scope.Close();
                return new FunctionDefinitionStatement(declaration, body);
            }

        }

        private Statement VisitExpressionStatement()
        {
            Expression expression = VisitExpression();
            return new ExpressionStatement(expression, NodeType.Expression);
        }

        public BlockStatement VisitBlock()
        {
            if (TokenType == TokenType.LeftBrace)
            {
                MoveNext();
                Statement[] statements = VisitListStatement().ToArray();
                if (TokenType == TokenType.RightBrace)
                    MoveNext();
                return new BlockStatement(statements);
            }
            if (CheckSyntaxExpected(TokenType.AnnonymousMethod))
            {
                MoveNext();
                return VisitBlock();
            }
            BlockStatement blockStatement = new BlockStatement(new Statement[] { new ExpressionStatement(VisitExpression(), NodeType.Return) });
            return blockStatement;
        }

        public IEnumerable<Statement> VisitListStatement()
        {
            while (TokenType != TokenType.RightBrace && TokenType != TokenType.End)
            {
                yield return VisitStatement(CodeScope.Local);
                if (TokenType == TokenType.SemiColon)
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
                exp = new BinaryOperationExpression(exp, right, NodeType.Comma);
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
                return new BinaryOperationExpression(exp, right, (NodeType)type);
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
                exp = new TernaryOperatorExpression(exp, second, third);
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
                exp = new BinaryOperationExpression(exp, right, NodeType.OrOr);
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
                exp = new BinaryOperationExpression(exp, right, NodeType.AndAnd);
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
                exp = new BinaryOperationExpression(exp, right, NodeType.Or);
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
                exp = new BinaryOperationExpression(exp, right, NodeType.Circumflex);
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
                exp = new BinaryOperationExpression(exp, right, NodeType.And);
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
                exp = new BinaryOperationExpression(exp, right, (NodeType)type);
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
                exp = new BinaryOperationExpression(exp, right, (NodeType)type);
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
                exp = new BinaryOperationExpression(exp, right, (NodeType)type);
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
                exp = new BinaryOperationExpression(exp, right, (NodeType)type);
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
                exp = new BinaryOperationExpression(exp, right, (NodeType)type);
            }
            return exp;
        }

        public Expression VisitUnaryExpression()
        {
            Expression exp = null;
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), NodeType.PrefixPlusPlus);
                    break;
                case TokenType.MinusMinus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), NodeType.PrefixMinusMinus);
                    break;
                case TokenType.Bang:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), NodeType.Bang);
                    break;
                case TokenType.Plus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), NodeType.Plus);
                    break;
                case TokenType.Minus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), NodeType.Minus);
                    break;
                default:
                    exp = VisitPostfixExpression();
                    break;
            }
            return exp;
        }

        public Expression VisitPostfixExpression()
        {
            Expression exp = VisitLeftHandSideExpression();
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    exp = new UnaryOperatorExpression(exp, NodeType.PostfixPlusPlus);
                    MoveNext();
                    break;

                case TokenType.MinusMinus:
                    exp = new UnaryOperatorExpression(exp, NodeType.PostfixMinusMinus);
                    MoveNext();
                    break;
            }
            return exp;
        }

        public Expression VisitLeftHandSideExpression()
        {
            Expression exp = null;
            switch (TokenType)
            {
                case TokenType.Numeric:
                    exp = new LiteralExpression(GetNumber());
                    break;
                case TokenType.String:
                    exp = new LiteralExpression(GetString());
                    break;
                case TokenType.Identifier:
                    exp = VisitIdentifier();
                    break;
                case TokenType.Variable:
                    var name = GetName();
                    exp = new ValueAccessExpression(name, GetVariable, ContainsVariable, NodeType.Variable);
                    break;
                case TokenType.Constant:
                    name = GetName();
                    exp = new ValueAccessExpression(name, GetConstant, ContainsConstant, NodeType.Constant);
                    break;
                case TokenType.LeftBrace:
                    var list = VisitListStatement().ToArray();
                    exp = new BlockExpression(list);
                    break;
                case TokenType.LeftParenthesis:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitConditionalExpression(), NodeType.Parenthesized);
                    CheckSyntaxExpected(TokenType.RightParenthesis);
                    break;
                case TokenType.Less:
                    //Might be array
                    exp = VisitArrayLiteral();
                    break;
            }
            //End of left
            if (TokenType == TokenType.SemiColon)
                return exp;
            MoveNext();
            return VisitRightExpression(exp);
        }

        public Expression VisitRightExpression(Expression exp)
        {
            for (TokenType type = TokenType; ; type = TokenType)
            {
                switch (type)
                {
                    case TokenType.LeftParenthesis:
                        var args = VisitArgumentList().ToArray();
                        exp = new InvocationExpression(exp, args, NodeType.Invocation);
                        break;
                    case TokenType.Initializer:
                        MoveNext();
                        var identifierName = GetName();
                        exp = new InitializerExpression(identifierName, exp);
                        break;
                    case TokenType.NullPropagator:
                        MoveNext();
                        exp = new NullPropegatorExpression(exp, VisitPostfixExpression());
                        break;
                    case TokenType.Qualified:
                    case TokenType.Dot:
                        MoveNext();
                        if (TokenType == TokenType.Identifier || TokenType == TokenType.Variable)
                        {
                            exp = new QualifiedExpression(exp, new IdentifierExpression(GetName(), (NodeType)TokenType), (NodeType)type);
                        }
                        break;
                    case TokenType.SemiColon:
                    case TokenType.End:
                    default:
                        return exp;
                }
                MoveNext();
            }
        }

        public Expression VisitIdentifier()
        {
            var name = GetName();
            if (!Keywords.TryGetIdentifier(name, out IdentifierType type))
                return new IdentifierExpression(name, NodeType.Identifier);
            switch (type)
            {
                case IdentifierType.New:
                    MoveNext();
                    Expression target = VisitExpression();
                    Expression[] arguments;
                    if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    {
                        arguments = VisitArgumentList().ToArray();
                    }
                    else
                    {
                        arguments = Enumerable.Empty<Expression>().ToArray();
                    }
                    return new InvocationExpression(target, arguments, NodeType.New);
                case IdentifierType.True:
                    return new LiteralExpression(Object.True);
                case IdentifierType.False:
                    return new LiteralExpression(Object.False);
                case IdentifierType.Out:
                    MoveNext();
                    var identfier = GetName();
                    return new ValueAccessExpression(identfier, GetVariable, ContainsVariable, NodeType.Out);
                case IdentifierType.Lamda:
                    MoveNext();
                    return VisitLamdaExpression();
                case IdentifierType.This:
                    return new SyntaxExpression(NodeType.This);
            }
            return Expression.Empty;
        }


        /// <summary>
        /// format <int>[1,2]
        /// </summary>
        /// <returns></returns>
        private Expression VisitArrayLiteral()
        {
            //Next <
            MoveNext();
            var value = new string(ReadString().ToArray());
            //>
            MoveNext();
            if (CheckSyntaxExpected(TokenType.LeftBracket))
            {
                var list = VisitExpressionList(TokenType.Comma);
                return new ArrayLiteralExpression(list.ToArray(), value);
            }
            throw new System.InvalidOperationException(string.Format("Invalid array declaration at column = {0}, line = {1}", column, line));
        }

        protected bool CheckExpectedIdentifier(IdentifierType expected)
        {
            int start = pos;
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                if (expected == type)
                    return true;
            }
            //Restore
            pos = start - 1;
            MoveNext();
            return false;
        }

        protected Expression VisitLamdaExpression()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                var args = VisitArgumentList().ToArray();
                if (TokenType == TokenType.RightParenthesis)
                {
                    MoveNext();
                    var expression = VisitBlock();
                    var method = new AnonymousFunctionExpression(args, expression);
                    return method;
                }
            }
            return Expression.Empty;
        }

        /// <summary>
        /// Dont Forget to Call ToArray
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Expression> VisitArgumentList()
        {
            TokenType type = TokenType;
            if (type == TokenType.LeftParenthesis)
                MoveNext();
            for (type = TokenType; type != TokenType.RightParenthesis; type = TokenType)
            {
                if (type == TokenType.Comma)
                {
                    MoveNext();
                    if (TokenType != TokenType.RightParenthesis)
                        yield return Expression.Empty;
                    continue;
                }

                Expression exp = VisitConditionalExpression();
                if (CheckSyntaxExpected(TokenType.Comma))
                    MoveNext();
                yield return exp;
            }
        }

        /// <summary>
        /// Dont Forget to Call ToArray x:int
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ParameterInfo> VisitFunctionArguments()
        {
            if (TokenType == TokenType.LeftParenthesis)
                MoveNext();
            while (TokenType != TokenType.RightParenthesis)
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = GetName();
                    var type = typeof(object);
                    MoveNext();
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        type = System.Type.GetType(GetName());
                    }
                    var parameter = new ParameterInfo(name, type);
                    if (TokenType == TokenType.Equal)
                    {
                        MoveNext();
                        parameter.DefaultValue = VisitConditionalExpression();
                    }
                    Scope.DeclareVariable(name, TypeInfo.From(type));
                    if (CheckSyntaxExpected(TokenType.Comma))
                        MoveNext();

                    yield return parameter;
                }
            }
        }

        public IEnumerable<Expression> VisitExpressionList(TokenType splitToken)
        {
            CheckSyntaxExpected(TokenType.LeftBrace);
            MoveNext();
            for (TokenType type = TokenType; type != TokenType.RightBrace; type = TokenType)
            {
                if (type == splitToken)
                {
                    yield return Expression.Empty;
                    MoveNext();
                    continue;
                }
                Expression exp = VisitConditionalExpression();
                if (CheckSyntaxExpected(splitToken))
                    MoveNext();
                yield return exp;
            }
        }


        public IEnumerable<VariableDeclarationExpression> VisitVarDeclations(TypeInfo type)
        {
            do
            {
                MoveNext();
                string name = string.Empty;
                if (CheckSyntaxExpected(TokenType.Identifier))
                    name = GetName();
                MoveNext();
                if (TokenType == TokenType.Equal)
                {
                    MoveNext();
                    yield return new InitializerExpression(name, VisitAssignmentExpression());
                }
                else
                {
                    yield return new VariableDeclarationExpression(name);
                }
                Scope.DeclareVariable(name, type);
            } while (TokenType == TokenType.Comma);
        }

        internal bool CheckSyntaxExpected(TokenType type)
        {
            return TokenType == type ? true : false;
        }
        #endregion

        #region Interger
        private IEnumerable<char> ReadNumeric()
        {
            char first = Text[pos - 1];
            char next = PeekChar();
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

            while (CanAdvance)
            {
                char next = PeekChar();
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
                            ReadChar();
                            continue;
                        }
                }
                break;
            }
        }

        private IEnumerable<char> CreateHexIntegerLiteral(char first)
        {
            yield return first;
            yield return ReadChar(); //x or X (ever tested before)
            double val = 0;
            while (CanAdvance)
            {
                char next = PeekChar();
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
                            ReadChar();
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
                            ReadChar();
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
                            ReadChar();
                            continue;
                        }
                }
                break;
            }
        }

        private IEnumerable<char> ReadInterger(char first)
        {
            yield return first;
            int dot = 0;
            int exp = 0;
            while (CanAdvance)
            {
                char next = PeekChar();
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
                            ReadChar();
                            continue;
                        }
                    case '.':
                        {
                            ReadChar();
                            dot++;
                            yield return next;
                            if (dot > 1)
                            {
                                yield break;
                            }
                            continue;
                        }
                    case 'e':
                    case 'E':
                        {
                            yield return next;
                            ReadChar();
                            exp++;
                            if (exp > 1)
                            {
                                yield break;
                            }
                            next = PeekChar();
                            if (next == '+' || next == '-')
                            {
                                yield return next;
                                ReadChar();
                            }
                            continue;
                        }
                }
                break;
            }
        }
        #endregion

        #region String

        public string GetString()
        {
            return new string(ReadString().ToArray());
        }

        public string GetName() => new string(ReadVariableName().ToArray());

        public double GetNumber()
        {
            var value = new string(ReadNumeric().ToArray());
            if (double.TryParse(value, Settings.NumberStyle, Settings.FormatProvider, out double result))
            {
                return result;
            }
            return double.NaN;
        }

        private IEnumerable<char> ReadVariableName()
        {
            for (; c != char.MinValue; c = ReadChar())
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    yield return c;
                    continue;
                }
                pos--;
                yield break;
            }
        }

        private IEnumerable<char> ReadString()
        {
            for (; c != '`'; c = ReadChar())
            {
                yield return c;
            }
        }

        public string GetText() => Text;

        #endregion

        public IEnumerator<TokenType> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }


        public static Object GetVariable(IOperationContext context, string name)
        {
            return context.Variables[name];
        }

        public static bool ContainsVariable(IOperationContext context, string name)
        {
            return context.Variables.ContainsKey(name);
        }

        public static Object GetConstant(IOperationContext context, string name)
        {
            return context.GetConstant(name);
        }

        public static bool ContainsConstant(IOperationContext context, string name)
        {
            return context.ContainsConstant(name);
        }
    }
}
