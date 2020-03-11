namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Loop Statement
    /// </summary>
    public class LoopStatement : Statement
    {
        public readonly Statement Body;

        /// <summary>
        /// Initializes a <see cref="LoopStatement"/>
        /// </summary>
        public LoopStatement(Statement body, StatementType type) : base(type)
        {
            Body = body;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public Statement Initialization
        {
            get;
            set;
        }

        /// <summary>
        /// Condition statement
        /// </summary>
        public Expression Condition { get; set; }

        /// <summary>
        /// Increment or decrement operation
        /// </summary>
        public NodeList<Expression> Increments
        {
            get;
            set;
        }

        public bool CheckConditionAtEnd => NodeType == StatementType.DoWhile;

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitLoop(this);
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals() { NonDefaultBreakStatementBehavior = true, NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, statementLocals);
            // Set up some labels.
            var continueTarget = generator.CreateLabel();
            var breakTarget = generator.CreateLabel();

            // Emit the initialization statement.
            if (Initialization != null)
                Initialization.GenerateCode(generator);

            // The inner loop starts here.
            var startOfLoop = generator.CreateLabel();
            var startOfCondition = generator.CreateLabel();
            if (CheckConditionAtEnd == false)
                generator.Branch(startOfCondition);
            //start of loop

            generator.DefineLabelPosition(startOfLoop);
            // Emit the loop body.
            generator.PushBreakOrContinueInfo(Labels, breakTarget, continueTarget, false);
            Body.GenerateCode(generator);
            generator.PopBreakOrContinueInfo();

            // The continue statement jumps here.
            generator.DefineLabelPosition(continueTarget);

            // Increment the loop variable.
            if (Increments != null)
                Increments.ForEach(e => e.GenerateCode(generator));

            generator.DefineLabelPosition(startOfCondition);
            // Check the condition and jump to the end if it is false.
            if (Condition != null)
            {
                // generator.MarkSequencePoint(ConditionStatement.Span);
                Condition.GenerateCode(generator);
                generator.CallStatic(Utils.ReflectionHelpers.BoooleanToBool);
                generator.BranchIfTrue(startOfLoop);
            }

            // Define the end of the loop (actually just after).
            generator.DefineLabelPosition(breakTarget);

            // Generate code for the end of the statement.
            GenerateEndOfStatement(generator, statementLocals);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            if (NodeType == StatementType.For)
            {
                return string.Concat("for(", Initialization, ";", Condition, ";", string.Join(",", Increments.Map(e => e.ToString())), ")\n{", Body, "\n}\n");
            }
            else if (NodeType == StatementType.While)
            {
                return string.Concat("while(", Condition, ")\n{", Body, "\n}\n");
            }
            else if (NodeType == StatementType.DoWhile)
            {
                return string.Concat("do\n{", Body, "\n}\nwhile (", Condition, ")");
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
