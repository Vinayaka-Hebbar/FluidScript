namespace FluidScript.Compiler.SyntaxTree
{
    public enum ExpressionType
    {
        Unknown = 0,
        Literal = 1,
        Numeric = 2,
        Octal = 3,
        Hex = 4,
        Unicode = 5,
        String = 6,
        Bool = 7,
        Identifier = 10,
        Parenthesized = 11,
        /// <summary>
        /// method call
        /// </summary>
        Invocation = 12,
        Indexer = 13,
        AnonymousMethod = 14,
        AnonymousObject = 15,
        //either 0->x or x = 0
        Declaration = 16,
        //Known types
        /// <summary>
        /// array block initailization
        /// </summary>
        Block = 17,
        Function = 18,
        Argument = 19,
        //Stop
        Comma = 25,
        //?
        Question = 28,
        /// <summary>
        /// .
        /// </summary>
        MemberAccess = 30,
        QualifiedNamespace = 31,
        New = 32,
        Array = 33,
        Out = 34,
        In = 35,
        This = 36,
        Labeled = 50,

        #region Math
        Plus = 66,
        Minus = 67,
        Multiply = 68,
        Divide = 69,
        Percent = 70,
        /// <summary>
        /// ^
        /// </summary>
        Circumflex = 71,
        /// <summary>
        /// <code>**</code>
        /// <see cref="Lexer.TokenType.StarStar"/>
        /// </summary>
        StarStar = 72,
        #endregion
        #region Logical & Shift
        Equal = 127,
        // <
        Less = 128,
        // > 
        Greater = 129,
        // <=
        LessEqual = 130,
        // >= 
        GreaterEqual = 131,
        // == 
        EqualEqual = 132,
        //!
        Bang = 133,
        // != 
        BangEqual = 134,
        // << 
        LessLess = 135,
        // >> 
        GreaterGreater = 136,
        //|
        Or = 137,
        //||
        OrOr = 138,
        //&
        And = 139,
        //&&
        AndAnd = 140,
        #endregion
        #region PostFix Prefix
        PostfixPlusPlus = 143,
        PostfixMinusMinus = 144,
        PrefixPlusPlus = 145,
        PrefixMinusMinus = 146,
        #endregion
    }
}
