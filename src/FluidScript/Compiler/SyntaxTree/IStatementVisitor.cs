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
        /// Visit block statement
        /// </summary>
        void VisitBlock(BlockStatement node);
        /// <summary>
        /// Visit Import statement
        /// </summary>
        void VisitImport(ImportStatement node);
        /// <summary>
        /// Visit variable declaration
        /// </summary>
        void VisitDeclaration(LocalDeclarationStatement node);
        /// <summary>
        /// Visit Continue statement
        /// </summary>
        void VisitContinue(ContinueStatement node);
        /// <summary>
        /// Visit break statement
        /// </summary>
        /// <param name="node"></param>
        void VisitBreak(BreakStatement node);
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
