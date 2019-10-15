namespace FluidScript
{
    public static class Extesnions
    {
        public static bool IsConditionTrue(this SyntaxParser parser, string text)
        {
            var exp = parser.ParseExpression(text);
            return exp.Evaluate().ToBool();
        }

        public static Object Evaluate(this SyntaxParser parser, string text)
        {
            var exp = parser.ParseExpression(text);
            return exp.Evaluate();
        }
    }
}
