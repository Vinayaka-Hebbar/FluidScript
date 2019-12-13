using FluidScript.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Block statement {}
    /// </summary>
    public sealed class BlockStatement : Statement
    {
        /// <summary>
        /// List of statements
        /// </summary>
        public readonly Statement[] Statements;

        /// <summary>
        /// Initializes new <see cref="BlockStatement"/>
        /// </summary>
        public BlockStatement(Statement[] statements, string[] labels) : base(labels, StatementType.Block)
        {
            Statements = statements;
        }

        ///<inheritdoc/>
        public override IEnumerable<Node> ChildNodes() => Statements;

        ///<inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitBlock(this);
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public override string ToString()
        {
            return string.Concat("{", string.Join(";", Statements.Select(statement => statement.ToString())), "}");
        }
    }
}
