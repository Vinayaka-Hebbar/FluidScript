﻿using FluidScript.Compiler.Binders;
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
                        name = Operators.Increment;
                        break;
                    case ExpressionType.PrefixPlusPlus:
                        name = Operators.Increment;
                        break;
                    case ExpressionType.PostfixMinusMinus:
                        name = Operators.Decrement;
                        break;
                    case ExpressionType.PrefixMinusMinus:
                        name = Operators.Decrement;
                        break;
                    case ExpressionType.Bang:
                        name = Operators.LogicalNot;
                        break;
                    case ExpressionType.Plus:
                        name = Operators.UnaryPlus;
                        break;
                    case ExpressionType.Minus:
                        name = Operators.UnaryNegation;
                        break;
                    case ExpressionType.Circumflex:
                        name = Operators.ExclusiveOr;
                        break;
                    case ExpressionType.Or:
                        name = Operators.BitwiseOr;
                        break;
                    case ExpressionType.And:
                        name = Operators.BitwiseAnd;
                        break;
                    case ExpressionType.Tilda:
                        name = Operators.OnesComplement;
                        break;
                }
                return name;
            }
        }

        /// <summary>
        /// Type conversion of arguments
        /// </summary>
        public Runtime.ArgumentConversions Conversions { get; set; }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option)
        {
            if (NodeType == ExpressionType.Parenthesized)
            {
                Operand.GenerateCode(generator, option);
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

            switch (NodeType)
            {
                case ExpressionType.PostfixMinusMinus:
                case ExpressionType.PostfixPlusPlus:
                    if (binder == null)
                        throw new NullReferenceException(nameof(binder));
                    if ((binder.Attributes & BindingAttributes.HasThis) != 0)
                        generator.LoadArgument(0);
                    operand.GenerateCode(generator, MethodCompileOption.Dupplicate);
                    if ((binder.Attributes & BindingAttributes.Member) != 0)
                    {
                        CallPostFixMember(generator, binder, option);
                        return;
                    }
                    if ((option & MethodCompileOption.Dupplicate) != 0)
                        generator.Duplicate();
                    // call the operator
                    generator.CallStatic(Method);
                    // update value
                    binder.GenerateSet(operand, generator);
                    break;
                case ExpressionType.PrefixMinusMinus:
                case ExpressionType.PrefixPlusPlus:
                    if (binder == null)
                        throw new NullReferenceException(nameof(binder));
                    if ((binder.Attributes & BindingAttributes.HasThis) != 0)
                        generator.LoadArgument(0);
                    operand.GenerateCode(generator, MethodCompileOption.Dupplicate);
                    // call the operator
                    generator.CallStatic(Method);
                    if ((binder.Attributes & BindingAttributes.Member) != 0)
                    {
                        CallPreFixMember(generator, binder, option);
                        return;
                    }
                    if ((option & MethodCompileOption.Dupplicate) != 0)
                        generator.Duplicate();
                    // update value
                    binder.GenerateSet(Operand, generator);
                    break;
                default:
                    // call the operator
                    operand.GenerateCode(generator, AssignOption);
                    generator.CallStatic(Method);
                    break;
            }

        }

        void CallPreFixMember(MethodBodyGenerator generator, IBinder binder, MethodCompileOption option)
        {
            // if no duplicate ex: i++ single line 
            if ((option & MethodCompileOption.Dupplicate) == 0)
            {
                binder.GenerateSet(Operand, generator);
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
            binder.GenerateSet(Operand, generator);
            // load the temp variable
            generator.LoadVariable(temp);
        }

        void CallPostFixMember(MethodBodyGenerator generator, IBinder binder, MethodCompileOption option)
        {
            // if no duplicate ex: i++ single line 
            if ((option & MethodCompileOption.Dupplicate) == 0)
            {
                binder.GenerateSet(Operand, generator);
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
            binder.GenerateSet(Operand, generator);
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
