using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public sealed class ParsedProgram
    {
        public readonly Statement[] Statements;
        public readonly IOperationContext Context;

        public ParsedProgram(IOperationContext context, IEnumerable<Statement> statements)
        {
            Context = context;
            Statements = statements.ToArray();
        }

        public Object Evaluate(string function, params Object[] args)
        {
            var visitor = new NodeVisitor(Context);
            foreach (Statement statement in Statements.Where(statement => statement.NodeType == NodeType.Declaration || statement.NodeType == NodeType.Function))
            {
                //add all
                statement.Accept(visitor);
            }
            var reqFunction = Context.Functions[function];
            if (reqFunction == null)
                return Object.Null;
            return reqFunction.Having(args.Length, CodeScope.Class).Invoke(visitor, args.Select(arg => new ArgumentExpression(arg)).ToArray());
        }
    }
}
