using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class QualifiedExpression : Expression
    {
        public readonly Expression Target;
        public readonly string Name;

        public QualifiedExpression(Expression target, string name, ExpressionType opCode) : base(opCode)
        {
            Target = target;
            Name = name;
        }

        public override IEnumerable<Node> ChildNodes => Childs(Target);
        public override string ToString()
        {
            if (NodeType == ExpressionType.QualifiedNamespace)
            {
                return Target.ToString() + '.' + Name;
            }
            return Name.ToString();
        }

        protected override void ResolveType(OptimizationInfo info)
        {
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var value = (NameExpression)Target;
                var name = value.Name;
                var scope = value.Scope;
                do
                {
                    switch (scope.Context)
                    {
                        case Metadata.ScopeContext.Local:
                            var local = (Metadata.DeclarativeScope)scope;
                            var variable = local.GetVariable(name);
                            if (variable != null)
                            {
                                ResolvedPrimitiveType = variable.PrimitiveType;
                                ResolvedType = variable.ResolveType(info);

                            }
                            break;
                        case Metadata.ScopeContext.Type:
                            var type = (Metadata.ObjectScope)scope;
                            Reflection.DeclaredMember member = type.GetMember(name).FirstOrDefault(item => item.IsMethod == false);
                            if (member != null)
                            {
                                ResolvedType = member.Declaration.ResolvedType;
                            }
                            else
                            {
                                System.Type resolvedType = null;
                                var declaredMember = info.DeclaringType.GetMember(name)
                                    .FirstOrDefault(item => item.MemberType != System.Reflection.MemberTypes.Method);
                                if (declaredMember.MemberType == System.Reflection.MemberTypes.Property)
                                {
                                    var property = (System.Reflection.PropertyInfo)declaredMember;
                                    resolvedType = property.PropertyType;
                                }
                                else if (declaredMember.MemberType == System.Reflection.MemberTypes.Field)
                                {
                                    var field = (System.Reflection.FieldInfo)declaredMember;
                                    resolvedType = field.FieldType;
                                }
                                if (resolvedType != null)
                                {
                                    ResolvedType = resolvedType;
                                    ResolvedPrimitiveType = Emit.TypeUtils.ToPrimitive(resolvedType);
                                }
                            }
                            break;
                        case Metadata.ScopeContext.Global:
                            break;
                    }
                    scope = scope.ParentScope;
                } while (scope != null);
            }
        }
    }
}
