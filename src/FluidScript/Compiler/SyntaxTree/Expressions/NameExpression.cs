using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{

    /// <summary>
    /// Identfier Expression
    /// </summary>
    public sealed class NameExpression : Expression, Binders.IBinderProvider
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
        /// Target object 
        /// </summary>
        internal object Target { get; set; }

        /// <summary>
        /// Creates Identifier Expression
        /// </summary>
        /// <param name="name">Name of the Identifier</param>
        /// <param name="expType">Expression type</param>
        public NameExpression(string name, ExpressionType expType) : base(expType)
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
        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            // for static no binder
            if (Binder != null)
            {
                if (Binder.CanEmitThis)
                    generator.LoadArgument(0);
                Binder.GenerateGet(generator);
            }
        }

    }
}
