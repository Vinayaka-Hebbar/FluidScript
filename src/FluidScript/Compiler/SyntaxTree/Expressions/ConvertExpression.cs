﻿namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// &lt;type&gt;(Expression)
    /// </summary>
    public class ConvertExpression : Expression
    {
        public readonly TypeSyntax TypeName;

        public readonly Expression Target;

        /// <summary>
        /// Conversion method
        /// </summary>
        public System.Reflection.MethodInfo Method { get; set; }

        public ConvertExpression(TypeSyntax typeName, Expression target) : base(ExpressionType.Convert)
        {
            TypeName = typeName;
            Target = target;
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption options)
        {
            Target.GenerateCode(generator);
            if (Method != null)
            {
                // implicit conversion
                generator.CallStatic(Method);
            }
            else
            {
                generator.CastClass(Type);
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitConvert(this);
        }

        public override string ToString()
        {
            return string.Concat("<", TypeName, ">", Target);
        }
    }
}
