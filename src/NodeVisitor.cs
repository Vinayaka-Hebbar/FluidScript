using FluidScript.SyntaxTree.Expressions;
using FluidScript.SyntaxTree.Statements;
using System;
using System.Linq;

namespace FluidScript
{
    public class NodeVisitor : INodeVisitor<Object>
    {
        public readonly IOperationContext Context;
        public readonly NodeVisitor Root;

        public NodeVisitor(IOperationContext context)
        {
            Context = context;
            Root = this;
        }

        public NodeVisitor(NodeVisitor other)
        {
            Context = new OperationContextWrapper(other.Context);
            Root = other.Root;
        }

        public Object VisitUnaryOperator(UnaryOperatorExpression unary)
        {
            var opCode = unary.OpCode;
            var operand = unary.Operand;
            if (opCode == Expression.Operation.Parenthesized)
                return operand.Accept(this);
            var result = Object.Zero;
            if (operand is ValueAccessExpression expression)
            {
                result = operand.Accept(this);
                Object value = result;
                switch (opCode)
                {
                    case Expression.Operation.PostfixPlusPlus:
                        value = value + 1;
                        break;
                    case Expression.Operation.PostfixMinusMinus:
                        value = value - 1;
                        break;
                    case Expression.Operation.PrefixPlusPlus:
                        result = value = value + 1;
                        break;
                    case Expression.Operation.PrefixMinusMinus:
                        result = value = value - 1;
                        break;
                    case Expression.Operation.Bang:
                        result = !value;
                        break;
                }

                Context.Variables[expression.Id] = value;
                return result;
            }
            switch (opCode)
            {
                case Expression.Operation.Bang:
                    return !operand.Accept(this);
                case Expression.Operation.Plus:
                    return +operand.Accept(this);
                case Expression.Operation.Minus:
                    return -operand.Accept(this);
                case Expression.Operation.Out:
                    return operand.Accept(this);
            }
            return result;
        }

        public Object VisitLiteral(LiteralExpression literalExpression)
        {
            return literalExpression.Value;
        }

        //Function Block
        public Object VisitBlock(BlockStatement block)
        {
            foreach (var statement in block.Statements)
            {
                if (statement.OpCode == Statement.Operation.Return)
                    return statement.Accept(this);
                var result = statement.Accept(this);
                if ((result.Type & ObjectType.Void) == ObjectType.Void)
                {
                    return result;
                }
            }
            return Object.Void;
        }

        public Object VisitVarDefination(VariableDeclarationStatement expression)
        {
            var delcarations = expression.DeclarationExpressions;
            foreach (var declaration in delcarations)
            {
                declaration.Accept(this);
            }
            return Object.Void;
        }

        public Object VisitIfElse(IfStatement statement)
        {
            if (statement.Expression.Accept(this).ToBool())
            {
                return statement.Body.Accept(this);
            }
            else
            {
                return statement.Other != null ? statement.Other.Accept(this) : Object.Void;
            }
        }

        public Object VisitInvocation(InvocationExpression expression)
        {
            var target = expression.Target;
            var args = expression.Arguments;
            if (target.Kind == Expression.Operation.Identifier)
            {
                if (target is IdentifierExpression identifier)
                {
                    var func = identifier.GetFunction(Context);
                    return func.Having(args.Length, Scope.Local).Invoke(this, args);
                }
            }
            return target.Accept(this);
        }

        public Object VisitQualifiedExpression(QualifiedExpression expression, IExpression[] args)
        {
            var root = Root;
            var target = expression.Target;
            if (target.Kind == Expression.Operation.This)
            {
                switch (expression.OpCode)
                {
                    case Expression.Operation.Identifier:
                        return root.Context.GetFunctionPart(expression.Identifier.Id, args.Length, Scope.Program).Invoke(this, args);
                }
            }
            return Object.Void;
        }

        public Object VisitBlock(BlockExpression expression)
        {
            var visitor = new NodeVisitor(this);
            var statements = expression.Statements;
            foreach (var statement in statements)
            {
                if (statement.OpCode == Statement.Operation.Return)
                {
                    return statement.Accept(visitor);
                }
                var result = statement.Accept(visitor);
                if (result.Type != ObjectType.Void)
                    return result;
            }
            return Object.Void;
        }

        public Object VisitNullPropagator(NullPropegatorExpression expression)
        {
            var left = expression.Left;
            var right = expression.Right;
            if (left.Kind == Expression.Operation.Variable || left.Kind == Expression.Operation.Constant)
            {
                var identifier = (ValueAccessExpression)left;
                var value = identifier.Accept(this);
                if (!value.IsNull)
                    return value;
                var result = right.Accept(this);
                switch (left.Kind)
                {
                    case Expression.Operation.Variable:
                        Context.Variables[identifier.Id] = result;
                        break;
                    case Expression.Operation.Constant:
                        Context.Constants.Add(identifier.Id, result);
                        break;
                }
                return result;
            }
            return Object.Zero;
        }

