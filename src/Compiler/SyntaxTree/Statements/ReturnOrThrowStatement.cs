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

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitReturnOrThrow(this);
        }

        public override void GenerateCode(ILGenerator generator, OptimizationInfo info)
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
                        if (block.Statements.Count > 0)
                        {
                            lastStatement = block.Statements[block.Statements.Count - 1] == this;
                        }
                    }
                    //todo variable name not used
                    if (info.ReturnVariable == null)
                        info.ReturnVariable = generator.DeclareVariable(info.ReturnType);
                    if (info.ReturnType != null && info.ReturnType != Expression.Type)
                    {
                        if (info.ReturnType.IsPrimitive)
                            TypeUtils.ConvertToPrimitive(generator, info.ReturnType);
                        if(info.ReturnType == typeof(object) && Expression.Type.IsPrimitive)
                        {
                            //box
                            generator.Box(Expression.Type);
                        }
                    }
                    generator.StoreVariable(info.ReturnVariable);
                }
                //last statement is not a return
                if (lastStatement == false)
                {
                    if (info.ReturnTarget == null)
                        info.ReturnTarget = generator.CreateLabel();
                    //if iniside try finally block 
                    info.EmitLongJump(generator, info.ReturnTarget);
                }
            }
            GenerateEndOfStatement(generator, info, statementLocals);
        }
    }
}
