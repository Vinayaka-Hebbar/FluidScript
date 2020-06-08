namespace FluidScript.Compiler.Lexer
{
    public enum TokenType : byte
    {
        None = 0,
        //1 For Literal
        Numeric = 2,
        Octal = 3,
        Hex = 4,
        Unicode = 5,
        String = 6,
        StringQuoted = 8,
        Bool = 9,
        // empty slot
        Variable = 10,
        SpecialVariable = 11,
        Identifier = 12,
        /// <summary>
        /// ()
        /// </summary>
        Parenthesized = 13,
        Invocation = 14,
        Indexer = 15,
        /// <summary>
        /// =>
        /// </summary>
        AnonymousMethod = 16,
        //15 skip
        //Reserved
        // Other

        NewLine = 17,
        /// <summary>
        /// [
        /// </summary>
        LeftBracket = 19,
        RightBracket = 20,
        /// <summary>
        /// (
        /// </summary>
        LeftParenthesis = 21,
        /// <summary>
        /// )
        /// </summary>
        RightParenthesis = 22,
        /// <summary>
        /// {
        /// </summary>
        LeftBrace = 23,
        /// <summary>
        /// }
        /// </summary>
        RightBrace = 24,
        Comma = 25,
        SemiColon = 26,
        Colon = 27,
        Question = 28,
        NullPropagator = 29,
        //property access
        Dot = 30,
        //::
        Qualified = 31,

        //Math
        #region Math
        Plus = 66,
        Minus = 67,
        Multiply = 68,
        Divide = 69,
        Percent = 70,
        /// <summary>
        ///^
        /// </summary>
        Circumflex = 71,
        /// <summary>
        /// ** for pow
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
        /// <summary>
        /// !
        /// </summary>
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
        PlusPlus = 141,
        MinusMinus = 142,
        #endregion
        #region Other
        /// <summary>
        /// /
        /// </summary>
        BackSlash = 180,
        /// <summary>
        /// \
        /// </summary>
        ForwardSlash = 181,
        #endregion
        End = 255,
        Bad = 254
    }
}
