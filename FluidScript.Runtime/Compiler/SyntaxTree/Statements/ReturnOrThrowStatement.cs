using FluidScript.Compiler.Emit;

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
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            if (NodeType == StatementType.Return)
            {
                return Expression?.Evaluate(instance);
            }
            if (NodeType == StatementType.Throw)
            {
                throw new System.Exception(Expression.Evaluate(instance).ToString());
            }
            return base.Evaluate(instance);
        }
#endif

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            // Generate code for the start of the statement.
            var statementLocals = new StatementLocals();
            GenerateStartOfStatement(generator, info, statementLocals);
            if (NodeType == StatementType.Return)
            {
                bool lastStatement = false;
                if (Expression != null)
                {
                    Expression.GenerateCode(generator, info);
                    if (info.SyntaxTree is BlockStatement block)
                    {
                        if (block.Statements.Length > 0)
                        {
                            lastStatement = block.Statements[block.Statements.Length - 1] == this;
                        }
                    }
                    //todo variable name not used
                    if (info.ReturnVariable == null)
                        info.ReturnVariable = generator.DeclareVariable(info.ReturnType);
                    if (info.ReturnType != null && info.ReturnType != Expression.ResultType(info))
                    {
                        if (info.ReturnType.IsPrimitive)
                            TypeUtils.ConvertToPrimitive(generator, info.ReturnType);
                        if (info.ReturnType == typeof(object) && Expression.ResultType(info).IsPrimitive)
                        {
                            //box
                            generator.Box(Expression.ResultType(info));
                        }
                    }
                    generator.StoreVariable(info.ReturnVariable);
                }
                if (info.ReturnTarget == null)
                    info.ReturnTarget = generator.CreateLabel();
                //last statement is not a return
                if (lastStatement == false)
                {
                    //if iniside try finally block 
                    info.EmitLongJump(generator, info.ReturnTarget);
                }
            }
            GenerateEndOfStatement(generator, info, statementLocals);
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
