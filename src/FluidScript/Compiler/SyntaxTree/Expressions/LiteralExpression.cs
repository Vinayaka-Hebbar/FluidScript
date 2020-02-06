using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Literal expression
    /// </summary>
    public sealed class LiteralExpression : Expression
    {
        /// <summary>
        /// Literal valie
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Initializes new <see cref="LiteralExpression"/>
        /// </summary>
        /// <param name="value"></param>
        public LiteralExpression(object value) : base(ExpressionType.Literal)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
            //todo unsigned to signed
            switch (Value)
            {
                case sbyte _:
                    generator.LoadByte((sbyte)Value);
                    generator.NewObject(Utils.Helpers.Byte_New);
                    break;
                case short _:
                    generator.LoadInt16((short)Value);
                    generator.NewObject(Utils.Helpers.Short_New);
                    break;
                case int _:
                    generator.LoadInt32((int)Value);
                    generator.NewObject(Utils.Helpers.Integer_New);
                    break;
                case long _:
                    generator.LoadInt64((long)Value);
                    generator.NewObject(Utils.Helpers.Long_New);
                    break;
                case float _:
                    generator.LoadSingle((float)Value);
                    generator.NewObject(Utils.Helpers.Float_New);
                    break;
                case double _:
                    generator.LoadDouble((double)Value);
                    generator.NewObject(Utils.Helpers.Double_New);
                    break;
                case bool value:
                    generator.LoadField(value ? Utils.Helpers.Bool_True : Utils.Helpers.Bool_False);
                    break;
                case char _:
                    generator.LoadChar((char)Value);
                    generator.NewObject(Utils.Helpers.Char_New);
                    break;
                case string _:
                    generator.LoadString(Value.ToString());
                    generator.NewObject(Utils.Helpers.String_New);
                    break;
                case null:
                    generator.LoadNull();
                    break;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
