using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public interface IExpressionVisitor<out TResult>
    {
        TResult VisitUnary(UnaryExpression node);
        TResult VisitBinary(BinaryExpression node);
        TResult VisitArrayLiteral(ArrayLiteralExpression node);
        TResult VisitAssignment(AssignmentExpression node);
        TResult VisitMember(MemberExpression node);
        TResult VisitMember(NameExpression node);
        TResult VisitCall(InvocationExpression node);
        TResult VisitThis(ThisExpression node);
        TResult VisitDeclaration(VariableDeclarationExpression node);
        TResult VisitLiteral(LiteralExpression node);
        TResult VisitTernary(TernaryExpression node);
        TResult VisitIndex(IndexExpression node);
    }
}
