using FluidScript.Compiler.Lexer;
using FluidScript.Utils;
using System;
using System.Text;

namespace FluidScript.ConsoleApp
{
    sealed class ScriptIndentation
    {
        const char Space = ' ';
        readonly StringBuilder sb = new StringBuilder();
        readonly string text;
        readonly bool formatted;
        readonly int len;
        int pos;
        char c;
        int tab = 0;
        TokenType TokenType;

        public ScriptIndentation(string text, bool formatted)
        {
            this.text = text;
            this.formatted = formatted;
            len = text.Length;
            pos = 0;
        }

        public bool CanAdvance => pos < len;

        private void AppendThenNext(char value)
        {
            sb.Append(value);
            c = ReadChar();
            TokenType = GetTokenType(true);
        }

        public void AppendThenNext()
        {
            switch (TokenType)
            {
                case TokenType.AndAnd:
                    sb.Append("&&");
                    break;
                case TokenType.OrOr:
                    sb.Append("||");
                    break;
                case TokenType.BangEqual:
                    sb.Append("!=");
                    break;
                case TokenType.NullPropagator:
                    sb.Append("??");
                    break;
                case TokenType.GreaterEqual:
                    sb.Append(">=");
                    break;
                case TokenType.EqualEqual:
                    sb.Append("==");
                    break;
                case TokenType.LessEqual:
                    sb.Append("<=");
                    break;
                case TokenType.GreaterGreater:
                    sb.Append(">>");
                    break;
                case TokenType.LessLess:
                    sb.Append("<<");
                    break;
                case TokenType.StarStar:
                    sb.Append("**");
                    break;
                case TokenType.PlusPlus:
                    sb.Append("++");
                    break;
                case TokenType.MinusMinus:
                    sb.Append("--");
                    break;
                case TokenType.Qualified:
                    sb.Append("::");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
            c = ReadChar();
            TokenType = GetTokenType(true);
        }

        private char FallBack()
        {
            return text[--pos];
        }

        public bool MoveNext(bool skipLine = true)
        {
            c = ReadChar();
            TokenType = GetTokenType(skipLine);
            return TokenType != TokenType.Bad;
        }

        private bool MoveNextIf(TokenType token)
        {
            return TokenType == token ? MoveNext() : false;
        }

        private bool MoveNextThenIf(TokenType token)
        {
            return MoveNext() && TokenType == token;
        }

        private bool MoveNextThenIfNot(TokenType token)
        {
            return MoveNext() && TokenType != token;
        }

        public bool AppendNextIf(TokenType token)
        {
            if (TokenType == token)
            {
                sb.Append(c);
                return MoveNext();
            }

            throw new Exception(string.Concat("Invalid token ", c));
        }

        public char ReadChar()
        {
            if (pos == len)
                return char.MinValue;
            return text[pos++];
        }

        public char PeekChar()
        {
            if (pos < len)
                return text[pos];
            return char.MinValue;
        }

        public TokenType GetTokenType(bool skipLine)
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
                case '[':
                    return TokenType.LeftBracket;
                case ']':
                    return TokenType.RightBracket;
                case '@':
                    if (char.IsLetterOrDigit(n))
                    {
                        c = ReadChar();
                        return TokenType.SpecialVariable;
                    }
                    break;
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
                    if (n == '-')
                    {
                        c = ReadChar();
                        return TokenType.MinusMinus;
                    }
                    return TokenType.Minus;
                case '*':
                    if (n == '*')
                    {
                        c = ReadChar();
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
                        return TokenType.AnonymousMethod;
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
                    if (CanAdvance)
                    {
                        c = ReadChar();
                        return GetTokenType(skipLine);
                    }
                    return TokenType.End;
                case '\n':
                    if (formatted && skipLine)
                    {
                        c = ReadChar();
                        return GetTokenType(skipLine);
                    }
                    return TokenType.NewLine;
                case '\r':
                    if (n == '\n')
                    {
                        c = ReadChar();
                        if (formatted && skipLine)
                        {
                            return GetTokenType(skipLine);
                        }
                        return TokenType.NewLine;
                    }
                    break;
                default:
                    if (char.IsLetter(c))
                    {
                        return TokenType.Identifier;
                    }
                    break;
            }
            if (CanAdvance == false)
                return TokenType.End;
            return TokenType.Bad;
        }

