﻿using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Expression Visitor
    /// </summary>
    /// <typeparam name="TResult">Return Type</typeparam>
    public interface IExpressionVisitor<out TResult>
    {
        /// <summary>
        /// Any Expressions
        /// </summary>
        TResult Default(Expression node);

        /// <summary>
        /// Unary Expressions
        /// </summary>
        TResult VisitUnary(UnaryExpression node);
        /// <summary>
        /// Instance of 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        TResult VisitInstanceOf(InstanceOfExpression node);

        /// <summary>
        /// Convert Expression &lt;int&gt;
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        TResult VisitConvert(ConvertExpression node);
        /// <summary>
        /// New Expression
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Resolved value</returns>
        TResult VisitNew(NewExpression node);

        /// <summary>
        /// Size of expression
        /// </summary>
        TResult VisitSizeOf(SizeOfExpression node);

        /// <summary>
        /// Binary Expression
        /// </summary>
        TResult VisitBinary(BinaryExpression node);

        /// <summary>
        /// AnonymousObject Expression
        /// </summary>
        TResult VisitAnonymousObject(AnonymousObjectExpression node);

        /// <summary>
        /// AnonymousFunction Expression
        /// </summary>
        TResult VisitAnonymousFunction(AnonymousFunctionExpression node);

        /// <summary>
        /// Array Literal
        /// </summary>
        TResult VisitArrayLiteral(ArrayListExpression node);
        /// <summary>
        /// Assignment Expression
        /// </summary>
        TResult VisitAssignment(AssignmentExpression node);
        /// <summary>
        /// Visit Member access
        /// </summary>
        TResult VisitMember(MemberExpression node);
        /// <summary>
        /// Visit Name (Identifier) expression
        /// </summary>
        TResult VisitMember(NameExpression node);
        /// <summary>
        /// Visit Method Call
        /// </summary>
        TResult VisitCall(InvocationExpression node);
        /// <summary>
        /// Visit this
        /// </summary>
        TResult VisitThis(ThisExpression node);
        /// <summary>
        /// Visit super
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Result</returns>
        TResult VisitSuper(SuperExpression node);
        /// <summary>
        /// Visit Declaration
        /// </summary>
        TResult VisitDeclaration(VariableDeclarationExpression node);
        /// <summary>
        /// Visit Literal
        /// </summary>
        TResult VisitLiteral(LiteralExpression node);
        /// <summary>
        /// Visit ternary expression
        /// </summary>
        TResult VisitTernary(TernaryExpression node);
        /// <summary>
        /// Visit Indexed Argument expression
        /// </summary>
        TResult VisitIndex(IndexExpression node);
        /// <summary>
        /// Visit Null
        /// </summary>
        TResult VisitNull(NullExpression node);
        /// <summary>
        /// Visit Null Propagator
        /// </summary>
        TResult VisitNullPropegator(NullPropegatorExpression node);
    }
}
