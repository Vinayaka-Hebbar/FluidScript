using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Array literal &lt;<see cref="ArrayType"/>&gt;[1,2]
    /// </summary>
    public sealed class ArrayListExpression : Expression
    {
        /// <summary>
        /// List of array items
        /// </summary>
        public readonly INodeList<Expression> Expressions;

        /// <summary>
        /// Array Size
        /// </summary>
        public readonly INodeList<Expression> Arguments;


        /// <summary>
        /// Array type
        /// </summary>
        public readonly TypeSyntax ArrayType;

        /// <summary>
        /// Initializes new <see cref="ArrayListExpression"/>
        /// </summary>
        public ArrayListExpression(INodeList<Expression> expressions, TypeSyntax type, INodeList<Expression> arguments) : base(ExpressionType.Array)
        {
            Expressions = expressions;
            ArrayType = type;
            Arguments = arguments;
        }

        public System.Reflection.ConstructorInfo Constructor
        {
            get;
            set;
        }

        public Type ElementType { get; set; }

        public Runtime.ArgumentConversions ArgumentConversions { get; set; }

        public Runtime.ArgumentConversions ArrayConversions { get; set; }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitArrayLiteral(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption options)
        {
            if (Arguments != null)
            {
                var conversions = ArgumentConversions;
                for (int i = 0; i < Arguments.Count; i++)
                {
                    Arguments[i].GenerateCode(generator);
                    var conversion = conversions[i];
                    if (conversion != null)
                        generator.EmitConvert(conversion);
                }
            }
            generator.NewObject(Constructor);

            int length = Expressions.Count;
            if (length > 0)
            {
                var variable = generator.DeclareVariable(Type);
                generator.StoreVariable(variable);
                generator.LoadVariable(variable);
                var conversions = ArrayConversions;
                generator.LoadInt32(length);
                Type type = ElementType;
                generator.NewArray(type);
                for (int i = 0; i < length; i++)
                {
                    generator.Duplicate();
                    generator.LoadInt32(i);
                    var expression = Expressions[i];
                    expression.GenerateCode(generator);
                    if (expression.Type.IsValueType && type.IsValueType == false)
                        generator.Box(expression.Type);
                    var convertion = conversions[i];
                    if (convertion != null)
                        generator.EmitConvert(convertion);

                    generator.StoreArrayElement(type);
                }
                var m = Type.GetMethod("AddRange", Utils.ReflectionUtils.PublicInstance);
                generator.Call(m);
                generator.LoadVariable(variable);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string args = Arguments == null ? string.Empty : string.Join(",", Arguments.Map(arg => arg.ToString()));
            if (ArrayType == null)
            {
                return string.Concat("[", string.Join(",", Expressions.Map(exp => exp.ToString())), "]<any>(", args, ")");
            }

            return string.Concat("[", string.Join(",", Expressions.Map(exp => exp.ToString())), "]<", ArrayType, ">(", args, ")");
        }

        public static void MakeObjectArray(MethodBodyGenerator generator, ICollection<Expression> expressions)
        {
            int i = 0;
            generator.LoadInt32(expressions.Count);
            generator.NewArray(typeof(object));
            foreach (var expression in expressions)
            {
                generator.Duplicate();
                generator.LoadInt32(i++);
                expression.Accept(generator).GenerateCode(generator);
                if (expression.Type.IsValueType)
                    generator.Box(expression.Type);
                generator.StoreArrayElement(typeof(object));
            }
        }
    }
}
