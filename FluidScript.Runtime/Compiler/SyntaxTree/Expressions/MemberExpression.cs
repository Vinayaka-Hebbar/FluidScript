using FluidScript.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class MemberExpression : Expression
    {
        public readonly Expression Target;
        public readonly string Name;

        public System.Reflection.MemberInfo Member
        {
            get;
            protected internal set;
        }

        public MemberExpression(Expression target, string name, ExpressionType opCode) : base(opCode)
        {
            Target = target;
            Name = name;
        }

        public override IEnumerable<Node> ChildNodes() => Childs(Target);

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
            if (Target.NodeType == ExpressionType.Identifier || Target.NodeType == ExpressionType.Invocation)
            {
                Target.GenerateCode(generator);
            }
            else if (Target.NodeType == ExpressionType.This)
            {
                generator.LoadArgument(0);
            }
            else if (Target.NodeType == ExpressionType.MemberAccess)
            {
                var name = Name;
                var variable = generator.GetLocalVariable(name);
                if (variable != null)
                {
                    if (variable.Type == null)
                        throw new System.Exception(string.Concat("Use of undeclared variable ", variable));
                    generator.LoadVariable(variable);
                }
                else
                {
                    //find in the class level
                    var member = generator.TypeGenerator.FindMember(name).FirstOrDefault();
                    if (member != null)
                    {
                        if (member.MemberType == System.Reflection.MemberTypes.Field)
                        {
                            var field = (System.Reflection.FieldInfo)member;
                            if (field.FieldType == null)
                                throw new System.Exception(string.Concat("Use of undeclared field ", field));
                            generator.LoadField(field);
                        }
                        else if (member.MemberType == System.Reflection.MemberTypes.Property)
                        {
                            var property = (System.Reflection.PropertyInfo)member;
                            generator.Call(property.GetGetMethod(true));
                        }

                    }
                }
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitMember(this);
        }
    }
}
