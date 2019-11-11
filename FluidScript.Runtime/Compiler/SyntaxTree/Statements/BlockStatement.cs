using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public BlockStatement(params Statement[] statements) : base(StatementType.Block)
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

        internal Func<RuntimeObject[], RuntimeObject> Invoke()
        {
            throw new NotImplementedException();
        }

        public override RuntimeObject Evaluate()
        {
            var result = RuntimeObject.Null;
            foreach (var statement in Statements)
            {
                if (statement.NodeType == StatementType.Return)
                {
                    result = statement.Evaluate();
                    break;
                }
                result = statement.Evaluate();
                if (result.IsReturn)
                {
                    break;
                }
            }
            result.IsReturn = true;
            return result;
        }

        public override string ToString()
        {
            return string.Concat("{", string.Join(";", Statements.Select(statement=> statement.ToString())), "}");
        }
    }
}
