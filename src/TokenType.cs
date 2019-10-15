﻿namespace FluidScript
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
        Variable = 8,
        Constant = 9,
        Identifier = 10,
        Parenthesized = 11,
        Invocation = 12,
        AnnonymousMethod = 13,
        Initializer = 14,
        //15 skip
        //Reserved
        // Other
        LeftParenthesis = 20,
        RightParenthesis = 21,
        LeftBrace = 22,
        RightBrace = 23,
        Comma = 24,
        SemiColon = 25,
        Colon = 26,
        Question = 27,
        NullPropagator = 28,
        //property access
        Dot = 29,
        //::
        Qualified = 30,
        //Math
        #region Math
        Plus = 66,
        Minus = 67,
        Multiply = 68,
        Divide = 69,
        Percent = 70,
        //^
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
        PlusPlus = 141,
        MinusMinus = 142,
        #endregion
        End = 255,
        Bad = 254
    }
}
