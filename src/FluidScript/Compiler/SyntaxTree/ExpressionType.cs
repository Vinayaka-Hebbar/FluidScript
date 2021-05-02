using FluidScript.Compiler.Lexer;

namespace FluidScript.Compiler.SyntaxTree
{
    public enum ExpressionType
    {
        Unknown = TokenType.None,
        Literal = 1,
        Numeric = TokenType.Numeric,
        Octal = 3,
        Hex = 4,
        Unicode = TokenType.Unicode,
        String = TokenType.String,
        Bool = TokenType.Bool,
        Identifier = TokenType.Identifier,
        Parenthesized = 11,

        Argument = TokenType.Parenthesized,
        /// <summary>
        /// method call
        /// </summary>
        Invocation = TokenType.Invocation,
        Indexer = TokenType.Indexer,
        AnonymousMethod = TokenType.AnonymousMethod,
        AnonymousObject = 17,
        //either 0->x or x = 0
        Declaration = 18,
        //Known types
        //Stop
        Comma = TokenType.Comma,
        //?
        Question = TokenType.Question,
        /// <summary>
        /// .
        /// </summary>
        MemberAccess = TokenType.Dot,
        QualifiedNamespace = TokenType.Qualified,
        New = 32,
        Array = 33,
        Out = 34,
        In = 35,
        This = 36,
        Super = 37,
        InstanceOf = 38,
        Convert = 39,
        Labeled = 50,


        /// <summary>
        /// array block initailization
        /// </summary>
        Block = 53,
        Function = 54,

        #region Math
        Plus = TokenType.Plus,
        Minus = TokenType.Minus,
        Multiply = TokenType.Multiply,
        Divide = TokenType.Divide,
        Percent = TokenType.Percent,
        /// <summary>
        /// ^
        /// </summary>
        Circumflex = TokenType.Circumflex,
        /// <summary>
        /// <code>**</code>
        /// <see cref="Lexer.TokenType.StarStar"/>
        /// </summary>
        StarStar = TokenType.StarStar,
        #endregion

        #region Logical & Shift
        Equal = TokenType.Equal,
        // <
        Less = TokenType.Less,
        // > 
        Greater = TokenType.Greater,
        // <=
        LessEqual = TokenType.LessEqual,
        // >= 
        GreaterEqual = TokenType.GreaterEqual,
        // == 
        EqualEqual = TokenType.EqualEqual,
        //!
        Bang = TokenType.Bang,
        // != 
        BangEqual = TokenType.BangEqual,
        // << 
        LessLess = TokenType.LessLess,
        // >> 
        GreaterGreater = TokenType.GreaterGreater,
        //|
        Or = TokenType.Or,
        //||
        OrOr = TokenType.OrOr,
        //&
        And = TokenType.And,
        //&&
        AndAnd = TokenType.AndAnd,
        #endregion

        #region PostFix Prefix
        PostfixPlusPlus = 143,
        PostfixMinusMinus = 144,
        PrefixPlusPlus = 145,
        PrefixMinusMinus = 146,
        #endregion

        #region Other
        Tilda = TokenType.Tilda,
        #endregion
        Custom = 253
    }
}
