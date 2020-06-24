using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Return statement
    /// </summary>
    public sealed class ReturnOrThrowStatement : Statement
    {
        const MethodGenerateOption GenerateOption = MethodGenerateOption.Return | MethodGenerateOption.Dupplicate;

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
                    var exp = Expression.Accept(generator);
                    exp.GenerateCode(generator, GenerateOption);
                    if (generator.SyntaxTree is BlockStatement block)
                    {
                        if (block.Statements.Count > 0)
                        {
                            lastStatement = block.Statements[block.Statements.Count - 1] == this;
                        }
                    }
                    var dest = generator.Method.ReturnType;
                    if (dest is null)
                        throw new System.NullReferenceException(nameof(System.Reflection.MethodInfo.ReturnType));
                    // void type no return
                    if (dest != TypeProvider.VoidType)
                    {

                        // todo variable name not used
                        if (generator.ReturnVariable == null)
                            generator.ReturnVariable = generator.DeclareVariable(dest);
                        System.Type src = exp.Type;
                        if (!dest.IsAssignableFrom(src))
                        {
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
                    else if (Expression.NodeType == ExpressionType.Invocation && Expression.Type != TypeProvider.VoidType)
                    {
                        // discard the return result
                        generator.Pop();
                    }
                }
                //last statement is not a return
                if (!lastStatement)
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
                return string.Concat("return ", Expression, ';');
            }
            return string.Concat("throw ", Expression, ';');
        }
    }
}
