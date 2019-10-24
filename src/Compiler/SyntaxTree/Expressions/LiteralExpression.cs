using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LiteralExpression : Expression
    {
        public readonly Object Value;

        public LiteralExpression(double value) : base(NodeType.Numeric)
        {
            Value = new Object(value);
        }

        public LiteralExpression(string value) : base(NodeType.String)
        {
            Value = new Object(value);
        }

        public LiteralExpression(Object value) : base(NodeType.Literal)
        {
            Value = value;
        }

        public override ObjectType ResultType => Value.Type;

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitLiteral(this);
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
        {
            var type = Value.Type;
            if (type == ObjectType.Null)
                generator.LoadNull();
            if (type == ObjectType.Bool)
                generator.LoadBool(Value.ToBool());
            if (type == ObjectType.String)
                generator.LoadString(Value.ToString());
            if (type == ObjectType.Char)
                generator.LoadChar(Value.ToChar());
            bool isNumber = (type & ObjectType.Number) == ObjectType.Number;
            if (isNumber)
            {
                var value = Value.Raw;
                //Unset ObjectType.Number
                switch (type & (~ObjectType.Number))
                {
                    case ObjectType.Byte:
                        generator.LoadByte((sbyte)value);
                        break;
                    case ObjectType.UByte:
                        generator.LoadByte((byte)value);
                        break;
                    case ObjectType.Int16:
                        generator.LoadInt16((short)value);
                        break;
                    case ObjectType.UInt16:
                        generator.LoadInt16((ushort)value);
                        break;
                    case ObjectType.Int32:
                        generator.LoadInt32((int)value);
                        break;
                    case ObjectType.UInt32:
                        generator.LoadInt32((uint)value);
                        break;
                    case ObjectType.Int64:
                        generator.LoadInt64((long)value);
                        break;
                    case ObjectType.UInt64:
                        generator.LoadInt64((ulong)value);
                        break;
                    case ObjectType.Float:
                        generator.LoadSingle((float)value);
                        break;
                    case ObjectType.Double:
                        generator.LoadDouble((double)value);
                        break;
                }
            }
        }

        public override Object GetValue()
        {
            return Value;
        }

        public override string ToString()
        {
            return Value.GetTypeName();
        }
    }
}