        public string Format()
        {
            if (MoveNext())
            {
                tab++;
                VisitStatement();
            }
            return sb.ToString();
        }

        private void VisitStatement()
        {
            switch (TokenType)
            {
                case TokenType.LeftBrace:
                    sb.Append(c);
                    AppendLine();
                    VisitStatementList();
                    // Consider new line also
                    if (TokenType == TokenType.RightBrace)
                        AppendThenNext();
                    break;
                case TokenType.Identifier:
                    VisitIdentifierStatement();
                    break;
                //case TokenType.SemiColon:
                //    MoveNext();
                //    statement = Statement.Empty;
                //    break;
                default:
                    VisitExpressionStatement();
                    break;
            }
        }

        private void VisitIdentifierStatement()
        {
            int start = pos;
            var name = ReadVariableName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                MoveNext();
                switch (type)
                {
                    case IdentifierType.Return:
                        sb.Append(name);
                        sb.Append(Space);
                        if (TokenType != TokenType.SemiColon)
                        {
                            VisitExpression();
                        }
                        return;
                    case IdentifierType.Var:
                        sb.Append(name);
                        sb.Append(Space);
                        //any type
                        VisitVarDeclarations();
                        return;
                    case IdentifierType.Function:
                        sb.Append(name);
                        sb.Append(Space);
                        VisitLocalFunction();
                        return;
                    case IdentifierType.While:
                        VisitWhileStatement();
                        return;
                    case IdentifierType.For:
                        sb.Append(name);
                        VisitForStatement();
                        return;
                    case IdentifierType.Do:
                        VisitDoWhileStatement();
                        return;
                    case IdentifierType.If:
                        VisitIfStatement();
                        return;
                    case IdentifierType.Else:
                        VisitStatement();
                        return;
                    case IdentifierType.Break:
                        sb.Append(name);
                        return;
                    case IdentifierType.Continue:
                        sb.Append(name);
                        sb.Append(Space);
                        if (TokenType == TokenType.Identifier)
                        {
                            string target = ReadVariableName();
                            sb.Append(target);
                            MoveNext();
                        }
                        return;

                }
            }
            //default label statment
            //restore to prev
            pos = start - 1;
            //skips if new line
            MoveNext();
            VisitExpressionStatement();
        }

        private void VisitExpressionStatement()
        {
            VisitExpression();
        }

