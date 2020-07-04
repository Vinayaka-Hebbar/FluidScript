namespace FluidScript.Compiler.SyntaxTree
{
    internal class SystemLiternalExpression : LiteralExpression
    {
        public SystemLiternalExpression(object value) : base(value)
        {
            if (value is object)
                Type = Value.GetType();
        }

        public override object ReflectedValue => Value;

        /// <inheritdoc/>
        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption options)
        {
            //todo unsigned to signed
            switch (Value)
            {
                case sbyte b:
                    generator.LoadByte(b);
                    break;
                case byte b:
                    generator.LoadByte(b);
                    break;
                case short s:
                    generator.LoadInt16(s);
                    break;
                case ushort s:
                    generator.LoadInt16(s);
                    break;
                case int i:
                    generator.LoadInt32(i);
                    break;
                case uint i:
                    generator.LoadInt32(i);
                    break;
                case long l:
                    generator.LoadInt64(l);
                    break;
                case ulong l:
                    generator.LoadInt64(l);
                    break;
                case float f:
                    generator.LoadSingle(f);
                    break;
                case double d:
                    generator.LoadDouble(d);
                    break;
                case bool b:
                    generator.LoadBool(b);
                    break;
                case char c:
                    generator.LoadChar(c);
                    break;
                case string s:
                    generator.LoadString(s);
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
                    return NullString;
                case string _:
                    return string.Concat("'", Value, "'");
            }
            return Value.ToString();
        }
    }
}
