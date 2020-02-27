using FluidScript.Compiler.Emit;
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
        public readonly NodeList<Statement> Statements;

        /// <summary>
        /// Initializes new <see cref="BlockStatement"/>
        /// </summary>
        public BlockStatement(NodeList<Statement> statements, string[] labels) : base(labels, StatementType.Block)
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
            if (Statements.Length == 0)
                generator.NoOperation();
            for (int index = 0; index < Statements.Length; index++)
            {
                Statements[index].GenerateCode(generator);
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
