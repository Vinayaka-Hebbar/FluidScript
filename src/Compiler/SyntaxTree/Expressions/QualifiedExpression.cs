using System;
using System.Collections.Generic;

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

        public override PrimitiveType PrimitiveType(System.Type declaredType)
        {
            if (ResolvedPrimitiveType == FluidScript.PrimitiveType.Any)
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
                            case Scopes.ScopeContext.Local:
                                var local = (Scopes.DeclarativeScope)scope;
                                var variable = local.GetVariable(name);
                                if(variable != null)
                                {
                                    return variable.PrimitiveType;
                                }
                                break;
                            case Scopes.ScopeContext.Type:
                                var type = (Scopes.ObjectScope)scope;
                                var method = type.GetMember(name);
                                if (method != null)
                                {
                                    ResolvedPrimitiveType = method.Declaration.PrimitiveType;
                                }
                                else
                                {
                                    //should be type
                                }
                                break;
                            case Scopes.ScopeContext.Global:
                                break;
                            default:
                                break;
                        }
                        scope = scope.ParentScope;
                    } while (scope != null);
                }
            }
            return ResolvedPrimitiveType;
        }
    }
}
