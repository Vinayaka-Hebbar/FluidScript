using FluidScript.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BlockStatement : Statement
    {
        //Todo Linq
        public readonly Statement[] Statements;

        public BlockStatement(Statement[] statements, string[] labels) : base(labels, StatementType.Block)
        {
            Statements = statements;
        }

        public override IEnumerable<Node> ChildNodes() => Statements;

        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlock(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var statementLocals = new StatementLocals() { NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, statementLocals);
            foreach (var statement in Statements)
            {
                statement.GenerateCode(generator);
            }
            GenerateEndOfStatement(generator, statementLocals);
        }

        public override string ToString()
        {
            return string.Concat("{", string.Join(";", Statements.Select(statement => statement.ToString())), "}");
        }
    }
}