        private void VisitIfStatement()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                // (
                AppendThenNext(c);
                VisitExpression();
                //)
                if (TokenType == TokenType.RightParenthesis)
                    AppendThenNext(c);
                VisitStatement();
                if (CheckExpectedIdentifier(IdentifierType.Else))
                {
                    sb.Append(Space);
                    VisitStatement();
                }
            }
            throw new System.Exception($"Syntax Error at line {pos}");
        }

        private void VisitForStatement()
        {
            if (TokenType == TokenType.LeftParenthesis)
            {
                AppendThenNext();
                VisitStatement();
                //todo throw error if others
                if (TokenType == TokenType.SemiColon)
                    AppendThenNext(c);
                VisitExpression();
                if (TokenType == TokenType.SemiColon)
                    AppendThenNext(c);
                VisitExpressionList(TokenType.Comma, TokenType.RightParenthesis);
                if (TokenType == TokenType.RightParenthesis)
                    AppendThenNext();

                AppendLine();


                if (TokenType == TokenType.LeftBrace)
                {
                    AppendTab();
                    VisitBlock();
                }
                else
                {
                    tab++;
                    AppendTab();
                    VisitStatement();
                    tab--;
                }
            }
        }

        private void VisitBlock()
        {
            //clear labels
            if (TokenType == TokenType.LeftBrace)
            {
                tab++;
                sb.Append(c);
                AppendLine();
                VisitStatementList();
                tab--;
                // don't skip new line
                if (TokenType == TokenType.RightBrace)
                {
                    AppendTab();
                    sb.Append(c);
                    MoveNext(false);
                }
            }
            else if (TokenType == TokenType.AnonymousMethod)
            {
                AppendThenNext();
                VisitAnonymousBlock();
            }
            else
                throw new System.Exception("Invalid Function declaration");
        }

        private void VisitAnonymousBlock()
        {
            throw new NotImplementedException();
        }

        private void VisitWhileStatement()
        {
            throw new NotImplementedException();
        }

        private void VisitLocalFunction()
        {
            throw new NotImplementedException();
        }

        private void VisitDoWhileStatement()
        {
            throw new NotImplementedException();
        }

        private void VisitVarDeclarations()
        {
            //var or field
            do
            {
                CheckSyntaxExpected(TokenType.Identifier);
                string name = ReadVariableName();
                sb.Append(name);
                MoveNext();
                if (TokenType == TokenType.Colon)
                {
                    AppendThenNext(c);
                    VisitType();
                }
                if (TokenType == TokenType.Equal)
                {
                    AppendThenNext(c);
                    VisitAssignmentExpression();
                }
                if (TokenType == TokenType.SemiColon)
                    break;
                sb.Append(c);
            } while (MoveNextIf(TokenType.Comma));

        }

        private void VisitExpression()
        {
            VisitAssignmentExpression();
            while (TokenType == TokenType.Comma)
            {
                AppendThenNext();
                VisitAssignmentExpression();
            }
        }

        private void VisitAssignmentExpression()
        {
            VisitConditionalExpression();
            TokenType type = TokenType;
            if (type == TokenType.Equal)
            {
                AppendThenNext();
                VisitAssignmentExpression();
            }
        }

        private void VisitConditionalExpression()
        {
            VisitLogicalORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Question;
                type = TokenType)
            {
                AppendThenNext();
                VisitConditionalExpression();
                AppendThenNext();
                VisitConditionalExpression();
            }
        }

        private void VisitLogicalORExpression()
        {
            VisitLogicalAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.OrOr;
                type = TokenType)
            {
                AppendThenNext();
                VisitLogicalAndExpression();
            }
        }

        private void VisitLogicalAndExpression()
        {

            VisitBitwiseORExpression();
            for (TokenType type = TokenType;
                type == TokenType.AndAnd;
                type = TokenType)
            {
                AppendThenNext();
                VisitBitwiseORExpression();
            }
        }

        private void VisitBitwiseORExpression()
        {
            VisitBitwiseXORExpression();
            for (TokenType type = TokenType;
                type == TokenType.Or;
                type = TokenType)
            {
                AppendThenNext();
                VisitBitwiseXORExpression();
            }
        }

        private void VisitBitwiseXORExpression()
        {
            VisitBitwiseAndExpression();
            for (TokenType type = TokenType;
                type == TokenType.Circumflex;
                type = TokenType)
            {
                AppendThenNext();
                VisitBitwiseAndExpression();
            }
        }

        private void VisitBitwiseAndExpression()
        {
            VisitEqualityExpression();
            for (TokenType type = TokenType;
                type == TokenType.And;
                type = TokenType)
            {
                AppendThenNext();
                VisitEqualityExpression();
            }
        }

        private void VisitEqualityExpression()
        {
            VisitRelationalExpression();
            for (TokenType type = TokenType;
                type == TokenType.EqualEqual || type == TokenType.BangEqual;
                type = TokenType)
            {
                AppendThenNext();
                VisitRelationalExpression();
            }
        }

        private void VisitRelationalExpression()
        {
            VisitShiftExpression();
            for (TokenType type = TokenType;
                type == TokenType.Greater || type == TokenType.GreaterEqual
                || type == TokenType.Less || type == TokenType.LessEqual;
                type = TokenType)
            {
                AppendThenNext();
                VisitShiftExpression();
            }
        }

        private void VisitShiftExpression()
        {
            VisitAdditionExpression();
            for (TokenType type = TokenType;
                type == TokenType.LessLess || type == TokenType.GreaterGreater;
                type = TokenType)
            {
                AppendThenNext();
                VisitAdditionExpression();
            }
        }

        private void VisitAdditionExpression()
        {
            VisitMultiplicationExpression();
            for (TokenType type = TokenType;
                type == TokenType.Plus || type == TokenType.Minus;
                type = TokenType)
            {
                AppendThenNext();
                VisitMultiplicationExpression();
            }
        }

        private void VisitMultiplicationExpression()
        {
            VisitExponentiation();
            for (TokenType type = TokenType;
                type == TokenType.Multiply || type == TokenType.Divide || type == TokenType.Percent;
                type = TokenType)
            {
                AppendThenNext();
                VisitExponentiation();
            }
        }

        private void VisitExponentiation()
        {
            VisitUnaryExpression();
            for (TokenType type = TokenType;
                type == TokenType.StarStar;
                type = TokenType)
            {
                AppendThenNext();
                VisitExponentiation();
            }
        }

        private void VisitUnaryExpression()
        {
            // todo await, typeof, delete 
            switch (TokenType)
            {
                case TokenType.Minus:
                case TokenType.Plus:
                case TokenType.Bang:
                case TokenType.MinusMinus:
                case TokenType.PlusPlus:
                    AppendThenNext();
                    VisitLeftHandSideExpression();
                    break;
                case TokenType.Less:
                    AppendThenNext();
                    VisitType();
                    AppendNextIf(TokenType.Greater);
                    VisitLeftHandSideExpression();
                    break;
                default:
                    VisitPostfixExpression();
                    break;
            }
        }

        private void VisitType()
        {
            if (TokenType == TokenType.Identifier)
            {
                var typeName = ReadTypeName();
                sb.Append(typeName);
                //after type name next
                MoveNext();
                //array
                if (TokenType == TokenType.LeftBracket)
                {
                    sb.Append('[');
                    VisitArrayRanks();
                }
            }
        }

        private void VisitArrayRanks()
        {
            while (TokenType == TokenType.LeftBracket)
            {
                MoveNext();
                VisitExpression();
                if (TokenType == TokenType.RightBracket)
                {
                    sb.Append(c);
                    MoveNext();
                }
            }
        }

        private void VisitPostfixExpression()
        {
            VisitLeftHandSideExpression();
            switch (TokenType)
            {
                case TokenType.MinusMinus:
                case TokenType.PlusPlus:
                    AppendThenNext();
                    break;
            }
        }

        private void VisitLeftHandSideExpression()
        {
            switch (TokenType)
            {
                case TokenType.Numeric:
                    sb.Append(GetNumeric());
                    break;
                case TokenType.String:
                    sb.AppendFormat("`{0}`", ReadString());
                    break;
                case TokenType.Identifier:
                    VisitIdentifier();
                    break;
                case TokenType.SpecialVariable:
                    sb.Append('@');
                    sb.Append(ReadVariableName());
                    break;
                case TokenType.LeftBrace:
                    sb.Append(c);
                    VisitAnonymousObjectMembers();
                    CheckSyntaxExpected(TokenType.RightBrace);
                    sb.Append(c);
                    break;
                case TokenType.LeftParenthesis:
                    AppendThenNext(c);
                    VisitExpression();
                    CheckSyntaxExpected(TokenType.RightParenthesis);
                    sb.Append(c);
                    break;
                case TokenType.LeftBracket:
                    //Might be array
                    VisitArrayLiteral();
                    VisitRightExpression();
                    return;
                case TokenType.RightParenthesis:
                case TokenType.RightBracket:
                    //skip end of expression
                    return;
            }
            MoveNext(false);
            //End of left
            VisitRightExpression();
        }

        private void VisitIdentifier()
        {
            var name = ReadVariableName();
            if (Keywords.TryGetIdentifier(name, out IdentifierType type))
            {
                switch (type)
                {
                    case IdentifierType.New:
                        sb.Append(name);
                        sb.Append(Space);
                        MoveNext();
                        if (TokenType == TokenType.LeftParenthesis)
                        {
                            VisitArgumentList(TokenType.Comma, TokenType.LeftParenthesis);
                            CheckSyntaxExpected(TokenType.RightParenthesis);
                            sb.Append(c);
                        }
                        return;
#if Runtime
                case IdentifierType.Undefined:
                    return Expression.Undefined;
#endif
                        //case IdentifierType.Function:
                        //    // ignore func
                        //    MoveNext();
                        //    var lamda = VisitLamdaExpression();
                        //    //to make last char not semicolon ex:func()=>1;
                        //    Source.FallBack();
                        //    return lamda;
                        //case IdentifierType.SizeOf:
                        //    //skip sizeof
                        //    if (MoveNextThenIf(TokenType.LeftParenthesis))
                        //    {
                        //        return new SizeOfExpression(VisitAssignmentExpression());
                        //    }
                        //    CheckSyntaxExpected(TokenType.RightParenthesis);
                        //    break;
                }
            }
            sb.Append(name);
        }

        private void VisitRightExpression()
        {
            for (TokenType type = TokenType; ; type = TokenType)
            {
                switch (type)
                {
                    case TokenType.LeftParenthesis:
                        VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis);
                        CheckSyntaxExpected(TokenType.RightParenthesis);
                        sb.Append(c);
                        break;
                    case TokenType.NullPropagator:
                        AppendThenNext();
                        VisitPostfixExpression();
                        return;
                    case TokenType.Qualified:
                    case TokenType.Dot:
                        AppendThenNext();
                        sb.Append(ReadVariableName());
                        break;
                    case TokenType.LeftBracket:
                        VisitArgumentList(TokenType.Comma, TokenType.RightBracket);
                        CheckSyntaxExpected(TokenType.RightBracket);
                        sb.Append(c);
                        break;
                    default:
                        return;
                }
                MoveNext();
            }
        }

        private void VisitStatementList()
        {
            while (MoveNextThenIfNot(TokenType.RightBrace))
            {
                AppendTab();
                VisitStatement();
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.SemiColon, TokenType.NewLine);
                sb.Append(c);
                AppendLine();
            }
        }

        private void AppendLine()
        {
            if (formatted)
                return;
            sb.Append(Environment.NewLine);
        }

        private void AppendTab()
        {
            if (formatted)
                return;
            for (int i = 0; i < tab; i++)
            {
                sb.Append('\t');
            }
        }

        private void VisitArrayLiteral()
        {
            //[
            if (TokenType == TokenType.LeftBracket)
            {
                //[
                VisitArgumentList(TokenType.Comma, TokenType.RightBracket);
                AppendNextIf(TokenType.RightBracket);
                // next will go when enters right side
            }
            if (TokenType == TokenType.Less)
            {
                //Next <
                AppendThenNext();
                VisitType();
                //>
                AppendNextIf(TokenType.Greater);
            }
            //(
            if (TokenType == TokenType.LeftParenthesis)
            {
                VisitArgumentList(TokenType.Comma, TokenType.RightParenthesis);
                AppendNextIf(TokenType.RightParenthesis);
            }
        }

        private void VisitAnonymousObjectMembers()
        {
            CheckSyntaxExpected(TokenType.LeftBrace);
            while (MoveNext())
            {
                if (TokenType == TokenType.Identifier)
                {
                    var name = ReadVariableName();
                    sb.Append(name);
                    MoveNext();
                    if (TokenType == TokenType.Colon)
                    {
                        AppendThenNext(c);
                        if (TokenType == TokenType.LeftParenthesis)
                            VisitLamdaExpression();
                        else
                            VisitConditionalExpression();
                    }
                }
                if (TokenType == TokenType.RightBrace)
                    break;
                CheckSyntaxExpected(TokenType.Comma);
            }
        }

        private void VisitLamdaExpression()
        {
            throw new NotImplementedException();
        }

        private void VisitExpressionList(TokenType splitToken, TokenType endToken)
        {
            do
            {
                VisitAssignmentExpression();
                if (TokenType == endToken)
                    break;
                CheckSyntaxExpected(splitToken);
                sb.Append(c);
            } while (MoveNext());
        }

        private void VisitArgumentList(TokenType splitToken, TokenType endToken)
        {
            sb.Append(c);
            while (MoveNextThenIfNot(endToken))
            {
                VisitAssignmentExpression();
                if (TokenType == endToken)
                    break;
                CheckSyntaxExpected(splitToken);
                sb.Append(c);
            }
        }

        internal void CheckSyntaxExpected(TokenType type)
        {
            if (TokenType == type)
                return;
            throw new System.Exception(string.Concat("Invalid token ", c, " expected ", type));
        }

        internal void CheckSyntaxExpected(TokenType type1, TokenType type2)
        {
            if (TokenType == type1 || TokenType == type2)
                return;
            throw new Exception(string.Concat("Invalid token ", c));
        }

        /// <summary>
        /// Indicates <see cref="Parser.TokenType"/> matches the <paramref name="expected"/>
        /// </summary>
        private bool CheckExpectedIdentifier(IdentifierType expected)
        {
            int start = pos;
            var name = ReadVariableName();
            MoveNext();
            if (Keywords.Match(name, expected))
            {
                sb.Append(name);
                return true;
            }
            //Restore
            pos = start - 1;
            //do not skip line info
            MoveNext(false);
            return false;
        }

        #region Interger
        private object GetNumeric()
        {
            char first = c;
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
            return ReadNumber(first);
        }

        private object CreateOctalIntegerLiteral(char first)
        {
            var cb = new CharBuilder();
            cb.Append(first);//0
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
                            cb.Append(next);
                            val = val * 8 + next - '0';
                            ReadChar();
                            continue;
                        }
                }
                break;
            }
            return int.Parse(cb.ToString());
        }

        private object CreateHexIntegerLiteral(char first)
        {
            CharBuilder cb = new CharBuilder();
            cb.Append(first);
            cb.Append(ReadChar());//x or X (ever tested before)
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
                            cb.Append(next);
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
                            cb.Append(next);
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
                            cb.Append(next);
                            val = val * 16 + next - 'A' + 10;
                            ReadChar();
                            continue;
                        }
                }
                break;
            }
            return int.Parse(cb.ToString());
        }

        private object ReadNumber(char first)
        {
            CharBuilder builder = new CharBuilder();
            builder.Append(first);
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
                            builder.Append(next);
                            ReadChar();
                            continue;
                        }
                    case '.':
                        //skip .
                        ReadChar();
                        next = PeekChar();
                        if (char.IsDigit(next))
                        {
                            dot++;
                            //add .
                            builder.Append('.');
                            builder.Append(next);
                            //skip digit
                            ReadChar();
                            if (dot > 1)
                            {
                                break;
                            }
                            continue;
                        }
                        FallBack();
                        break;
                    case 'e':
                    case 'E':
                        {
                            builder.Append(next);
                            c = ReadChar();
                            exp++;
                            if (exp > 1)
                            {
                                break;
                            }
                            next = PeekChar();
                            if (next == '+' || next == '-')
                            {
                                builder.Append(next);
                                ReadChar();
                            }
                            continue;
                        }
                }
                break;
            }
            if (dot > 0)
            {
                return double.Parse(builder.ToString());
            }
            else
            {
                return int.Parse(builder.ToString());
            }
        }
        #endregion

        #region String

        public string ReadTypeName()
        {
            CharBuilder cb = new CharBuilder();
            //todo use do while 
            for (; char.IsLetterOrDigit(c) || c == '_' || c == '.'; c = ReadChar())
            {
                cb.Append(c);
            }
            c = FallBack();
            return cb.ToString();
        }

        public string ReadVariableName()
        {
            CharBuilder cb = new CharBuilder();
            for (; char.IsLetterOrDigit(c) || c == '_'; c = ReadChar())
            {
                cb.Append(c);
            }
            c = FallBack();
            return cb.ToString();
        }

        public string ReadString()
        {
            CharBuilder cb = new CharBuilder();
            for (; c != '`'; c = ReadChar())
            {
                cb.Append(c);
            }
            return cb.ToString();
        }

        #endregion

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
