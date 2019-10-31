using FluidScript.Core;
using FluidScript.Compiler.SyntaxTree;
using System.Linq;

namespace FluidScript.Compiler
{
    public sealed class InvocationParser
    {
        public Object Parse(string text)
        {
            ScriptEngine expression = new ScriptEngine();
            var exp = expression.ParseExpression(text);
            var context = EvaluateMethod(new NodeVisitor(expression.Context), exp.Statement, ExpressionType.Unknown);
            if (context.CanInvoke)
            {
                return null;
            }
            return Object.Null;
        }

        private IInvocationContext EvaluateMethod(INodeVisitor<Object> visitor, Expression target, ExpressionType parentKind)
        {
            ExpressionType opCode = target.NodeType;
            switch (opCode)
            {
                case ExpressionType.PropertyAccess:
                    var exp = (QualifiedExpression)target;
                    var left = EvaluateMethod(visitor, exp.Target, opCode);
                    return new PropertyInvocation(exp.Identifier.Id, left, exp.Identifier.NodeType, parentKind);
                case ExpressionType.QualifiedNamespace:
                case ExpressionType.Identifier:
                    return new TypeNameContext(target.ToString());
                case ExpressionType.Invocation:
                case ExpressionType.New:
                    var invocation = (InvocationExpression)target;
                    var args = invocation.Arguments.Select(arg => arg.Accept(visitor)).ToArray();
                    var value = EvaluateMethod(visitor, invocation.Target, opCode);
                    return new MethodInvocation(value, args);

            }
            return null;
        }

    }
}
