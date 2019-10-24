namespace FluidScript.Compiler
{
    public static class Extesnions
    {
        public static bool IsConditionTrue(this ScriptEngine parser, string text)
        {
            var exp = parser.ParseExpression(text);
            return exp.Evaluate().ToBool();
        }

        public static Object Evaluate(this ScriptEngine parser, string text)
        {
            var exp = parser.ParseExpression(text);
            return exp.Evaluate();
        }
    }
}
