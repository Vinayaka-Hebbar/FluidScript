using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public sealed class CompiledProgram
    {
        public readonly Statement[] Statements;
        public readonly Scopes.Scope Context;

        public CompiledProgram(Scopes.Scope context, IEnumerable<Statement> statements)
        {
            Context = context;
            Statements = statements.ToArray();
        }

        //public Object Evaluate(string function, params Object[] args)
        //{
        //    var visitor = new NodeVisitor(Context);
        //    foreach (Statement statement in Statements.Where(statement => statement.NodeType == StatementType.Declaration || statement.NodeType == ExpressionType.Function))
        //    {
        //        //add all
        //        statement.Accept(visitor);
        //    }
        //    var reqFunction = Context.Functions[function];
        //    if (reqFunction == null)
        //        return Object.Null;
        //    return reqFunction.Having(args.Length, CodeScope.Class).Invoke(visitor, args.Select(arg => new ArgumentExpression(arg)).ToArray());
        //}
    }
}
