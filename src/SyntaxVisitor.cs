using FluidScript.SyntaxTree.Expressions;
using FluidScript.SyntaxTree.Statements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript
{
    public class SyntaxVisitor : IEnumerable<TokenType>, IEnumerator<TokenType>
    {
        public readonly string Text;
        public readonly int Length;
        public TokenType TokenType;
        private char c;
        private int pos;
        private int line = 1;
        public readonly IOperationContext Context;
        public readonly IReadOnlyOperationContext ReadOnlyContext;

        public SyntaxVisitor(string text, IOperationContext context)
        {
            Text = text;
            Length = text.Length;
            Context = context;
            ReadOnlyContext = context.ReadOnlyContext;
        }

        #region Iterator
        public TokenType Current => TokenType;

        object IEnumerator.Current => TokenType;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
                yield return VisitStatement(Scope.Program);
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
            }
        }

        public char ReadChar()
        {
            if (pos >= Length)
                return char.MinValue;
            return Text[pos++];
        }

        public char PeekChar()
        {
            if (pos >= Length)
                return char.MinValue;
            return Text[pos];
        }

        public bool CanAdvance => pos < Length;

        public Statement VisitStatement(Scope scope)
        {
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    return VisitBlock();
                case TokenType.Identifier:
                    return VisitIdentifierStatement(scope);
                case TokenType.SemiColon:
                    //Rare case
                    throw new Exception($"unexpected semicolon at line {line} , pos {pos}");
                default:
                    return VisitExpressionStatement();
            }
        }

        private Statement VisitIdentifierStatement(Scope scope)
        {
            int start = pos;
            var name = GetVariableName();
            if (ReadOnlyContext.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Out:
                        MoveNext();
                        IExpression expression = null;
                        if (!CheckSyntaxExpected(TokenType.SemiColon))
                        {
                            expression = VisitExpression();
                        }
                        return new ReturnOrThrowStatement(Statement.Operation.Return, expression);
                    case IdentifierType.Var:
                        var declarations = VisitVarDeclations().ToArray();
                        return new VariableDeclarationStatement(declarations);
                    case IdentifierType.Function:
                        return VisitFunctionDefinition(scope);
                    case IdentifierType.If:
                        return VisitIfStatement();
                    case IdentifierType.Else:
                        MoveNext();
                        return VisitStatement(Scope.Local);
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
                Statement body = VisitStatement(Scope.Local);
                int start = pos;
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                Statement other = null;
                if (CheckExpectedIdentifier(IdentifierType.Else))
                    other = VisitStatement(Scope.Local);
                else
                {
                    pos = start - 1;
                    MoveNext();
                }
                return new IfStatement(expression, body, other);
            }
            throw new Exception($"Syntax Error at line {line}, pos {pos}");
        }

        private FunctionDefinitionStatement VisitFunctionDefinition(Scope scope)
        {
            MoveNext();
            string name = null;
            if (CheckSyntaxExpected(TokenType.Identifier))
                name = GetVariableName();
            MoveNext();
            var arguments = Enumerable.Empty<IExpression>();
            if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                arguments = VisitArgumentList();
            var argumentsList = arguments.ToArray();
            if (CheckSyntaxExpected(TokenType.RightParenthesis))
                MoveNext();
            FunctionDeclarationStatement declaration = new FunctionDeclarationStatement(name, argumentsList);
            //To avoid block function
            BlockStatement body = VisitBlock();
            return new FunctionDefinitionStatement(declaration, body, scope);
        }

        private Statement VisitExpressionStatement()
        {
            IExpression expression = VisitExpression();
            return new ExpressionStatement(expression, Statement.Operation.Expression);
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
            BlockStatement blockStatement = new BlockStatement(new Statement[] { new ExpressionStatement(VisitExpression(), Statement.Operation.Return) });
            return blockStatement;
        }

        public IEnumerable<Statement> VisitListStatement()
        {
            while (TokenType != TokenType.RightBrace && TokenType != TokenType.End)
            {
                yield return VisitStatement(Scope.Local);
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
            }
        }

        #region Visitor
        public IExpression VisitExpression()
        {
            IExpression exp = VisitAssignmentExpression();
            while (TokenType == TokenType.Comma)
            {
                MoveNext();
                IExpression right = VisitAssignmentExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.Comma);
            }
            return exp;
        }

        public IExpression VisitAssignmentExpression()
        {
            IExpression exp = VisitConditionalExpression();
            return VisitAssignmentExpression(exp);
        }

        private IExpression VisitAssignmentExpression(IExpression exp)
        {
            TokenType type = TokenType;
            if (type == TokenType.Equal)
            {
                MoveNext();
                IExpression right = VisitAssignmentExpression();
                return new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitConditionalExpression()
        {
            IExpression exp = VisitLogicalORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Question;
                type = TokenType)
            {
                MoveNext();
                IExpression second = VisitConditionalExpression();
                MoveNext();
                IExpression third = VisitConditionalExpression();
                exp = new TernaryOperatorExpression(exp, second, third);
            }
            return exp;
        }

        public IExpression VisitLogicalORExpression()
        {
            IExpression exp = VisitLogicalAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.OrOr;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitLogicalAndExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.OrOr);
            }
            return exp;
        }

        public IExpression VisitLogicalAndExpression()
        {
            IExpression exp = VisitBitwiseORExpression();
            for (TokenType type = TokenType;
                type == TokenType.AndAnd;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitBitwiseORExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.AndAnd);
            }
            return exp;
        }

        public IExpression VisitBitwiseORExpression()
        {
            IExpression exp = VisitBitwiseXORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Or;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitBitwiseXORExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.Or);
            }
            return exp;
        }

        public IExpression VisitBitwiseXORExpression()
        {
            IExpression exp = VisitBitwiseAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.Circumflex;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitBitwiseAndExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.Circumflex);
            }
            return exp;
        }

        public IExpression VisitBitwiseAndExpression()
        {
            IExpression exp = VisitEqualityExpression();
            for (TokenType type = TokenType;
                type == TokenType.And;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitEqualityExpression();
                exp = new BinaryOperationExpression(exp, right, Expression.Operation.And);
            }
            return exp;
        }

        public IExpression VisitEqualityExpression()
        {
            IExpression exp = VisitRelationalExpression();
            for (TokenType type = TokenType;
                type == TokenType.EqualEqual || type == TokenType.BangEqual;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitRelationalExpression();
                exp = new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitRelationalExpression()
        {
            IExpression exp = VisitShiftExpression();
            for (TokenType type = TokenType;
                type == TokenType.Greater || type == TokenType.GreaterEqual
                || type == TokenType.Less || type == TokenType.LessEqual;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitShiftExpression();
                exp = new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitShiftExpression()
        {
            IExpression exp = VisitAdditionExpression();
            for (TokenType type = TokenType;
                type == TokenType.LessLess || type == TokenType.GreaterGreater;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitAdditionExpression();
                exp = new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitAdditionExpression()
        {
            IExpression exp = VisitMultiplicationExpression();
            for (TokenType type = TokenType;
                type == TokenType.Plus || type == TokenType.Minus;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitMultiplicationExpression();
                exp = new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitMultiplicationExpression()
        {
            IExpression exp = VisitUnaryExpression();
            for (TokenType type = TokenType;
                type == TokenType.Multiply || type == TokenType.Divide || type == TokenType.Percent;
                type = TokenType)
            {
                MoveNext();
                IExpression right = VisitUnaryExpression();
                exp = new BinaryOperationExpression(exp, right, (Expression.Operation)type);
            }
            return exp;
        }

        public IExpression VisitUnaryExpression()
        {
            IExpression exp = null;
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), Expression.Operation.PrefixPlusPlus);
                    break;
                case TokenType.MinusMinus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), Expression.Operation.PrefixMinusMinus);
                    break;
                case TokenType.Bang:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), Expression.Operation.Bang);
                    break;
                case TokenType.Plus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), Expression.Operation.Plus);
                    break;
                case TokenType.Minus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), Expression.Operation.Minus);
                    break;
                default:
                    exp = VisitPostfixExpression();
                    break;
            }
            return exp;
        }

        public IExpression VisitPostfixExpression()
        {
            IExpression exp = VisitLeftHandSideExpression();
            switch (TokenType)
            {
                case TokenType.PlusPlus:
                    exp = new UnaryOperatorExpression(exp, Expression.Operation.PostfixPlusPlus);
                    MoveNext();
                    break;

                case TokenType.MinusMinus:
                    exp = new UnaryOperatorExpression(exp, Expression.Operation.PostfixMinusMinus);
                    MoveNext();
                    break;
            }
            return exp;
        }

        public IExpression VisitLeftHandSideExpression()
        {
            IExpression exp = null;
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
                    var name = GetVariableName();
                    exp = new ValueAccessExpression(name, GetVariable, ContainsVariable, Expression.Operation.Variable);
                    break;
                case TokenType.Constant:
                    name = GetVariableName();
                    exp = new ValueAccessExpression(name, GetConstant, ContainsConstant, Expression.Operation.Constant);
                    break;
                case TokenType.LeftBrace:
                    var list = VisitListStatement().ToArray();
                    exp = new BlockExpression(list);
                    break;
                case TokenType.LeftParenthesis:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitConditionalExpression(), Expression.Operation.Parenthesized);
                    CheckSyntaxExpected(TokenType.RightParenthesis);
                    break;
            }
            //End of left
            if (TokenType == TokenType.SemiColon)
                return exp;
            MoveNext();
            return VisitRightExpression(exp);
        }

        public IExpression VisitRightExpression(IExpression exp)
        {
            for (TokenType type = TokenType; ; type = TokenType)
            {
                switch (type)
                {
                    case TokenType.LeftParenthesis:
                        var args = VisitArgumentList().ToArray();
                        exp = new InvocationExpression(exp, args, Expression.Operation.Invocation);
                        break;
                    case TokenType.Initializer:
                        MoveNext();
                        var identifierName = GetVariableName();
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
                            exp = new QualifiedExpression(exp, new IdentifierExpression(GetVariableName(), (Expression.Operation)TokenType), (Expression.Operation)type);
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

        public IExpression VisitIdentifier()
        {
            var name = GetVariableName();
            if (!ReadOnlyContext.TryGetIdentifier(name, out IdentifierType type))
                return new IdentifierExpression(name, Expression.Operation.Identifier);
            switch (type)
            {
                case IdentifierType.New:
                    MoveNext();
                    IExpression target = VisitExpression();
                    IExpression[] arguments;
                    if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    {
                        arguments = VisitArgumentList().ToArray();
                    }
                    else
                    {
                        arguments = Enumerable.Empty<IExpression>().ToArray();
                    }
                    return new InvocationExpression(target, arguments, Expression.Operation.New);
                case IdentifierType.True:
                    return new LiteralExpression(Object.True);
                case IdentifierType.False:
                    return new LiteralExpression(Object.False);
                case IdentifierType.Out:
                    MoveNext();
                    var identfier = GetVariableName();
                    return new ValueAccessExpression(identfier, GetVariable, ContainsVariable, Expression.Operation.Out);
                case IdentifierType.Lamda:
                    MoveNext();
                    return VisitLamdaExpression();
                case IdentifierType.This:
                    return new SyntaxExpression(Expression.Operation.This);
            }
            return Expression.Empty;
        }

        protected bool CheckExpectedIdentifier(IdentifierType expected)
        {
            int start = pos;
            var name = GetVariableName();
            if (ReadOnlyContext.TryGetIdentifier(name, out IdentifierType type))
            {
                if (expected == type)
                    return true;
            }
            //Restore
            pos = start - 1;
            MoveNext();
            return false;
        }

        protected IExpression VisitLamdaExpression()
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
        public IEnumerable<IExpression> VisitArgumentList()
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

                IExpression exp = VisitConditionalExpression();
                if (CheckSyntaxExpected(TokenType.Comma))
                    MoveNext();
                yield return exp;
            }
        }

        public IEnumerable<IExpression> VisitExpressionList(TokenType splitToken)
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
                IExpression exp = VisitConditionalExpression();
                if (CheckSyntaxExpected(splitToken))
                    MoveNext();
                yield return exp;
            }
        }


        public IEnumerable<VariableDeclarationExpression> VisitVarDeclations()
        {
            do
            {
                MoveNext();
                string name = string.Empty;
                if (CheckSyntaxExpected(TokenType.Identifier))
                    name = GetVariableName();
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

        public string GetVariableName() => new string(ReadVariableName().ToArray());

        public double GetNumber()
        {
            var value = new string(ReadNumeric().ToArray());
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
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
