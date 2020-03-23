using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Identfier Expression
    /// </summary>
    public sealed class NameExpression : Expression
    {
        /// <summary>
        /// Name of the Identifier
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Binding for compiler generation
        /// </summary>
        public Binders.IBinder Binder { get; internal set; }

        /// <summary>
        /// Creates Identifier Expression
        /// </summary>
        /// <param name="name">Name of the Identifier</param>
        /// <param name="opCode">Expression type</param>
        public NameExpression(string name, ExpressionType opCode) : base(opCode)
        {
            Name = name;
        }

        /// <summary>
        /// String Value
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Optimizes or evaluate the expression
        /// </summary>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }

        /// <summary>
        /// Generate Compiled code
        /// </summary>
        /// <param name="generator"></param>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var c = generator.Method.CallingConvention;
            /// static
            if (Binder != null)
            {
                if (Binder.IsMember && Binder.IsStatic == false)
                    generator.LoadArgument(0);
                Binder.GenerateGet(generator);
            }
        }

    }
}
