using System.Collections.Generic;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockStatement : Statement
    {
        //Todo Linq
        public readonly IList<Statement> Statements;
        public BlockStatement(Statement[] statements, string[] labels) : base(labels, StatementType.Block)
        {
            Statements = new List<Statement>(statements);
        }

        public override IEnumerable<Node> ChildNodes => Statements;

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            var statementLocals = new StatementLocals() { NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, info, statementLocals);
            foreach (var statement in Statements)
            {
                statement.GenerateCode(generator, info);
            }
            GenerateEndOfStatement(generator, info, statementLocals);
        }
    }
}
