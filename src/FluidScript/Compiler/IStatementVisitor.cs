using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public interface IStatementVisitor
    {
        void VisitExpression(ExpressionStatement node);
        void VisitReturn(ReturnOrThrowStatement node);
        void VisitBlock(BlockStatement node);
    }
}
