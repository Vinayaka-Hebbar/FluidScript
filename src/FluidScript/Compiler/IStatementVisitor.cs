using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Statement visitor
    /// </summary>
    public interface IStatementVisitor
    {
        /// <summary>
        /// Expression statemenr
        /// </summary>
        void VisitExpression(ExpressionStatement node);
        /// <summary>
        /// Visit return statement
        /// </summary>
        void VisitReturn(ReturnOrThrowStatement node);
        /// <summary>
        /// Visit block
        /// </summary>
        void VisitBlock(BlockStatement node);
        /// <summary>
        /// Visit variable declaration
        /// </summary>
        void VisitDeclaration(LocalDeclarationStatement node);

        /// <summary>
        /// Visit loop
        /// </summary>
        void VisitLoop(LoopStatement node);
        /// <summary>
        /// If Condition
        /// </summary>
        void VisitIf(IfStatement node);
    }
}
