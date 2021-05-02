using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    internal class LamdaVisitor : IExpressionVisitor<Expression>, IStatementVisitor
    {
        private readonly System.Collections.Generic.HashSet<string> Parameters;

        private readonly System.Collections.Generic.HashSet<string> LocalVariables;

        public LamdaVisitor(string[] parameters)
        {
            Parameters = new System.Collections.Generic.HashSet<string>(parameters);
            LocalVariables = new System.Collections.Generic.HashSet<string>();
            HoistedLocals = new System.Collections.Generic.Dictionary<string, Expression>();
        }

        internal System.Collections.Generic.Dictionary<string, Expression> HoistedLocals { get; }

        public Expression Default(Expression node)
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
            for (int i = 0; i < node.Members.Count; i++)
            {
                node.Members[i].Expression.Accept(this);
            }
            return node;
        }

        public Expression VisitArrayLiteral(ArrayListExpression node)
        {
            for (int i = 0; i < node.Expressions.Count; i++)
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
            for (int i = 0; i < node.Statements.Count; i++)
            {
                node.Statements[i].Accept(this);
            }
        }

        public void VisitBreak(BreakStatement node)
        {

        }

        public Expression VisitCall(InvocationExpression node)
        {
            node.Arguments.ForEach(arg => arg.Accept(this));
            var exp = node.Target.Accept(this);
            if (exp.NodeType == ExpressionType.Identifier)
            {
                var identifier = (NameExpression)exp;
                var name = identifier.Name;
                if (Parameters.Contains(name) == false && LocalVariables.Contains(name) == false && HoistedLocals.ContainsKey(name) == false)
                    HoistedLocals.Add(name, exp);
            }
            else if (node.NodeType == ExpressionType.MemberAccess)
            {
                var member = (MemberExpression)exp;
                member.Target.Accept(this);
            }
            return node;
        }

        public void VisitContinue(ContinueStatement node)
        {

        }

        public Expression VisitConvert(ConvertExpression node)
        {
            node.Target.Accept(this);
            return node;
        }

        public Expression VisitDeclaration(VariableDeclarationExpression node)
        {
            LocalVariables.Add(node.Name);
            node.Value.Accept(this);
            return node;
        }

        public void VisitDeclaration(LocalDeclarationStatement node)
        {
            for (int i = 0; i < node.DeclarationExpressions.Count; i++)
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
            node.Arguments.ForEach(arg => arg.Accept(this));
            node.Target.Accept(this);
            return node;
        }

        public Expression VisitLiteral(LiteralExpression node)
        {
            return node;
        }

        public void VisitLoop(LoopStatement node)
        {
            node.Initialization?.Accept(this);
            node.Condition.Accept(this);
            node.Increments?.ForEach(e => e.Accept(this));
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
            if (Parameters.Contains(name) == false && LocalVariables.Contains(name) == false && HoistedLocals.ContainsKey(name) == false)
                HoistedLocals.Add(name, node);
            return node;
        }

        public Expression VisitNew(NewExpression node)
        {
            node.Arguments.ForEach(n => n.Accept(this));
            return node;
        }

        public Expression VisitInstanceOf(InstanceOfExpression node)
        {
            node.Target.Accept(this);
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
            node.Value.Accept(this);
        }

        public Expression VisitSizeOf(SizeOfExpression node)
        {
            node.Value.Accept(this);
            return node;
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
            var name = "__value";
            if (HoistedLocals.ContainsKey(name) == false)
                HoistedLocals.Add(name, node);
            return node;
        }

        public Expression VisitSuper(SuperExpression node)
        {
            var name = "__value";
            if (HoistedLocals.ContainsKey(name) == false)
                HoistedLocals.Add(name, node);
            return node;
        }

        public Expression VisitUnary(UnaryExpression node)
        {
            node.Operand.Accept(this);
            return node;
        }

        public void VisitImport(ImportStatement node)
        {
        }
    }
}
