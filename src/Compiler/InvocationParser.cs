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
            var context = EvaluateMethod(new NodeVisitor(expression.Context), exp.Expression, NodeType.Unknown);
            if (context.CanInvoke)
            {
                return null;
            }
            return Object.Null;
        }

        private IInvocationContext EvaluateMethod(NodeVisitor visitor, Expression target, NodeType parentKind)
        {
            NodeType opCode = target.NodeType;
            switch (opCode)
            {
                case NodeType.PropertyAccess:
                    var exp = (QualifiedExpression)target;
                    var left = EvaluateMethod(visitor, exp.Target, opCode);
                    return new PropertyInvocation(exp.Identifier.Id, left, exp.Identifier.NodeType, parentKind);
                case NodeType.QualifiedNamespace:
                case NodeType.Identifier:
                    return new TypeNameContext(target.ToString());
                case NodeType.Invocation:
                case NodeType.New:
                    var invocation = (InvocationExpression)target;
                    var args = invocation.Arguments.Select(arg => arg.Accept(visitor)).ToArray();
                    var value = EvaluateMethod(visitor, invocation.Target, opCode);
                    return new MethodInvocation(value, args);

            }
            return null;
        }

    }
}
