using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AssignmentExpression : Expression
    {
        #region GenerateOption
        const MethodCompileOption Option = MethodCompileOption.Dupplicate | MethodCompileOption.Return;
        #endregion

        public readonly Expression Left;
        public readonly Expression Right;

        public AssignmentExpression(Expression left, Expression right) : base(ExpressionType.Equal)
        {
            Left = left;
            Right = right;
        }

        public ParamConversion Conversion { get; set; }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAssignment(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option)
        {
            if (Left.NodeType == ExpressionType.Identifier)
            {
                var exp = (NameExpression)Left;
                var binder = exp.Binder;
                // binder is null create new local variable
                if (binder is null)
                    exp.Binder = binder = new Binders.VariableBinder(generator.DeclareVariable(Right.Type, exp.Name));
                if ((binder.Attributes & Binders.BindingAttributes.HasThis) != 0)
                    generator.LoadArgument(0);
                Right.GenerateCode(generator, Option);
                if (Conversion != null)
                    generator.EmitConvert(Conversion);
                binder.GenerateSet(Right, generator, option);
            }
            else if (Left.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)Left;
                exp.Target.GenerateCode(generator);
                // member assign for dynamic to be Any
                if (exp.Target.Type.IsValueType && 
                    (exp.Binder.Attributes & Binders.BindingAttributes.Dynamic) == Binders.BindingAttributes.Dynamic)
                    generator.Box(TypeProvider.AnyType);
                Right.GenerateCode(generator, Option);
                if (Conversion != null)
                {
                    generator.EmitConvert(Conversion);
                }

                exp.Binder.GenerateSet(Right, generator, option);
            }
            else if (Left.NodeType == ExpressionType.Indexer)
            {
                GenerateIndexer(generator);
            }
        }

        public void GenerateIndexer(MethodBodyGenerator generator)
        {
            var exp = (IndexExpression)Left;
            var conversions = exp.Conversions;
            var arguments = exp.Arguments;
            exp.Target.GenerateCode(generator, MethodCompileOption.EmitStartAddress);
            var argLength = arguments.Count;
            for (int index = 0; index < argLength; index++)
            {
                var arg = arguments[index];
                var conv = conversions[index];
                if (conv != null)
                {
                    if (conv.ConversionType == ConversionType.Normal)
                    {
                        arg.GenerateCode(generator, AssignOption);
                        generator.EmitConvert(conv);
                    }
                    else if (conv.ConversionType == ConversionType.ParamArray)
                    {
                        var args = new Expression[arguments.Count - index];
                        arguments.CopyTo(args, conv.Index);
                        generator.EmitConvert((ParamArrayConversion)conv, args);
                    }
                }
                else
                {
                    arg.GenerateCode(generator, AssignOption);
                }
            }
            Right.GenerateCode(generator, MethodCompileOption.Dupplicate);
            // value convert if any
            generator.EmitConvert(conversions[argLength]);
            generator.Call(exp.Setter);
        }

        public override string ToString()
        {
            return string.Concat(Left.ToString(), "=", Right.ToString());
        }
    }
}
