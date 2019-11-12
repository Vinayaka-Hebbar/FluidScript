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

        public System.Type[] ArgumentTypes(Emit.OptimizationInfo info)
        {
            return Arguments.Select(arg => arg.ResultType(info)).ToArray();
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
                        case Metadata.ScopeContext.Local:
                            //todo inner method
                            break;
                        case Metadata.ScopeContext.Type:
                            var type = (Metadata.ObjectScope)scope;
                            DeclaredMethod method = type.GetMethod(name, ArgumentTypes(info));
                            if (method != null)
                            {
                                Call(method.Store, generator, info);
                                return;
                            }
                            else
                            {
                                var targetMethod = info.DeclaringType.BaseType.GetMethod(name, ArgumentTypes(info));
                                Call(targetMethod, generator, info);
                            }
                            break;
                        case Metadata.ScopeContext.Global:
                            break;
                        default:
                            break;
                    }
                    scope = scope.ParentScope;
                } while (scope != null);

            }
        }

        private void Call(System.Reflection.MethodBase method, ILGenerator generator, MethodOptimizationInfo info)
        {
            if (method.IsStatic == false)
            {
                generator.LoadArgument(0);

            }
            foreach (var expression in Arguments)
            {
                expression.GenerateCode(generator, info);
            }
            generator.Call(method);
        }

        protected override void ResolveType(OptimizationInfo info)
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
                            case Metadata.ScopeContext.Local:
                                //todo inner method
                                break;
                            case Metadata.ScopeContext.Type:
                                var type = (Metadata.ObjectScope)scope;
                                DeclaredMethod method = type.GetMethod(name, ArgumentTypes(info));
                                if (method != null)
                                {
                                    ResolvedType = method.Store.ReturnType;
                                    ResolvedPrimitiveType = method.Declaration.PrimitiveType;
                                    return;
                                }
                                var targetMethod = info.DeclaringType.BaseType.GetMethod(name, ArgumentTypes(info));
                                if(targetMethod != null)
                                {
                                    ResolvedType = targetMethod.ReturnType;
                                    ResolvedPrimitiveType = Emit.TypeUtils.ToPrimitive(ResolvedType);
                                    return;
                                }
                                break;
                            case Metadata.ScopeContext.Global:
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
                ResolvedType = Target.ResultType(info).GetElementType();
                ResolvedPrimitiveType = Target.PrimitiveType(info) & (~FluidScript.PrimitiveType.Array);
            }
        }
    }
}
