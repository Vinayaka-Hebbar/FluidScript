using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BodyStatement : Statement
    {
        public readonly Statement[] Statements;
        public BodyStatement(Statement[] statements, string[] labels) : base(labels, StatementType.Block)
        {
            Statements = statements;
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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            foreach (var statement in Statements)
            {
                StatementType nodeType = statement.NodeType;
                var value = statement.Evaluate(instance);
                if (nodeType == StatementType.Return)
                {
                    return value;
                }
                if (nodeType != StatementType.Expression)
                {
                    if (value is object)
                    {
                        return value;
                    }
                }
            }
            return null;
        }
#endif

        public override string ToString()
        {
            return string.Concat("{", string.Join(";", Statements.Select(statement => statement.ToString())), "}");
        }
    }
}
