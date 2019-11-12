using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
        public static readonly LiteralExpression Null = new LiteralExpression();

        public readonly object Value;

        public LiteralExpression(double value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedType = typeof(double);
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Double;
        }

        private LiteralExpression() : base(ExpressionType.Literal)
        {
            Value = "null";
        }

        public LiteralExpression(string value) : base(ExpressionType.String)
        {
            Value = value;
            ResolvedType = typeof(string);
            ResolvedPrimitiveType = FluidScript.PrimitiveType.String;
        }

        public LiteralExpression(bool value) : base(ExpressionType.Bool)
        {
            Value = value;
            ResolvedType = typeof(bool);
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Bool;
        }

        public LiteralExpression(int value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedType = typeof(int);
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Int32;
        }

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
            ResolvedType = value.GetType();
            //must be primitive
            ResolvedPrimitiveType = TypeUtils.PrimitiveTypes[value.GetType()];
        }

        public override RuntimeObject Evaluate()
        {
            return new Core.PrimitiveObject(Value, ResolvedPrimitiveType);
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            var type = ResolvedPrimitiveType;
            //Unset ObjectType.Number
            switch (type)
            {
                case FluidScript.PrimitiveType.Byte:
                    generator.LoadByte((sbyte)Value);
                    break;
                case FluidScript.PrimitiveType.UByte:
                    generator.LoadByte((byte)Value);
                    break;
                case FluidScript.PrimitiveType.Char:
                    generator.LoadChar((char)Value);
                    break;
                case FluidScript.PrimitiveType.Int16:
                    generator.LoadInt16((short)Value);
                    break;
                case FluidScript.PrimitiveType.UInt16:
                    generator.LoadInt16((ushort)Value);
                    break;
                case FluidScript.PrimitiveType.Int32:
                    generator.LoadInt32((int)Value);
                    break;
                case FluidScript.PrimitiveType.UInt32:
                    generator.LoadInt32((uint)Value);
                    break;
                case FluidScript.PrimitiveType.Int64:
                    generator.LoadInt64((long)Value);
                    break;
                case FluidScript.PrimitiveType.UInt64:
                    generator.LoadInt64((ulong)Value);
                    break;
                case FluidScript.PrimitiveType.Float:
                    generator.LoadSingle((float)Value);
                    break;
                case FluidScript.PrimitiveType.Double:
                    generator.LoadDouble((double)Value);
                    break;
                case FluidScript.PrimitiveType.Bool:
                    generator.LoadBool((bool)Value);
                    break;
                case FluidScript.PrimitiveType.String:
                    generator.LoadString(Value.ToString());
                    break;
                case FluidScript.PrimitiveType.Undefined:
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
