using FluidScript.Core;
using FluidScript.SyntaxTree.Expressions;
using System.Linq;

namespace FluidScript
{
    public class InvocationParser
    {
        public Object Parse(string text)
        {
            SyntaxParser expression = new SyntaxParser();
            var exp = expression.ParseExpression(text);
            var context = EvaluateMethod(new NodeVisitor(expression.Context), exp.Expression, Expression.Operation.Unknown);
            if (context.CanInvoke)
            {
                return null;
            }
            return Object.Null;
        }

        private IInvocationContext EvaluateMethod(NodeVisitor visitor, IExpression target, Expression.Operation parentKind)
        {
            Expression.Operation opCode = target.Kind;
            switch (opCode)
            {
                case Expression.Operation.PropertyAccess:
                    var exp = (QualifiedExpression)target;
                    var left = EvaluateMethod(visitor, exp.Target, opCode);
                    return new PropertyInvocation(exp.Identifier.Id, left, exp.Identifier.OpCode, parentKind);
                case Expression.Operation.QualifiedNamespace:
                case Expression.Operation.Identifier:
                    return new TypeNameContext(target.ToString());
                case Expression.Operation.Invocation:
                case Expression.Operation.New:
                    var invocation = (InvocationExpression)target;
                    var args = invocation.Arguments.Select(arg => arg.Accept(visitor)).ToArray();
                    var value = EvaluateMethod(visitor, invocation.Target, opCode);
                    return new MethodInvocation(value, args);

            }
            return null;
        }

    }
}