        public Object VisitFunction(IFunctionExpression function, Object[] args)
        {
            var arguments = function.Arguments;
            var statement = function.Body;
            for (int i = 0; i < args.Length; i++)
            {
                Object arg = args[i];
                Context[arguments[i].ToString()] = arg;
            }
            var outExpression = arguments.FirstOrDefault(arg => arg.Kind == Expression.Operation.Out);

            if (outExpression != null)
            {
                Context[outExpression.ToString()] = Object.Empty;
                statement.Accept(this);
                return outExpression.Accept(this);
            }
            return statement.Accept(this);
        }

        public Object VisitBinaryOperation(BinaryOperationExpression expression)
        {
            var left = expression.Left;
            var right = expression.Right;
            switch (expression.OpCode)
            {
                case Expression.Operation.Plus:
                    return left.Accept(this) + right.Accept(this);
                case Expression.Operation.Minus:
                    return left.Accept(this) - right.Accept(this);
                case Expression.Operation.Multiply:
                    return left.Accept(this) * right.Accept(this);
                case Expression.Operation.Divide:
                    return left.Accept(this) / right.Accept(this);
                case Expression.Operation.Percent:
                    return left.Accept(this) % right.Accept(this);
                case Expression.Operation.Circumflex:
                    return left.Accept(this) ^ right.Accept(this);
                case Expression.Operation.EqualEqual:
                    return left.Accept(this) == right.Accept(this);
                case Expression.Operation.BangEqual:
                    return left.Accept(this) != right.Accept(this);
                case Expression.Operation.Less:
                    return left.Accept(this) < right.Accept(this);
                case Expression.Operation.LessEqual:
                    return left.Accept(this) <= right.Accept(this);
                case Expression.Operation.LessLess:
                    return left.Accept(this) << right.Accept(this).ToInt32();
                case Expression.Operation.Greater:
                    return left.Accept(this) > right.Accept(this);
                case Expression.Operation.GreaterEqual:
                    return left.Accept(this) >= right.Accept(this);
                case Expression.Operation.GreaterGreater:
                    return left.Accept(this) >> (int)right.Accept(this);
                case Expression.Operation.And:
                    return left.Accept(this) & right.Accept(this);
                case Expression.Operation.AndAnd:
                    return new Object(left.Accept(this).ToBool() && right.Accept(this).ToBool());
                case Expression.Operation.Or:
                    return left.Accept(this) | right.Accept(this);
                case Expression.Operation.OrOr:
                    return new Object(left.Accept(this).ToBool() || right.Accept(this).ToBool());
                case Expression.Operation.Equal:
                    if (left is IdentifierExpression identifier)
                    {
                        var value = right.Accept(this);
                        Context[identifier.Id] = value;
                        return value;
                    }
                    return Object.Zero;
                case Expression.Operation.Comma:
                    left.Accept(this);
                    return right.Accept(this);
                default:
                    return Object.Zero;
            }
        }

        public Object VisitValueAccess(ValueAccessExpression expression)
        {
            var context = Context;
            var id = expression.Id;
            if (expression.CanAccess(context, id))
                return expression.Access(context, id);
            return Object.NaN;
        }

        public Object VisitExpressions(ArrayExpression arrayExpression)
        {
            Object result = Object.Void;
            var expressions = arrayExpression.Expressions;
            foreach (Expression expression in expressions)
            {
                result = expression.Accept(this);
            }
            return result;
        }

        public Object VisitAnonymousFuntion(AnonymousFunctionExpression anonymousFunctionExpression)
        {
            throw new NotImplementedException();
        }

        public Object VisitVarDeclaration(VariableDeclarationExpression declaration)
        {
            var id = declaration.Id;
            Context[id] = Object.Null;
            return Object.Void;
        }

        public Object VisitVoid()
        {
            return Object.Void;
        }

        public Object VisitQualifiedExpression(QualifiedExpression expression)
        {
            var root = Root;
            var Target = expression.Target;
            if (Target.Kind == Expression.Operation.This)
            {
                IdentifierExpression identifier = expression.Identifier;
                switch (identifier.OpCode)
                {
                    case Expression.Operation.Variable:
                        return root.Context[identifier.Id];
                }
            }
            return Object.Void;
        }

        public Object VisitInitializer(InitializerExpression initializerExpression)
        {
            var value = Object.Void;
            var id = initializerExpression.Id;
            var Target = initializerExpression.Target;
            switch (Target.Kind)
            {
                case Expression.Operation.Function:
                    Context.Add(id, (Target as IFunctionExpression).GetPartBuilder());
                    break;
                default:
                    value = Target.Accept(this);
                    Context[id] = value;
                    break;

            }
            return value;
        }

        public Object VisitFunctionDefinition(FunctionDefinitionStatement definition)
        {
            Context.Add(definition.Name, definition.GetPartBuilder());
            return Object.Void;
        }

        public Object VisitIdentifier(IdentifierExpression identifierExpression)
        {
            throw new NotImplementedException();
        }

        public Object VisitArgument(ArgumentExpression argument)
        {
            return argument.Value;
        }

        public Object VisitReturnOrThrow(ReturnOrThrowStatement statement)
        {
            var expression = statement.Expression;
            if (statement.OpCode == Statement.Operation.Return)
            {
                return new Object(expression.Accept(this), ObjectType.Void);
            }
            throw new Exception(expression.Accept(this).ToString());
        }
    }
}
