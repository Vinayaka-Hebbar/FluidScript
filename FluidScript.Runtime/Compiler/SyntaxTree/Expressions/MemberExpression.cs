using FluidScript.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class MemberExpression : Expression
    {
        public readonly Expression Target;
        public readonly string Name;

        public Binding Binding
        {
            get;
            protected internal set;
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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            if (NodeType == ExpressionType.MemberAccess)
            {
                var value = Target.Evaluate(instance);
                return value[Name];
            }
            return instance[Name];
        }
#endif

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (Target.NodeType == ExpressionType.This)
            {
                generator.LoadArgument(0);
            }
            else
            {
                Target.GenerateCode(generator);
                Binding.GenerateGet(generator);
            }
        }

        public void GenerateSet(MethodBodyGenerator generator, Expression right)
        {
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var target = (NameExpression)Target;
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }
    }
}
