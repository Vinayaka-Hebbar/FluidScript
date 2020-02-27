using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    internal class LamdaVisitor : IExpressionVisitor<Expression>, IStatementVisitor
    {
        private readonly System.Collections.Generic.HashSet<string> Parameters;

        public LamdaVisitor(string[] parameters)
        {
            Parameters = new System.Collections.Generic.HashSet<string>(parameters);
            HoistedLocals = new System.Collections.Generic.Dictionary<string, Expression>();
        }

        public System.Collections.Generic.Dictionary<string, Expression> HoistedLocals { get; }

        public Expression Visit(Expression node)
        {
            node.Accept(this);
            return node;
        }

        public Expression VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            return node;
        }

        public Expression VisitAnonymousObject(AnonymousObjectExpression node)
        {
            for (int i = 0; i < node.Members.Length; i++)
            {
                node.Members[i].Expression.Accept(this);
            }
            return node;
        }

        public Expression VisitArrayLiteral(ArrayLiteralExpression node)
        {
            for (int i = 0; i < node.Expressions.Length; i++)
            {
                node.Expressions[i].Accept(this);
            }
            return node;
        }

        public Expression VisitAssignment(AssignmentExpression node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            return node;
        }

        public Expression VisitBinary(BinaryExpression node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            return node;
        }

        public void VisitBlock(BlockStatement node)
        {
            for (int i = 0; i < node.Statements.Length; i++)
            {
                node.Statements[i].Accept(this);
            }
        }

        public void VisitBreak(BreakStatement node)
        {
           
        }

        public Expression VisitCall(InvocationExpression node)
        {
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                node.Arguments[i].Accept(this);
            }
            node.Target.Accept(this);
            return node;
        }

        public void VisitContinue(ContinueStatement node)
        {
           
        }

        public Expression VisitDeclaration(VariableDeclarationExpression node)
        {
            node.Value.Accept(this);
            return node;
        }

        public void VisitDeclaration(LocalDeclarationStatement node)
        {
            for (int i = 0; i < node.DeclarationExpressions.Length; i++)
            {
                node.DeclarationExpressions[i].Accept(this);
            }
        }

        public void VisitExpression(ExpressionStatement node)
        {
            node.Expression.Accept(this);
        }

        public void VisitIf(IfStatement node)
        {
            node.Condition.Accept(this);
            node.Then.Accept(this);
            node.Else?.Accept(this);
        }

        public Expression VisitIndex(IndexExpression node)
        {
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                node.Arguments[i].Accept(this);
            }
            node.Target.Accept(this);
            return node;
        }

        public Expression VisitLiteral(LiteralExpression node)
        {
            return node;
        }

        public void VisitLoop(LoopStatement node)
        {
            node.InitStatement?.Accept(this);
            node.Condition?.Accept(this);
            node.IncrementStatement?.Accept(this);
            node.Body.Accept(this);
        }

        public Expression VisitMember(MemberExpression node)
        {
            node.Target.Accept(this);
            return node;
        }

        public Expression VisitMember(NameExpression node)
        {
            var name = node.Name;
            if (Parameters.Contains(name) == false && HoistedLocals.ContainsKey(name) == false)
                HoistedLocals.Add(name, node);
            return node;
        }

        public Expression VisitNull(NullExpression node)
        {
            return node;
        }

        public Expression VisitNullPropegator(NullPropegatorExpression node)
        {
            node.Left.Accept(this);
            node.Right.Accept(this);
            return node;
        }

        public void VisitReturn(ReturnOrThrowStatement node)
        {
            node.Expression.Accept(this);
        }

        public Expression VisitTernary(TernaryExpression node)
        {
            node.First.Accept(this);
            node.Second.Accept(this);
            node.Third.Accept(this);
            return node;
        }

        public Expression VisitThis(ThisExpression node)
        {
            var name = node.ToString();
            if (HoistedLocals.ContainsKey(name) == false)
                HoistedLocals.Add(name, node);
            return node;
        }

        public Expression VisitUnary(UnaryExpression node)
        {
            node.Operand.Accept(this);
            return node;
        }
    }
}
