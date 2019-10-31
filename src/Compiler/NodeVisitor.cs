using FluidScript.Compiler.SyntaxTree;
using System;
using System.Linq;

namespace FluidScript.Compiler
{
    public class NodeVisitor : INodeVisitor<Object>
    {
        public readonly Scopes.Scope Context;
        public readonly NodeVisitor Root;

        public NodeVisitor(Scopes.Scope scope)
        {
            Context = scope;
            Root = this;
        }

        public Object VisitUnaryOperator(UnaryOperatorExpression unary)
        {
            var opCode = unary.NodeType;
            var operand = unary.Operand;
            if (opCode == ExpressionType.Parenthesized)
                return operand.Accept(this);
            var result = Object.Zero;
            if (operand is ValueAccessExpression expression)
            {
                result = operand.Accept(this);
                Object value = result;
                switch (opCode)
                {
                    case ExpressionType.PostfixPlusPlus:
                        value = value + 1;
                        break;
                    case ExpressionType.PostfixMinusMinus:
                        value = value - 1;
                        break;
                    case ExpressionType.PrefixPlusPlus:
                        result = value = value + 1;
                        break;
                    case ExpressionType.PrefixMinusMinus:
                        result = value = value - 1;
                        break;
                    case ExpressionType.Bang:
                        result = !value;
                        break;
                }
                Context.G[expression.Name] = value;
                return result;
            }
            switch (opCode)
            {
                case ExpressionType.Bang:
                    return !operand.Accept(this);
                case ExpressionType.Plus:
                    return +operand.Accept(this);
                case ExpressionType.Minus:
                    return -operand.Accept(this);
                case ExpressionType.Out:
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
                if (statement.NodeType == StatementType.Return)
                    return statement.Accept(this);
                var result = statement.Accept(this);
                if ((result.Type & ObjectType.Any) == ObjectType.Any)
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
            if (statement.Accept(this).ToBool())
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
            if (target.NodeType == ExpressionType.Identifier)
            {
                if (target is IdentifierExpression identifier)
                {
                    var func = identifier.GetFunction(Context);
                    return func.Having(args.Length, CodeScope.Local).Invoke(this, args);
                }
            }
            return target.Accept(this);
        }

        public Object VisitQualifiedExpression(QualifiedExpression expression, Expression[] args)
        {
            var root = Root;
            var target = expression.Target;
            if (target.NodeType == ExpressionType.This)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Identifier:
                        return root.Context.GetFunctionPart(expression.Identifier.Id, args.Length, CodeScope.Class).Invoke(this, args);
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
                if (statement.NodeType == ExpressionType.Return)
                {
                    return statement.Accept(visitor);
                }
                var result = statement.Accept(visitor);
                if (result.Type != ObjectType.Any)
                    return result;
            }
            return Object.Void;
        }

        public Object VisitNullPropagator(NullPropegatorExpression expression)
        {
            var left = expression.Left;
            var right = expression.Right;
            if (left.NodeType == ExpressionType.Variable || left.NodeType == ExpressionType.Constant)
            {
                var identifier = (ValueAccessExpression)left;
                var value = identifier.Accept(this);
                if (!value.IsNull)
                    return value;
                var result = right.Accept(this);
                switch (left.NodeType)
                {
                    case ExpressionType.Variable:
                        Context.Variables[identifier.Name] = result;
                        break;
                    case ExpressionType.Constant:
                        Context.Constants.Add(identifier.Name, result);
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
            var outExpression = arguments.FirstOrDefault(arg => arg.NodeType == ExpressionType.Out);

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
            switch (expression.NodeType)
            {
                case ExpressionType.Plus:
                    return left.Accept(this) + right.Accept(this);
                case ExpressionType.Minus:
                    return left.Accept(this) - right.Accept(this);
                case ExpressionType.Multiply:
                    return left.Accept(this) * right.Accept(this);
                case ExpressionType.Divide:
                    return left.Accept(this) / right.Accept(this);
                case ExpressionType.Percent:
                    return left.Accept(this) % right.Accept(this);
                case ExpressionType.Circumflex:
                    return left.Accept(this) ^ right.Accept(this);
                case ExpressionType.EqualEqual:
                    return left.Accept(this) == right.Accept(this);
                case ExpressionType.BangEqual:
                    return left.Accept(this) != right.Accept(this);
                case ExpressionType.Less:
                    return left.Accept(this) < right.Accept(this);
                case ExpressionType.LessEqual:
                    return left.Accept(this) <= right.Accept(this);
                case ExpressionType.LessLess:
                    return left.Accept(this) << right.Accept(this).ToInt32();
                case ExpressionType.Greater:
                    return left.Accept(this) > right.Accept(this);
                case ExpressionType.GreaterEqual:
                    return left.Accept(this) >= right.Accept(this);
                case ExpressionType.GreaterGreater:
                    return left.Accept(this) >> (int)right.Accept(this);
                case ExpressionType.And:
                    return left.Accept(this) & right.Accept(this);
                case ExpressionType.AndAnd:
                    return new Object(left.Accept(this).ToBool() && right.Accept(this).ToBool());
                case ExpressionType.Or:
                    return left.Accept(this) | right.Accept(this);
                case ExpressionType.OrOr:
                    return new Object(left.Accept(this).ToBool() || right.Accept(this).ToBool());
                case ExpressionType.Equal:
                    if (left is IdentifierExpression identifier)
                    {
                        var value = right.Accept(this);
                        Context[identifier.Id] = value;
                        return value;
                    }
                    return Object.Zero;
                case ExpressionType.Comma:
                    left.Accept(this);
                    return right.Accept(this);
                default:
                    return Object.Zero;
            }
        }

        public Object VisitValueAccess(ValueAccessExpression expression)
        {
            var context = Context;
            var id = expression.Name;
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
            var id = declaration.Name;
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
            if (Target.NodeType == ExpressionType.This)
            {
                IdentifierExpression identifier = expression.Identifier;
                switch (identifier.NodeType)
                {
                    case ExpressionType.Variable:
                        return root.Context[identifier.Id];
                }
            }
            return Object.Void;
        }

        public Object VisitInitializer(InitializerExpression initializerExpression)
        {
            var value = Object.Void;
            var id = initializerExpression.Name;
            var Target = initializerExpression.Target;
            switch (Target.NodeType)
            {
                case ExpressionType.Function:
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
            if (statement.NodeType == ExpressionType.Return)
            {
                return new Object(expression.Accept(this), ObjectType.Any);
            }
            throw new Exception(expression.Accept(this).ToString());
        }
    }
}
