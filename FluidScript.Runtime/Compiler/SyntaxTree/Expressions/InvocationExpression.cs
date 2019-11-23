#if Emit
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.Reflection;
#endif
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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var args = new RuntimeObject[Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
            {
                args[i] = Arguments[i].Evaluate(instance);
            }
            if (NodeType == ExpressionType.Invocation)
            {
                if (Target.NodeType == ExpressionType.Identifier)
                {
                    var value = (NameExpression)Target;
                    return instance.Call(value.Name, args);
                }
                else if (Target.NodeType == ExpressionType.MemberAccess)
                {
                    var qualified = (QualifiedExpression)Target;
                    var value = qualified.Target.Evaluate(instance);
                    return value.Call(qualified.Name, args);
                }
                else
                {
                    var value = Target.Evaluate(instance);
                    return value.DynamicInvoke(args);
                }
            }
            if (NodeType == ExpressionType.Indexer)
            {
                var value = Target.Evaluate(instance);
                return value.DynamicInvoke(args);
            }
            return RuntimeObject.Null;
        }

        internal void SetArray(RuntimeObject instance, RuntimeObject value)
        {
            var args = new RuntimeObject[Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
            {
                args[i] = Arguments[i].Evaluate(instance);
            }
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var identifier = (NameExpression)Target;
                var result = identifier.Evaluate(instance);
                Library.ArrayObject org = (Library.ArrayObject)result;
                var array = org;
                var modified = SetArrayAtIndex(instance, Arguments, ref array, value);
                if (!ReferenceEquals(org, modified))
                {
                    instance[identifier.Name] = modified;
                }
            }
        }

        internal static Library.ArrayObject SetArrayAtIndex(RuntimeObject instance, Expression[] args, ref Library.ArrayObject target, RuntimeObject value)
        {
            RuntimeObject current = RuntimeObject.Null;
            var indexes = SkipLast(args).Select(arg => arg.Evaluate(instance).ToInt32()).ToArray();
            var index = args.Last().Evaluate(instance).ToInt32();
            target = GetArray(indexes, ref target);
            if (target.Length <= index)
            {
                target.Resize(index + 1);
            }
            target[index] = value;
            return target;
        }

        private static Library.ArrayObject GetArray(int[] indexes, ref Library.ArrayObject target)
        {
            Library.ArrayObject array = target;
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (array.Length <= index)
                {
                    array.Resize(index + 1);
                }
                var current = array[index];
                if ((current.ReflectedType & FluidScript.RuntimeType.Array) == FluidScript.RuntimeType.Array)
                {
                    var innerArray = (Library.ArrayObject)current;
                    array = GetArray(indexes.Skip(i + 1).Take(indexes.Length - 1).ToArray(), ref innerArray);
                }

            }
            return array;
        }

        private static System.Collections.Generic.IEnumerable<Expression> SkipLast(Expression[] expressions)
        {
            for (int i = 0; i < expressions.Length - 1; i++)
            {
                yield return expressions[i];
            }
        }

#endif

#if Emit
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
                                Call(method.Store, generator, info);
                                return;
                            }
                            else
                            {
                                var targetMethod = info.DeclaringType.BaseType.GetMethod(name, ArgumentTypes(info));
                                Call(targetMethod, generator, info);
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
                            case Scopes.ScopeContext.Local:
                                //todo inner method
                                break;
                            case Scopes.ScopeContext.Type:
                                var type = (Scopes.ObjectScope)scope;
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
                ResolvedType = Target.ResultType(info).GetElementType();
                ResolvedPrimitiveType = Target.PrimitiveType(info) & (~FluidScript.PrimitiveType.Array);
            }
        }
#endif

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Select(arg => arg.ToString())), ")");
        }
    }
}
