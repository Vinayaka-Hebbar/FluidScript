using FluidScript.Compiler.Emit;
using FluidScript.Extensions;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Indexer value[exps]
    /// </summary>
    public sealed class IndexExpression : Expression
    {
        public readonly Expression Target;

        public readonly INodeList<Expression> Arguments;

        /// <summary>
        /// Argument convert list
        /// </summary>
        public Runtime.ArgumentConversions Conversions { get; set; }

        public System.Reflection.MethodInfo Getter { get; set; }

        public System.Reflection.MethodInfo Setter { get; set; }

        public IndexExpression(Expression target, NodeList<Expression> arguments) : base(ExpressionType.Indexer)
        {
            Target = target;
            Arguments = arguments;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitIndex(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption options)
        {
            // for exp.item[0] is Dynamic SafeGetValue will call
            // such case variable address must be emitted
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Target;
                if (exp.Type.IsValueType && (exp.Binder.Attributes & Binders.BindingAttributes.Dynamic) != 0)
                    options |= MethodCompileOption.EmitStartAddress;
            }
            else if(Target.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Target;
                if (exp.Type.IsValueType && (exp.Binder.Attributes & Binders.BindingAttributes.Dynamic) != 0)
                    options |= MethodCompileOption.EmitStartAddress;
            }
            else 
            {
                // todo other cases are valid or not?
                if (Target.Type.IsValueType)
                    options |= MethodCompileOption.EmitStartAddress;
            }
            Target.GenerateCode(generator, options);
            Arguments.ForEach((arg, index) =>
            {
                arg.GenerateCode(generator);
                generator.EmitConvert(Conversions[index]);
            });
            generator.Call(Getter);
        }

        public override string ToString()
        {
            return string.Concat(Target, "[", string.Join(",", Arguments.Map(arg => arg.ToString())), "]");
        }
    }
}
