using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.Scopes;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public class SyntaxVisitor : IEnumerable<TokenType>, IEnumerator<TokenType>
    {
        internal static readonly Keywords InbuiltKeywords = new Keywords();
        public readonly IScriptSource Source;
        private readonly IList<string> _currentLabels = new List<string>();
        public TokenType TokenType;
        private char c;
        /// <summary>
        /// Current Scope
        /// </summary>
        public Scope Scope;
        internal readonly Keywords Keywords;
        public readonly ParserSettings Settings;

        public SyntaxVisitor(IScriptSource source, Scope initialScope, ParserSettings settings)
        {
            Source = source;
            Scope = initialScope;
            Settings = settings;
            Keywords = InbuiltKeywords;
        }

        /// <summary>
        /// labels like start: and goto start
        /// </summary>
        public string[] CurrentLabels
        {
            get => _currentLabels.ToArray();
        }

        #region Iterator
        public TokenType Current => TokenType;

        object IEnumerator.Current => TokenType;

        public void Dispose()
        {
            Source.Dispose();
            System.GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            c = Source.ReadChar();
            TokenType = GetTokenType();
            return true;
        }

        public void Reset()
        {
            Source.Reset();
        }

        public TokenType GetTokenType()
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
                    c = Source.ReadChar();
                    return TokenType.Variable;
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
                    if (n == '>')
                    {
                        c = Source.ReadChar();
                        return TokenType.Initializer;
                    }
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
                        c = Source.ReadChar();
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
                    if (Source.CanAdvance)
                    {
                        c = Source.ReadChar();
                        return GetTokenType();
                    }
                    return TokenType.End;
                case '\n':
                    Source.NextLine();
                    c = Source.ReadChar();
                    return GetTokenType();
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

        public void Parse()
        {
            if (TokenType == TokenType.Identifier)
            {
                var name = GetName();
                if (Keywords.TryGetIdentifier(name, out IdentifierType type))
                {
                    switch (type)
                    {
                        case IdentifierType.Class:
                            VisitTypeDeclaration();
                            break;
                    }
                }
            }
        }

        internal TypeDefinitionStatement VisitTypeDeclaration()
        {
            MoveNext();
            if (TokenType == TokenType.Identifier)
            {
                string typeName = GetName();
                string baseTypeName = null;
                string[] implements = null;
                MoveNext();
                if (TokenType == TokenType.Identifier)
                {
                    string name = GetName();
                    if (name.Equals("extends"))
                    {
                        MoveNext();
                        baseTypeName = GetName();
                        MoveNext();
                    }
                }
                if (TokenType == TokenType.Identifier)
                {
                    var name = GetName();
                    if (name.Equals("implements"))
                    {
                        implements = Split(TokenType.Comma).ToArray();
                    }
                }
                TypeDeclaration declaration = null;
                Node[] nodes = null;
                using (var scope = new ObjectScope(this, typeName))
                {
                    declaration = new TypeDeclaration(typeName, baseTypeName, implements, scope);
                    if (TokenType == TokenType.LeftBrace)
                    {
                        MoveNext();
                        nodes = VisitNodes().ToArray();
                    }
                }

                var type = Scope.DeclareMember(declaration, BindingFlags.Public, MemberTypes.Type);
                return new TypeDefinitionStatement(declaration, nodes, type);
            }
            throw new System.Exception("Syntax error");
        }

        public IEnumerable<Node> VisitNodes()
        {
            while (TokenType != TokenType.RightBrace)
            {
                yield return VisitNode();
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
            }
        }

        public Node VisitNode()
        {
            switch (TokenType)
            {
                case TokenType.Identifier:
                    return VisitIdentifierNode();
                default:
                    throw new Exception(string.Format("Invalid Token type {0}", TokenType));
            }
        }

        private Node VisitIdentifierNode()
        {
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Function:
                        return VisitFunctionDefinition();
                    //default label statment
                    default:
                        throw new Exception("Unexpected Keyword");
                }
            }
            else
            {
                return VisitLabeledNode(name);
            }
        }

        public Statement VisitStatement()
        {
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    return VisitBlock();
                case TokenType.Identifier:
                    return VisitIdentifierStatement();
                case TokenType.SemiColon:
                    //Rare case
                    throw new System.Exception($"unexpected semicolon at line {Source.Line} , pos {Source.Position}");
                default:
                    return VisitExpressionStatement();
            }
        }

        private Statement VisitIdentifierStatement()
        {
            long start = Source.Position;
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.Return:
                        MoveNext();
                        Expression expression = null;
                        if (!CheckSyntaxExpected(TokenType.SemiColon))
                        {
                            expression = VisitExpression();
                        }
                        return new ReturnOrThrowStatement(expression, StatementType.Return);
                    case IdentifierType.Var:
                        //any type
                        var declarations = VisitDeclarations().ToArray();
                        return new VariableDeclarationStatement(declarations);
                    case IdentifierType.Function:
                        return VisitFunctionDefinition();
                    case IdentifierType.If:
                        return VisitIfStatement();
                    case IdentifierType.Else:
                        MoveNext();
                        return VisitStatement();
                        //default label statment
                }
            }
            //restore to prev
            Source.SeekTo(start - 1);
            MoveNext();
            return VisitExpressionStatement();
        }

        private Node VisitLabeledNode(string name)
        {
            MoveNext();
            string typeName = null;
            if (TokenType == TokenType.Colon)
            {
                MoveNext();
                typeName = GetName();
                MoveNext();
            }
            DeclaredFlags flags = DeclaredFlags.None;
            if (TokenType == TokenType.LeftBracket)
            {
                MoveNext();
                if (TokenType == TokenType.RightBracket)
                {
                    flags |= DeclaredFlags.Array;
                    MoveNext();
                }
            }
            Expression expression = null;
            if (TokenType == TokenType.Equal)
            {
                MoveNext();
                expression = VisitConditionalExpression();
            }
            var member = Scope.DeclareMember(new FieldDelcaration(name, new Emit.TypeName(typeName, flags)), BindingFlags.Private, MemberTypes.Field, new ExpressionStatement(expression));
            var initial = new FieldDeclarationExpression(name, member);
            if (TokenType == TokenType.Comma)
            {
                var declarations = VisitFieldDeclarations(initial).ToArray();
                return new FieldDeclarationStatement(declarations);
            }
            return new FieldDeclarationStatement(initial);

        }

        private IEnumerable<FieldDeclarationExpression> VisitFieldDeclarations(FieldDeclarationExpression initial)
        {
            yield return initial;
            MoveNext();
            while (TokenType != TokenType.SemiColon && TokenType != TokenType.End)
            {
                if (TokenType == TokenType.Identifier)
                {
                    string name = GetName();
                    MoveNext();
                    string typeName = null;
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        typeName = GetName();
                        MoveNext();
                    }
                    DeclaredFlags flags = DeclaredFlags.None;
                    if (TokenType == TokenType.LeftBracket)
                    {
                        MoveNext();
                        if (TokenType == TokenType.RightBracket)
                        {
                            flags |= DeclaredFlags.Array;
                            MoveNext();
                        }
                    }
                    Expression expression = null;
                    if (TokenType == TokenType.Equal)
                    {
                        MoveNext();
                        expression = VisitConditionalExpression();
                    }
                    var member = Scope.DeclareMember(new FieldDelcaration(name, new Emit.TypeName(typeName, flags)), BindingFlags.Private, MemberTypes.Field, new ExpressionStatement(expression));
                    yield return new FieldDeclarationExpression(name, member);
                }
            }
        }

        protected Statement VisitIfStatement()
        {
            MoveNext();
            if (TokenType == TokenType.LeftParenthesis)
            {
                var expression = VisitExpression();
                if (TokenType == TokenType.RightParenthesis)
                    MoveNext();
                Statement body = VisitStatement();
                long start = Source.Position;
                if (TokenType == TokenType.SemiColon)
                    MoveNext();
                Statement other = null;
                if (CheckExpectedIdentifier(IdentifierType.Else))
                    other = VisitStatement();
                else
                {
                    Source.SeekTo(start - 1);
                    MoveNext();
                }
                return new IfStatement(expression, body, other);
            }
            throw new System.Exception($"Syntax Error at line {Source.Line}, pos {Source.Position}");
        }

        public FunctionDeclarationStatement VisitFunctionDefinition()
        {
            MoveNext();
            string name = null;
            if (TokenType == TokenType.Identifier)
                name = GetName();
            MoveNext();
            ArgumentInfo[] argumentsList;
            BlockStatement body = null;
            //return type
            string type = null;
            FunctionDeclaration declaration = null;
            using (var scope = new DeclarativeScope(this, name))
            {
                //todo not required when the method is static
                scope.DeclareVariable("this", Emit.TypeName.Empty, null, VariableType.Argument);
                IEnumerable<ArgumentInfo> arguments = Enumerable.Empty<ArgumentInfo>();
                if (CheckSyntaxExpected(TokenType.LeftParenthesis))
                    arguments = VisitFunctionArguments();
                argumentsList = arguments.ToArray();
                if (CheckSyntaxExpected(TokenType.RightParenthesis))
                    MoveNext();
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    type = GetName();
                    MoveNext();
                }
                DeclaredFlags flags = DeclaredFlags.None;
                if (TokenType == TokenType.LeftBracket)
                {
                    MoveNext();
                    if (TokenType == TokenType.RightBracket)
                    {
                        flags |= DeclaredFlags.Array;
                        MoveNext();
                    }
                }
                declaration = new FunctionDeclaration(name, new Emit.TypeName(type, flags), argumentsList, scope);
                if (TokenType == TokenType.LeftBrace)
                {
                    //todo abstract, virtual functions
                    //To avoid block function
                    body = VisitBlock();
                }

            }
            var memeber = Scope.DeclareMember(declaration, BindingFlags.Public, MemberTypes.Function, body);
            if (body != null)
            {
                return new FunctionDefinitionStatement(declaration, body, memeber);
            }
            return new FunctionDeclarationStatement(declaration);

        }

        private Statement VisitExpressionStatement()
        {
            Expression expression = VisitExpression();
            return new ExpressionStatement(expression);
        }

        public BlockStatement VisitBlock()
        {
            //clear labels
            _currentLabels.Clear();
            if (TokenType == TokenType.LeftBrace)
            {
                MoveNext();
                Statement[] statements = VisitListStatement().ToArray();
                if (TokenType == TokenType.RightBrace)
                    MoveNext();
                return new BlockStatement(statements, CurrentLabels);
            }
            if (CheckSyntaxExpected(TokenType.AnonymousMethod))
            {
                MoveNext();
                return VisitBlock();
            }
            BlockStatement blockStatement = new BlockStatement(new Statement[] { new ReturnOrThrowStatement(VisitExpression(), StatementType.Return) }, CurrentLabels);
            return blockStatement;
        }

        public IEnumerable<Statement> VisitListStatement()
        {
            while (TokenType != TokenType.RightBrace && TokenType != TokenType.End)
            {
                yield return VisitStatement();
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.Comma);
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
                return new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.OrOr);
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.AndAnd);
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.Or);
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.Circumflex);
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
                exp = new BinaryOperationExpression(exp, right, ExpressionType.And);
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
                exp = new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                exp = new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                exp = new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                exp = new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                exp = new BinaryOperationExpression(exp, right, (ExpressionType)type);
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
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), ExpressionType.PrefixPlusPlus);
                    break;
                case TokenType.MinusMinus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), ExpressionType.PrefixMinusMinus);
                    break;
                case TokenType.Bang:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), ExpressionType.Bang);
                    break;
                case TokenType.Plus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), ExpressionType.Plus);
                    break;
                case TokenType.Minus:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitLeftHandSideExpression(), ExpressionType.Minus);
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
                    exp = new UnaryOperatorExpression(exp, ExpressionType.PostfixPlusPlus);
                    MoveNext();
                    break;

                case TokenType.MinusMinus:
                    exp = new UnaryOperatorExpression(exp, ExpressionType.PostfixMinusMinus);
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
                    exp = new LiteralExpression(GetNumeric());
                    break;
                case TokenType.String:
                    exp = new LiteralExpression(GetString());
                    break;
                case TokenType.Identifier:
                    exp = VisitIdentifier();
                    break;
                case TokenType.Variable:
                    var name = GetName();
                    exp = new ValueAccessExpression(name, ExpressionType.Variable);
                    break;
                case TokenType.Constant:
                    name = GetName();
                    exp = new ValueAccessExpression(name, ExpressionType.Constant);
                    break;
                case TokenType.LeftBrace:
                    var list = VisitListStatement().ToArray();
                    exp = new BlockExpression(list);
                    break;
                case TokenType.LeftParenthesis:
                    MoveNext();
                    exp = new UnaryOperatorExpression(VisitConditionalExpression(), ExpressionType.Parenthesized);
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
                        exp = new InvocationExpression(exp, args, ExpressionType.Invocation);
                        break;
                    case TokenType.Initializer:
                        //0->x
                        MoveNext();
                        var identifierName = GetName();
                        var variable = Scope.DeclareVariable(identifierName, exp.TypeName, exp);
                        exp = new VariableDeclarationExpression(identifierName, Scope, variable);
                        break;
                    case TokenType.NullPropagator:
                        MoveNext();
                        exp = new NullPropegatorExpression(exp, VisitPostfixExpression());
                        break;
                    case TokenType.Qualified:
                    case TokenType.Dot:
                        MoveNext();
                        exp = new QualifiedExpression(exp, GetName(), (ExpressionType)type);
                        break;
                    case TokenType.LeftBracket:
                        MoveNext();
                        args = VisitExpressionList(TokenType.Comma, TokenType.RightBracket).ToArray();
                        exp = new InvocationExpression(exp, args, ExpressionType.Indexer);
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
                return new NameExpression(name, Scope, ExpressionType.Identifier);
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
                        arguments = new Expression[0];
                    }
                    return new InvocationExpression(target, arguments, ExpressionType.New);
                case IdentifierType.True:
                    return new LiteralExpression(true);
                case IdentifierType.False:
                    return new LiteralExpression(false);
                case IdentifierType.Null:
                    return LiteralExpression.Null;
                case IdentifierType.Lamda:
                    MoveNext();
                    return VisitLamdaExpression();
                case IdentifierType.This:
                    return new Expression(ExpressionType.This);
            }
            return Expression.Empty;
        }


        /// <summary>
        /// format <int>[1,2]
        /// </summary>
        /// <returns></returns>
        private Expression VisitArrayLiteral()
        {
            string typeName = null;
            DeclaredFlags flags = DeclaredFlags.None;
            if (TokenType == TokenType.Less)
            {
                //Next <
                MoveNext();
                typeName = GetName();
                //>
                MoveNext();
                if (TokenType == TokenType.LeftBracket)
                {
                    MoveNext();
                    if (TokenType == TokenType.RightBracket)
                    {
                        flags |= DeclaredFlags.Array;
                        MoveNext();
                    }
                }
                if (TokenType == TokenType.Greater)
                    MoveNext();
            }
            if (CheckSyntaxExpected(TokenType.LeftBracket))
            {
                //[
                MoveNext();
                var list = VisitExpressionList(TokenType.Comma, TokenType.RightBracket).ToArray();
                return new ArrayLiteralExpression(list, new Emit.TypeName(typeName, flags));
            }
            throw new System.InvalidOperationException(string.Format("Invalid array declaration at column = {0}, line = {1}", Source.Column, Source.Line));
        }

        protected bool CheckExpectedIdentifier(IdentifierType expected)
        {
            long start = Source.Position;
            var name = GetName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                if (expected == type)
                    return true;
            }
            //Restore
            Source.SeekTo(start - 1);
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
        public IEnumerable<ArgumentInfo> VisitFunctionArguments()
        {
            if (TokenType == TokenType.LeftParenthesis)
                MoveNext();
            while (TokenType != TokenType.RightParenthesis)
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = GetName();
                    string typeName = null;
                    DeclaredFlags flags = DeclaredFlags.None;
                    MoveNext();
                    if (TokenType == TokenType.Colon)
                    {
                        MoveNext();
                        typeName = GetName();
                        //after type name next
                        MoveNext();
                    }
                    if (TokenType == TokenType.LeftBracket)
                    {
                        MoveNext();
                        if (TokenType == TokenType.RightBracket)
                        {
                            flags |= DeclaredFlags.Array;
                            MoveNext();
                        }
                    }
                    var parameter = new ArgumentInfo(name, new Emit.TypeName(typeName, flags));
                    if (TokenType == TokenType.Equal)
                    {
                        MoveNext();
                        parameter.DefaultValue = VisitConditionalExpression();
                    }

                    Scope.DeclareVariable(parameter.Name, parameter.TypeName, parameter.DefaultValue, VariableType.Argument);

                    //todo check if method arguments needs any scopes
                    if (TokenType == TokenType.Comma)
                        MoveNext();

                    yield return parameter;
                }
            }
        }

        public IEnumerable<Expression> VisitExpressionList(TokenType splitToken, TokenType endToken)
        {
            for (TokenType type = TokenType; type != endToken; type = TokenType)
            {
                if (type == splitToken)
                {
                    yield return Expression.Empty;
                    MoveNext();
                    continue;
                }
                Expression exp = VisitConditionalExpression();
                yield return exp;
                if (CheckSyntaxExpected(splitToken))
                    MoveNext();
            }
        }


        public IEnumerable<VariableDeclarationExpression> VisitDeclarations()
        {
            do
            {
                //todo remove initializer and declations
                MoveNext();
                string name = string.Empty;
                if (CheckSyntaxExpected(TokenType.Identifier))
                    name = GetName();
                MoveNext();
                string typeName = null;
                DeclaredFlags flags = DeclaredFlags.None;
                Expression expression = null;
                if (TokenType == TokenType.Colon)
                {
                    MoveNext();
                    typeName = GetName();
                    MoveNext();
                }
                if (TokenType == TokenType.LeftBracket)
                {
                    MoveNext();
                    if (TokenType == TokenType.RightBracket)
                    {
                        flags |= DeclaredFlags.Array;
                        MoveNext();
                    }
                }
                if (TokenType == TokenType.Equal)
                {
                    MoveNext();
                    expression = VisitAssignmentExpression();
                }
                yield return new VariableDeclarationExpression(name, Scope, Scope.DeclareVariable(name, new Emit.TypeName(typeName, flags), expression));
            } while (TokenType == TokenType.Comma);
        }

        private IEnumerable<string> Split(TokenType split)
        {
            var type = TokenType;
            for (; type == TokenType.Identifier;)
            {
                yield return GetName();
                if (TokenType == split)
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
                    case '.':
                        {
                            Source.ReadChar();
                            dot++;
                            builder.Append(next);
                            if (dot > 1)
                            {
                                break;
                            }
                            continue;
                        }
                    case 'e':
                    case 'E':
                        {
                            builder.Append(next);
                            Source.ReadChar();
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

        public string GetString()
        {
            return new string(ReadString().ToArray());
        }
        public string GetName()
        {
            return new string(ReadVariableName().ToArray());
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

        public string GetText() => Source.ToString();

        #endregion

        public IEnumerator<TokenType> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

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
