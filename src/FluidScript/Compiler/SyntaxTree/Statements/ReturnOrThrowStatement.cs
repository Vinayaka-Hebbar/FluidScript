using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Return statement
    /// </summary>
    public sealed class ReturnOrThrowStatement : Statement
    {
        /// <summary>
        /// return expression
        /// </summary>
        public readonly Expression Expression;

        /// <summary>
        /// Initializes new <see cref="ReturnOrThrowStatement"/>
        /// </summary>
        public ReturnOrThrowStatement(Expression expression, StatementType nodeType) : base(nodeType)
        {
            Expression = expression;
        }

        /// <inheritdoc/>
        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitReturn(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(MethodBodyGenerator generator)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, statementLocals);
            if (NodeType == StatementType.Return)
            {
                bool lastStatement = true;
                if (Expression != null)
                {
                    var epression = Expression.Accept(generator);
                    epression.GenerateCode(generator);
                    if (generator.SyntaxTree is BlockStatement block)
                    {
                        if (block.Statements.Length > 0)
                        {
                            lastStatement = block.Statements[block.Statements.Length - 1] == this;
                        }
                    }
                    var dest = generator.Method.ReturnType;
                    if (dest == null)
                        throw new System.NullReferenceException(nameof(System.Reflection.MethodInfo.ReturnType));
                    //todo variable name not used
                    if (generator.ReturnVariable == null)
                        generator.ReturnVariable = generator.DeclareVariable(dest);
                    System.Type src = epression.Type;
                    if (!dest.IsAssignableFrom(src))
                    {
                        //todo box value type
                        if (Utils.TypeUtils.TryImplicitConvert(src, dest, out System.Reflection.MethodInfo method))
                        {
                            generator.Call(method);
                            src = method.ReturnType;
                        }
                        else
                        {
                            throw new System.Exception(string.Concat("can't cast ", src, " to ", dest));
                        }
                    }
                    if (src.IsValueType && dest.IsValueType == false)
                    {
                        generator.Box(src);
                    }
                    generator.StoreVariable(generator.ReturnVariable);

                    if (generator.ReturnTarget == null)
                    {
                        generator.ReturnTarget = generator.CreateLabel();
                    }
                }
                //last statement is not a return
                if (lastStatement == false)
                {
                    //if iniside try finally block 
                    generator.EmitLongJump(generator, generator.ReturnTarget);
                }
            }
            GenerateEndOfStatement(generator, statementLocals);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (NodeType == StatementType.Return)
            {
                return string.Concat("return ", Expression.ToString());
            }
            return string.Concat("throw ", Expression.ToString());
        }
    }
}
