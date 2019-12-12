using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LiteralExpression : Expression
    {
        public readonly object Value;

        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitLiteral(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            //todo unsigned to signed
            switch (Value)
            {
                case sbyte _:
                    generator.LoadByte((sbyte)Value);
                    generator.NewObject(ReflectionHelpers.Byte_New);
                    Type = typeof(Byte);
                    break;
                case short _:
                    generator.LoadInt16((short)Value);
                    generator.NewObject(ReflectionHelpers.Short_New);
                    Type = typeof(Short);
                    break;
                case int _:
                    generator.LoadInt32((int)Value);
                    generator.NewObject(ReflectionHelpers.Integer_New);
                    Type = typeof(Integer);
                    break;
                case long _:
                    generator.LoadInt64((long)Value);
                    generator.NewObject(ReflectionHelpers.Long_New);
                    Type = typeof(Long);
                    break;
                case float _:
                    generator.LoadSingle((float)Value);
                    generator.NewObject(ReflectionHelpers.Float_New);
                    Type = typeof(Float);
                    break;
                case double _:
                    generator.LoadDouble((double)Value);
                    generator.NewObject(ReflectionHelpers.Double_New);
                    Type = typeof(Double);
                    break;
                case bool _:
                    generator.LoadBool((bool)Value);
                    generator.NewObject(ReflectionHelpers.Bool_New);
                    Type = typeof(Boolean);
                    break;
                case string _:
                    generator.LoadString(Value.ToString());
                    generator.NewObject(ReflectionHelpers.String_New);
                    Type = typeof(String);
                    break;
                case null:
                    generator.LoadNull();
                    Type = typeof(IFSObject);
                    break;
            }
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
