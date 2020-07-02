using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Literal expression
    /// </summary>
    public class LiteralExpression : Expression
    {
        internal const string NullString = "null";

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

        object m_value;
        /// <summary>
        /// Reflected Value of <see cref="Value"/>
        /// </summary>
        public virtual object ReflectedValue
        {
            get
            {
                if (m_value == null)
                {
                    switch (Value)
                    {
                        case int i:
                            m_value = new Integer(i);
                            break;
                        case double d:
                            m_value = new Double(d);
                            break;
                        case string s:
                            m_value = new String(s);
                            break;
                        case bool b:
                            m_value = b ? Boolean.True : Boolean.False;
                            break;
                        default:
                            m_value = Value;
                            break;
                    }
                    Type = m_value.GetType();
                }
                return m_value;
            }
        }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption options)
        {
            //todo unsigned to signed
            switch (Value)
            {
                case sbyte _:
                    generator.LoadByte((sbyte)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Byte_New);
                    break;
                case short _:
                    generator.LoadInt16((short)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Short_New);
                    break;
                case int _:
                    generator.LoadInt32((int)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Integer_New);
                    break;
                case long _:
                    generator.LoadInt64((long)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Long_New);
                    break;
                case float _:
                    generator.LoadSingle((float)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Float_New);
                    break;
                case double _:
                    generator.LoadDouble((double)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Double_New);
                    break;
                case bool value:
                    generator.LoadField(value ? Utils.ReflectionHelpers.Bool_True : Utils.ReflectionHelpers.Bool_False);
                    break;
                case char _:
                    generator.LoadChar((char)Value);
                    generator.NewObject(Utils.ReflectionHelpers.Char_New);
                    break;
                case string _:
                    generator.LoadString(Value.ToString());
                    generator.NewObject(Utils.ReflectionHelpers.String_New);
                    break;
                case null:
                    generator.LoadNull();
                    break;
            }
        }

        /// <inheritdoc/>
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
