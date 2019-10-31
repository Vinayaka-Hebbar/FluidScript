using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public interface INodeVisitor<TReturn> where TReturn : IRuntimeObject
    {
        TReturn VisitBinaryOperation(BinaryOperationExpression expression);
        TReturn VisitBlock(BlockExpression expression);
        TReturn VisitBlock(BlockStatement block);
        TReturn VisitFunction(IFunctionExpression function, Object[] args);
        TReturn VisitInitializer(InitializerExpression initializerExpression);
        TReturn VisitReturnOrThrow(ReturnOrThrowStatement returnOrThrowStatement);
        TReturn VisitVoid();
        TReturn VisitIfElse(IfStatement statement);
        TReturn VisitInvocation(InvocationExpression expression);
        TReturn VisitIdentifier(NameExpression identifierExpression);
        TReturn VisitVarDeclaration(VariableDeclarationExpression variableDeclarationExpression);
        TReturn VisitExpressions(ArrayExpression arrayExpression);
        TReturn VisitNullPropagator(NullPropegatorExpression expression);
        TReturn VisitQualifiedExpression(QualifiedExpression expression, Expression[] args);
        TReturn VisitQualifiedExpression(QualifiedExpression expression);
        TReturn VisitUnaryOperator(UnaryOperatorExpression unary);
        TReturn VisitVarDefination(VariableDeclarationStatement expression);
        TReturn VisitValueAccess(ValueAccessExpression expression);
        TReturn VisitLiteral(LiteralExpression expression);
        TReturn VisitAnonymousFuntion(AnonymousFunctionExpression anonymousFunctionExpression);
        TReturn VisitFunctionDefinition(FunctionDefinitionStatement functionDefinitionStatement);
    }
}