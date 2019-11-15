using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
#if Runtime
        public static readonly LiteralExpression Null = new LiteralExpression(RuntimeObject._null, RuntimeType.Any);

        public static readonly Expression Undefined = new LiteralExpression(RuntimeObject._undefined, RuntimeType.Undefined);
#else
        public static readonly LiteralExpression Null = new LiteralExpression(null, RuntimeType.Undefined);
#endif

        public readonly object Value;

        public LiteralExpression(double value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedType = typeof(double);
            ResolvedPrimitiveType = FluidScript.RuntimeType.Double;
        }

        public LiteralExpression(string value) : base(ExpressionType.String)
        {
            Value = value;
            ResolvedType = typeof(string);
            ResolvedPrimitiveType = FluidScript.RuntimeType.String;
        }

        public LiteralExpression(bool value) : base(ExpressionType.Bool)
        {
            Value = value;
            ResolvedType = typeof(bool);
            ResolvedPrimitiveType = FluidScript.RuntimeType.Bool;
        }

        public LiteralExpression(int value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedType = typeof(int);
            ResolvedPrimitiveType = FluidScript.RuntimeType.Int32;
        }

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
            ResolvedType = value.GetType();
            //must be primitive
            ResolvedPrimitiveType = TypeUtils.PrimitiveTypes[value.GetType()];
        }

        public LiteralExpression(object value, RuntimeType type) : base(ExpressionType.Literal)
        {
            Value = value;
            ResolvedType = typeof(object);
            //must be primitive
            ResolvedPrimitiveType = type;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return new Core.PrimitiveObject(Value, ResolvedPrimitiveType);
        }
#else
        public override object Evaluate()
        {
            return Value;
        }
#endif

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            var type = ResolvedPrimitiveType;
            //Unset ObjectType.Number
            switch (type)
            {
                case FluidScript.RuntimeType.Byte:
                    generator.LoadByte((sbyte)Value);
                    break;
                case FluidScript.RuntimeType.UByte:
                    generator.LoadByte((byte)Value);
                    break;
                case FluidScript.RuntimeType.Char:
                    generator.LoadChar((char)Value);
                    break;
                case FluidScript.RuntimeType.Int16:
                    generator.LoadInt16((short)Value);
                    break;
                case FluidScript.RuntimeType.UInt16:
                    generator.LoadInt16((ushort)Value);
                    break;
                case FluidScript.RuntimeType.Int32:
                    generator.LoadInt32((int)Value);
                    break;
                case FluidScript.RuntimeType.UInt32:
                    generator.LoadInt32((uint)Value);
                    break;
                case FluidScript.RuntimeType.Int64:
                    generator.LoadInt64((long)Value);
                    break;
                case FluidScript.RuntimeType.UInt64:
                    generator.LoadInt64((ulong)Value);
                    break;
                case FluidScript.RuntimeType.Float:
                    generator.LoadSingle((float)Value);
                    break;
                case FluidScript.RuntimeType.Double:
                    generator.LoadDouble((double)Value);
                    break;
                case FluidScript.RuntimeType.Bool:
                    generator.LoadBool((bool)Value);
                    break;
                case FluidScript.RuntimeType.String:
                    generator.LoadString(Value.ToString());
                    break;
                case FluidScript.RuntimeType.Undefined:
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
