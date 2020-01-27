namespace FluidScript.Compiler
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
        Bool = 7,
        // empty slot
        Variable = 8,
        SpecialVariable = 9,
        Identifier = 10,
        Parenthesized = 11,
        Invocation = 12,
        Indexer = 13,
        /// <summary>
        /// =>
        /// </summary>
        AnonymousMethod = 14,
        //15 skip
        //Reserved
        // Other

        NewLine = 16,
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
        End = 255,
        Bad = 254
    }
}
