using System;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Scopes;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
        public readonly object Value;

        public LiteralExpression(double value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Double;
        }

        public LiteralExpression(string value) : base(ExpressionType.String)
        {
            Value = value;
            ResolvedPrimitiveType = FluidScript.PrimitiveType.String;
        }

        public LiteralExpression(bool value) : base(ExpressionType.Bool)
        {
            Value = value;
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Bool;
        }

        public LiteralExpression(int value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResolvedPrimitiveType = FluidScript.PrimitiveType.Int32;
        }

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
            //must be primitive
            ResolvedPrimitiveType = TypeUtils.PrimitiveTypes[value.GetType()];
        }

        public override Type ResultType()
        {
            if (ResolvedType == null)
            {
                if (Value == null)
                    return null;
                ResolvedType = Value.GetType();
            }
            return ResolvedType;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            var type = ResolvedPrimitiveType;
            if (type == FluidScript.PrimitiveType.Null)
                generator.LoadNull();
            if (type == FluidScript.PrimitiveType.Bool)
                generator.LoadBool((bool)Value);
            if (type == FluidScript.PrimitiveType.String)
                generator.LoadString(Value.ToString());
            if (type == FluidScript.PrimitiveType.Char)
                generator.LoadChar((char)Value);
            bool isNumber = (type & FluidScript.PrimitiveType.Number) == FluidScript.PrimitiveType.Number;
            if (isNumber)
            {
                //Unset ObjectType.Number
                switch (type)
                {
                    case FluidScript.PrimitiveType.Byte:
                        generator.LoadByte((sbyte)Value);
                        break;
                    case FluidScript.PrimitiveType.UByte:
                        generator.LoadByte((byte)Value);
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
                }
            }
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
