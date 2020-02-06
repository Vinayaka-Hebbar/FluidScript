namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// If condition syntax 
    /// </summary>
    public sealed class IfStatement : Statement
    {
        /// <summary>
        /// Condition
        /// </summary>
        public readonly Expression Condition;
        /// <summary>
        /// True
        /// </summary>
        public readonly Statement Then;
        /// <summary>
        /// False
        /// </summary>
        public readonly Statement Else;

        /// <summary>
        /// Initializes new <see cref="IfStatement"/>
        /// </summary>
        public IfStatement(Expression condition, Statement then, Statement other) : base(StatementType.If)
        {
            Condition = condition;
            Then = then;
            Else = other;
        }

        public override System.Collections.Generic.IEnumerable<Node> ChildNodes()
        {
            yield return Condition;
            yield return Then;
            if (Else != null)
                yield return Else;
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitIf(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, statementLocals);
            var condition = Condition.Accept(generator);
            // Generate code for condition convert to System.Boolean
            condition.GenerateCode(generator);
            if (condition.Type == typeof(Boolean))
                generator.CallStatic(Utils.Helpers.Booolean_To_Bool);
            // We will need a label at the end of the if statement.
            var endOfEverything = generator.CreateLabel();
            if (Else == null)
            {
                //jump to end of if clause
                generator.BranchIfFalse(endOfEverything);
                //generate code for then clause
                Then.GenerateCode(generator);
            }
            else
            {
                //branch to else clause if false
                var startOfElseClause = generator.CreateLabel();
                generator.BranchIfFalse(startOfElseClause);

                //generate code for then clause
                Then.GenerateCode(generator);

                //brach to end of everything
                generator.Branch(endOfEverything);

                //generate code of else clause
                generator.DefineLabelPosition(startOfElseClause);
                Else.GenerateCode(generator);
            }

            //define label at end of statement
            generator.DefineLabelPosition(endOfEverything);
            //generate code for end of statement
            GenerateEndOfStatement(generator, statementLocals);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var elseCondition = Else == null ? string.Empty : string.Concat("else ", Else);
            return string.Concat("if (", Condition, ")\n", Then, '\n', elseCondition);
        }
    }
}
