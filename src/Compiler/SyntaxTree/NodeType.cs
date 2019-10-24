namespace FluidScript.Compiler.SyntaxTree
{
    public enum NodeType
    {
        Unknown = 0,
        Literal = 1,
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
        AnonymousMethod = 13,
        Declaration = 14,
        //Known types
        Block = 15,
        Function = 16,
        Argument = 17,
        //Stop
        Comma = 24,
        //?
        Question = 27,
        PropertyAccess = 29,
        QualifiedNamespace = 30,
        New = 31,
        Array = 32,
        Out = 33,
        In = 34,
        This = 35,
        Expression = 36,
        Return = 37,
        Throw = 38,
        If = 39,
        While = 40,
        For = 41,

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
        PostfixPlusPlus = 143,
        PostfixMinusMinus = 144,
        PrefixPlusPlus = 145,
        PrefixMinusMinus = 146,
        #endregion
    }
}
