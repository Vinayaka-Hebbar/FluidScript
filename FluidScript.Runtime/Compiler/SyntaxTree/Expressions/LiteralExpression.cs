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
                    ResolvedType = typeof(Byte);
                    break;
                case short _:
                    generator.LoadInt16((short)Value);
                    generator.NewObject(ReflectionHelpers.Short_New);
                    ResolvedType = typeof(Short);
                    break;
                case int _:
                    generator.LoadInt32((int)Value);
                    generator.NewObject(ReflectionHelpers.Integer_New);
                    ResolvedType = typeof(Integer);
                    break;
                case long _:
                    generator.LoadInt64((long)Value);
                    generator.NewObject(ReflectionHelpers.Long_New);
                    ResolvedType = typeof(Long);
                    break;
                case float _:
                    generator.LoadSingle((float)Value);
                    generator.NewObject(ReflectionHelpers.Float_New);
                    ResolvedType = typeof(Float);
                    break;
                case double _:
                    generator.LoadDouble((double)Value);
                    generator.NewObject(ReflectionHelpers.Double_New);
                    ResolvedType = typeof(Double);
                    break;
                case bool _:
                    generator.LoadBool((bool)Value);
                    generator.NewObject(ReflectionHelpers.Bool_New);
                    ResolvedType = typeof(Boolean);
                    break;
                case string _:
                    generator.LoadString(Value.ToString());
                    generator.NewObject(ReflectionHelpers.String_New);
                    ResolvedType = typeof(String);
                    break;
                case null:
                    generator.LoadNull();
                    ResolvedType = typeof(IFSObject);
                    break;
            }
        }


        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
