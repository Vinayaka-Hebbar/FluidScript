using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class InvocationExpression : Expression
    {
        public readonly Expression Target;
        public readonly Expression[] Arguments;

        public InvocationExpression(Expression target, Expression[] arguments, ExpressionType opCode) : base(opCode)
        {
            Target = target;
            Arguments = arguments;
        }

        public System.Type[] ArgumentTypes()
        {
            return Arguments.Select(arg => arg.ResultType()).ToArray();
        }

        public override PrimitiveType PrimitiveType()
        {
            if (ResolvedPrimitiveType == FluidScript.PrimitiveType.Any)
            {
                if (NodeType == ExpressionType.Invocation)
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
                                    //todo inner method
                                    break;
                                case Scopes.ScopeContext.Type:
                                    var type = (Scopes.ObjectScope)scope;
                                    DeclaredMethod method = type.GetMethod(name, ArgumentTypes(declaredType));
                                    if (method != null)
                                    {
                                        ResolvedPrimitiveType = method.Declaration.PrimitiveType;
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
                if (NodeType == ExpressionType.Indexer)
                {
                    return Target.PrimitiveType(declaredType) & (~FluidScript.PrimitiveType.Array);
                }
            }
            return ResolvedPrimitiveType;
        }

        public override System.Type ResultType()
        {
            if (ResolvedType == null)
            {
                ResolvedType = GetResolvedType(info);
            }
            return ResolvedType;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (NodeType == ExpressionType.Invocation)
            {
                GenerateCall(generator, info);
            }
            if (NodeType == ExpressionType.Indexer)
            {
                Target.GenerateCode(generator, info);
                foreach (var expr in Arguments)
                {
                    expr.GenerateCode(generator, info);
                }
                System.Type type = ResultType(info);
                generator.LoadArrayElement(type);
            }
        }

        private void GenerateCall(ILGenerator generator, MethodOptimizationInfo info)
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
                            //todo inner method
                            break;
                        case Scopes.ScopeContext.Type:
                            var type = (Scopes.ObjectScope)scope;
                            DeclaredMethod method = type.GetMethod(name, ArgumentTypes(info));
                            if (method != null)
                            {
                                Call(method, generator, info);
                                return;
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

        private void Call(DeclaredMethod method, ILGenerator generator, MethodOptimizationInfo info)
        {
            if (method.Store.IsStatic == false)
            {
                generator.LoadArgument(0);

            }
            foreach (var expression in Arguments)
            {
                expression.GenerateCode(generator, info);
            }
            generator.Call(method.Store);
        }

        private System.Type GetResolvedType(Emit.MethodOptimizationInfo info)
        {
            if (NodeType == ExpressionType.Invocation)
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
                                //todo inner method
                                break;
                            case Scopes.ScopeContext.Type:
                                var type = (Scopes.ObjectScope)scope;
                                DeclaredMethod method = type.GetMethod(name, ArgumentTypes(info));
                                if (method != null)
                                {
                                    return method.Store.ReturnType;
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
            if (NodeType == ExpressionType.Indexer)
            {
                return Target.ResultType(info).GetElementType();
            }
            return null;
        }
    }
}
