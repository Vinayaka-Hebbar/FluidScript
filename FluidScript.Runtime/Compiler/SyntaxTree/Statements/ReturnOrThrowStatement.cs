using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ReturnOrThrowStatement : Statement
    {
        public readonly Expression Expression;
        public ReturnOrThrowStatement(Expression expression, StatementType nodeType) : base(nodeType)
        {
            Expression = expression;
        }

#if Runtime
        internal override RuntimeObject Evaluate(RuntimeObject instance, Metadata.Prototype prototype)
        {
            if (NodeType == StatementType.Return)
            {
                if (Expression == null)
                    return RuntimeObject.Void;
                return Expression.Evaluate(instance);
            }
            if (NodeType == StatementType.Throw)
            {
                throw new System.Exception(Expression.Evaluate(instance).ToString());
            }
            return base.Evaluate(instance);
        }
#endif

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
                    var returnType = generator.MethodGenerator.ReturnType;
                    //todo variable name not used
                    if (generator.ReturnVariable == null)
                        generator.ReturnVariable = generator.DeclareVariable(returnType);
                    System.Type resolvedType = epression.Type;
                    if (returnType != null && returnType != resolvedType)
                    {
                        if(TypeUtils.TryImplicitConvert(resolvedType, returnType, out System.Reflection.MethodInfo method))
                        {
                            generator.Call(method);
                        }
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
