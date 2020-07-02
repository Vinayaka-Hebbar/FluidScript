namespace FluidScript.Compiler.SyntaxTree
{
    internal class SystemLiternalExpression : Expression
    {
        public readonly object Value;

        public SystemLiternalExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
            if (value is object)
                Type = Value.GetType();
        }

        /// <inheritdoc/>
        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption options)
        {
            //todo unsigned to signed
            switch (Value)
            {
                case sbyte _:
                    generator.LoadByte((sbyte)Value);
                    break;
                case short _:
                    generator.LoadInt16((short)Value);
                    break;
                case int _:
                    generator.LoadInt32((int)Value);
                    break;
                case long _:
                    generator.LoadInt64((long)Value);
                    break;
                case float _:
                    generator.LoadSingle((float)Value);
                    break;
                case double _:
                    generator.LoadDouble((double)Value);
                    break;
                case bool value:
                    generator.LoadBool((bool)Value);
                    break;
                case char _:
                    generator.LoadChar((char)Value);
                    break;
                case string _:
                    generator.LoadString(Value.ToString());
                    break;
                case null:
                    generator.LoadNull();
                    break;
            }
        }

        public override string ToString()
        {
            switch (Value)
            {
                case null:
                    return LiteralExpression.NullString;
                case string _:
                    return string.Concat("'", Value, "'");
            }
            return Value.ToString();
        }
    }
}
