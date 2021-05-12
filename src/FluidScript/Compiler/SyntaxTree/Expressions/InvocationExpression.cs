using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Runtime;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class InvocationExpression : Expression
    {
        public readonly Expression Target;

        public readonly NodeList<Expression> Arguments;

        public System.Reflection.MethodBase Method { get; set; }

        /// <summary>
        /// Argument convert list
        /// </summary>
        public ArgumentConversions Conversions { get; set; }

        public InvocationExpression(Expression target, NodeList<Expression> arguments) : base(ExpressionType.Invocation)
        {
            Target = target;
            Arguments = arguments;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitCall(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option)
        {
            GenerateCode(generator, option, Target);
            generator.EmitArguments(Arguments, Conversions);
            generator.Call(Method);
            // if current value must not be returned for assigment
            if ((option & MethodCompileOption.Return) == 0 && Type is object && Type != TypeProvider.VoidType)
                generator.Pop();
            if ((option & MethodCompileOption.EmitStartAddress) != 0
                && Type.IsValueType)
            {
                var temp = generator.DeclareVariable(Type);
                generator.StoreVariable(temp);
                generator.LoadAddressOfVariable(temp);
            }
        }

        void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option, MemberExpression exp)
        {
            switch (exp.Target.NodeType)
            {
                case ExpressionType.Indexer:
                    GenerateAddressOfResult(generator, option, (IndexExpression)exp.Target);
                    return;
                case ExpressionType.Invocation:
                    exp.GenerateCode(generator, option);
                    if (Method.IsAbstract && exp.Type.IsDynamicInvocable())
                    {
                        generator.Box(exp.Type);
                        return;
                    }
                    var temp = generator.DeclareVariable(exp.Target.Type);
                    generator.StoreVariable(temp);
                    generator.LoadAddressOfVariable(temp);
                    return;
                case ExpressionType.MemberAccess:
                    exp.GenerateCode(generator, option);
                    temp = generator.DeclareVariable(exp.Target.Type);
                    generator.StoreVariable(temp);
                    generator.LoadAddressOfVariable(temp);
                    return;
                case ExpressionType.Identifier:
                    var name = (NameExpression)exp.Target;
                    if (Method.IsAbstract && exp.Type.IsDynamicInvocable())
                    {
                        // Don't load the address for IDynamicInvocable
                        option &= ~MethodCompileOption.EmitStartAddress;
                        exp.GenerateCode(generator, option);
                        generator.Box(name.Type);
                        return;
                    }

                    option |= MethodCompileOption.EmitStartAddress;
                    exp.GenerateCode(generator, option);
                    return;
                case ExpressionType.Parenthesized:
                    GenerateCode(generator, option, ((UnaryExpression)exp.Target).Operand);
                    return;
                default:
                    exp.GenerateCode(generator, option);
                    return;

            }
        }

        void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option, Expression target)
        {
            // the target may require to emit address
            if (target.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression exp = (MemberExpression)target;
                if (exp.Target.Type.IsValueType)
                {
                    GenerateCode(generator, option, exp);
                }
                else
                {
                    target.GenerateCode(generator, option);
                }
            }
            else if (target.NodeType == ExpressionType.Identifier)
            {
                // if it an identifier expression this might be local member call
                var exp = (NameExpression)target;
                bool isDynamic = Method.IsAbstract && exp.Type.IsDynamicInvocable();
                // if not call IDynamicInocable.Invoke()
                if (exp.Type.IsValueType && !isDynamic)
                    option |= MethodCompileOption.EmitStartAddress;

                exp.GenerateCode(generator, option);
                if (isDynamic)
                    generator.Box(exp.Type);
                if (exp.Binder == null && Method.IsStatic == false)
                    generator.LoadArgument(0);
            }
            else if (target.NodeType == ExpressionType.Indexer)
            {
                if (target.Type.IsValueType)
                {
                    GenerateAddressOfResult(generator, option, (IndexExpression)target);
                    return;
                }
                target.GenerateCode(generator, option);
            }
            else
            {
                // other like parenthesis
                target.GenerateCode(generator, option);
                if (target.Type.IsValueType)
                {
                    var temp = generator.DeclareVariable(target.Type);
                    generator.StoreVariable(temp);
                    generator.LoadAddressOfVariable(temp);
                }

            }

        }

        void GenerateAddressOfResult(MethodBodyGenerator generator, MethodCompileOption option, IndexExpression exp)
        {
            ILLocalVariable temp;
            if (exp.Target is Binders.IBindable)
            {
                var member = (Binders.IBindable)exp.Target;
                if (Method.IsAbstract && (member.Binder.Attributes & Binders.BindingAttributes.Dynamic) != 0)
                {
                    exp.GenerateCode(generator, option);
                    temp = generator.DeclareVariable(exp.Type);
                    generator.StoreVariable(temp);
                    generator.LoadVariable(temp);
                    generator.Box(exp.Type);
                    return;
                }
            }
            exp.GenerateCode(generator, option | MethodCompileOption.EmitStartAddress);
            temp = generator.DeclareVariable(exp.Type);
            generator.StoreVariable(temp);
            generator.LoadAddressOfVariable(temp);
        }

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Map(arg => arg.ToString())), ")");
        }
    }
}
