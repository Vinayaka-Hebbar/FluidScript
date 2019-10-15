using FluidScript.SyntaxTree.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.SyntaxTree
{
    public class ParsedProgram
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
            foreach (Statement statement in Statements.Where(statement => statement.OpCode == Statement.Operation.Declaration || statement.OpCode == Statement.Operation.Function))
            {
                //add all
                statement.Accept(visitor);
            }
            var reqFunction = Context.Functions[function];
            if (reqFunction == null)
                return Object.Null;
            return reqFunction.Having(args.Length, Scope.Program).Invoke(visitor, args.Select(arg => (IExpression)new ArgumentExpression(arg)).ToArray());
        }
    }
}
