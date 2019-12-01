using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
        public readonly object Value;

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            if (Value is string)
                return new Library.StringObject(Value.ToString());
            return new Library.PrimitiveObject(Value, TypeUtils.GetRuntimeType(Value.GetType()));
        }

        public override RuntimeObject Evaluate(Metadata.Prototype prototype)
        {
            if (Value is string)
                return new Library.StringObject(Value.ToString());
            return new Library.PrimitiveObject(Value, TypeUtils.GetRuntimeType(Value.GetType()));
        }
#else
        public override object Evaluate()
        {
            return Value;
        }
#endif

        protected override void ResolveType(MethodBodyGenerator method)
        {
            ResolvedType = Value.GetType();
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var type = GetRuntimeType(generator);
            //Unset ObjectType.Number
            switch (type)
            {
                case RuntimeType.Byte:
                    generator.LoadByte((sbyte)Value);
                    break;
                case RuntimeType.UByte:
                    generator.LoadByte((byte)Value);
                    break;
                case RuntimeType.Char:
                    generator.LoadChar((char)Value);
                    break;
                case RuntimeType.Int16:
                    generator.LoadInt16((short)Value);
                    break;
                case RuntimeType.UInt16:
                    generator.LoadInt16((ushort)Value);
                    break;
                case RuntimeType.Int32:
                    generator.LoadInt32((int)Value);
                    break;
                case RuntimeType.UInt32:
                    generator.LoadInt32((uint)Value);
                    break;
                case RuntimeType.Int64:
                    generator.LoadInt64((long)Value);
                    break;
                case RuntimeType.UInt64:
                    generator.LoadInt64((ulong)Value);
                    break;
                case RuntimeType.Float:
                    generator.LoadSingle((float)Value);
                    break;
                case RuntimeType.Double:
                    generator.LoadDouble((double)Value);
                    break;
                case RuntimeType.Bool:
                    generator.LoadBool((bool)Value);
                    break;
                case RuntimeType.String:
                    generator.LoadString(Value.ToString());
                    break;
                case RuntimeType.Undefined:
                    generator.LoadNull();
                    break;
            }
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
