using FluidScript.Compiler.Binders;
using FluidScript.Compiler.Emit;
using System;
using System.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class UnaryExpression : Expression
    {
        public readonly Expression Operand;

        public UnaryExpression(Expression operand, ExpressionType opcode)
            : base(opcode)
        {
            Operand = operand;
        }

        /// <summary>
        /// Operator overload method
        /// </summary>
        public MethodInfo Method { get; internal set; }


        /// <summary>
        /// Get the unary operator method name
        /// </summary>
        public string MethodName
        {
            get
            {
                string name = null;
                switch (NodeType)
                {
                    case ExpressionType.PostfixPlusPlus:
                        name = "op_Increment";
                        break;
                    case ExpressionType.PrefixPlusPlus:
                        name = "op_Increment";
                        break;
                    case ExpressionType.PostfixMinusMinus:
                        name = "op_Decrement";
                        break;
                    case ExpressionType.PrefixMinusMinus:
                        name = "op_Decrement";
                        break;
                    case ExpressionType.Bang:
                        name = "op_LogicalNot";
                        break;
                    case ExpressionType.Plus:
                        name = "op_UnaryPlus";
                        break;
                    case ExpressionType.Minus:
                        name = "op_UnaryNegation";
                        break;
                    case ExpressionType.Circumflex:
                        name = "op_ExclusiveOr";
                        break;
                    case ExpressionType.Or:
                        name = "op_BitwiseOr";
                        break;
                    case ExpressionType.And:
                        name = "op_BitwiseAnd";
                        break;
                    case ExpressionType.Tilda:
                        name = "op_OnesComplement";
                        break;
                }
                return name;
            }
        }

        /// <summary>
        /// Type conversion of arguments
        /// </summary>
        public ArgumentConversions Conversions { get; set; }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            if (NodeType == ExpressionType.Parenthesized)
            {
                Operand.GenerateCode(generator);
                return;
            }
            var operand = Operand;
            IBinder binder = null;
            if (operand.NodeType == ExpressionType.Identifier)
            {
                //todo only for get; set; member
                //i++ or Member++
                binder = ((NameExpression)operand).Binder;
            }
            else if (operand.NodeType == ExpressionType.MemberAccess)
            {
                binder = ((MemberExpression)operand).Binder;
            }
            // todo: Conversion if value is short, byte lower data type
            if (binder == null)
                throw new NullReferenceException(nameof(binder));
            switch (NodeType)
            {
                case ExpressionType.PostfixMinusMinus:
                case ExpressionType.PostfixPlusPlus:
                    if (binder.CanEmitThis)
                        generator.LoadArgument(0);
                    operand.GenerateCode(generator, MethodGenerateOption.Dupplicate);
                    if (binder.IsMember)
                    {
                        CallPostFixMember(generator, binder, option);
                        return;
                    }
                    if ((option & MethodGenerateOption.Dupplicate) != 0)
                        generator.Duplicate();
                    // call the operator
                    generator.CallStatic(Method);
                    // update value
                    binder.GenerateSet(generator);
                    break;
                case ExpressionType.PrefixMinusMinus:
                case ExpressionType.PrefixPlusPlus:
                    if (binder.CanEmitThis)
                        generator.LoadArgument(0);
                    operand.GenerateCode(generator, MethodGenerateOption.Dupplicate);
                    // call the operator
                    generator.CallStatic(Method);
                    if (binder.IsMember)
                    {
                        CallPreFixMember(generator, binder, option);
                        return;
                    }
                    if ((option & MethodGenerateOption.Dupplicate) != 0)
                        generator.Duplicate();
                    // update value
                    binder.GenerateSet(generator);
                    break;
                default:
                    // call the operator
                    operand.GenerateCode(generator, MethodGenerateOption.Dupplicate);
                    generator.CallStatic(Method);
                    break;
            }

        }

        void CallPreFixMember(MethodBodyGenerator generator, IBinder binder, MethodGenerateOption option)
        {
            // if no duplicate ex: i++ single line 
            if ((option & MethodGenerateOption.Dupplicate) == 0)
            {
                binder.GenerateSet(generator);
                return;
            }
            // ++i where i is member
            // initially operator is called
            var temp = generator.DeclareVariable(binder.Type);
            // store the result to variable
            generator.StoreVariable(temp);
            // then load the variable
            generator.LoadVariable(temp);
            // store the variable result to member
            binder.GenerateSet(generator);
            // load the temp variable
            generator.LoadVariable(temp);
        }

        void CallPostFixMember(MethodBodyGenerator generator, IBinder binder, MethodGenerateOption option)
        {
            // if no duplicate ex: i++ single line 
            if ((option & MethodGenerateOption.Dupplicate) == 0)
            {
                binder.GenerateSet(generator);
                return;
            }
            // i++ where i is member
            // intially member is loaded
            var temp = generator.DeclareVariable(binder.Type);
            // store the result to variable
            generator.StoreVariable(temp);
            // load the variable
            generator.LoadVariable(temp);
            // call operator
            generator.CallStatic(Method);
            // store operation result to member
            binder.GenerateSet(generator);
            //load the temp variable
            generator.LoadVariable(temp);

        }

        public override string ToString()
        {
            var result = Operand.ToString();
            switch (NodeType)
            {
                case ExpressionType.Parenthesized:
                    return string.Concat('(', result, ')');
                case ExpressionType.PostfixPlusPlus:
                    return string.Concat(result, "++");
                case ExpressionType.PostfixMinusMinus:
                    return string.Concat(result, "--");
                case ExpressionType.PrefixPlusPlus:
                    return string.Concat("++", result);
                case ExpressionType.PrefixMinusMinus:
                    return string.Concat("--", result);
                case ExpressionType.Bang:
                    return string.Concat("!", result);
                case ExpressionType.Plus:
                    return string.Concat("+", result);
                case ExpressionType.Minus:
                    return string.Concat("-", result);
                case ExpressionType.Tilda:
                    return string.Concat("~", result);
                case ExpressionType.Out:
                    return string.Concat("out ", result);
                default:
                    return base.ToString();
            }
        }
    }
}
