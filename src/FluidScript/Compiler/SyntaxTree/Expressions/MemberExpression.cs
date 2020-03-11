using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Member access expression
    /// </summary>
    public class MemberExpression : Expression
    {
        public readonly Expression Target;
        public readonly string Name;

        /// <summary>
        /// Compiler generation binder
        /// </summary>
        public Binders.IBinder Binder
        {
            get;
            set;
        }

        public MemberExpression(Expression target, string name, ExpressionType opCode) : base(opCode)
        {
            Target = target;
            Name = name;
        }

        public override IEnumerable<Node> ChildNodes() => Target.ChildNodes().Concat(Childs(Target));

        public override string ToString()
        {
            if (NodeType == ExpressionType.QualifiedNamespace || NodeType == ExpressionType.MemberAccess)
            {
                return Target.ToString() + '.' + Name;
            }
            return Name.ToString();
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            Target.GenerateCode(generator);
            Binder?.GenerateGet(generator);
        }
    }
}
