using System;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
        public readonly object Value;

        public LiteralExpression(double value) : base(ExpressionType.Numeric)
        {
            Value = value;
            ResultType = PrimitiveType.Double;
        }

        public LiteralExpression(string value) : base(ExpressionType.String)
        {
            Value = value;
            ResultType = PrimitiveType.String;
        }

        public LiteralExpression(bool value):base(ExpressionType.Bool)
        {
            Value = value;
            ResultType = PrimitiveType.Bool;
        }

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
            //must be primitive
            ResultType = TypeUtils.PrimitiveTypes[value.GetType()];
        }

        public override Type Type
        {
            get
            {
                if (ResolvedType == null)
                {
                    if (Value == null)
                        return null;
                    ResolvedType = Value.GetType();
                }
                return ResolvedType;
            }
        }

        public override PrimitiveType ResultType { get; }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            var type = ResultType;
            if (type == PrimitiveType.Null)
                generator.LoadNull();
            if (type == PrimitiveType.Bool)
                generator.LoadBool((bool)Value);
            if (type == PrimitiveType.String)
                generator.LoadString(Value.ToString());
            if (type == PrimitiveType.Char)
                generator.LoadChar((char)Value);
            bool isNumber = (type & PrimitiveType.Number) == PrimitiveType.Number;
            if (isNumber)
            {
                //Unset ObjectType.Number
                switch (type)
                {
                    case PrimitiveType.Byte:
                        generator.LoadByte((sbyte)Value);
                        break;
                    case PrimitiveType.UByte:
                        generator.LoadByte((byte)Value);
                        break;
                    case PrimitiveType.Int16:
                        generator.LoadInt16((short)Value);
                        break;
                    case PrimitiveType.UInt16:
                        generator.LoadInt16((ushort)Value);
                        break;
                    case PrimitiveType.Int32:
                        generator.LoadInt32((int)Value);
                        break;
                    case PrimitiveType.UInt32:
                        generator.LoadInt32((uint)Value);
                        break;
                    case PrimitiveType.Int64:
                        generator.LoadInt64((long)Value);
                        break;
                    case PrimitiveType.UInt64:
                        generator.LoadInt64((ulong)Value);
                        break;
                    case PrimitiveType.Float:
                        generator.LoadSingle((float)Value);
                        break;
                    case PrimitiveType.Double:
                        generator.LoadDouble((double)Value);
                        break;
                }
            }
        }

        public override object GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
