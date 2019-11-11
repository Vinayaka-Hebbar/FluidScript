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

        public override RuntimeObject Evaluate()
        {
            if (NodeType == ExpressionType.Invocation)
            {
                var types = new PrimitiveType[Arguments.Length];
                var args = new RuntimeObject[Arguments.Length];
                for (int i = 0; i < Arguments.Length; i++)
                {
                    var value = Arguments[i].Evaluate();
                    args[i] = value;
                    types[i] = value.Type;
                }
                if (Target.NodeType == ExpressionType.Identifier)
                {
                    var value = (NameExpression)Target;
                    var scope = value.Scope;
                    do
                    {
                        switch (scope.Context)
                        {
                            case Scopes.ScopeContext.Type:
                                var type = (Scopes.ObjectScope)scope;
                                var method = type.GetMethod(value.Name, types);
                                if (method != null)
                                {
                                    if (method.Delegate == null)
                                    {
                                        method.Delegate = method.Create();
                                    }
                                    return method.Delegate(args);
                                }
                                break;
                        }
                        scope = scope.Parent;
                    } while (scope != null);
                }
            }
            if (NodeType == ExpressionType.Indexer)
            {
                if (Target.NodeType == ExpressionType.Identifier)
                {
                    var identifier = (NameExpression)Target;
                    var value = identifier.Evaluate();
                    return GetArrayAtIndex(Arguments, value);
                }
            }
            return RuntimeObject.Null;
        }

        internal void SetArray(RuntimeObject value)
        {
            if (Target.NodeType == ExpressionType.Identifier)
            {
                var identifier = (NameExpression)Target;
                var result = identifier.Evaluate();
                RuntimeObject[] org = result.ToArray();
                var array = org;
                var modified = SetArrayAtIndex(Arguments, ref array, value);
                if (!ReferenceEquals(org, modified))
                {
                    identifier.Set(new RuntimeObject(modified));
                }
            }
        }

        internal static RuntimeObject GetArrayAtIndex(Expression[] args, RuntimeObject value)
        {
            if ((value.Type & FluidScript.PrimitiveType.Array) == FluidScript.PrimitiveType.Array)
            {
                var array = (RuntimeObject[])value.Store;
                for (int i = 0; i < args.Length; i++)
                {
                    Expression arg = args[i];
                    int index = arg.Evaluate().ToInt32();
                    value = array[index];
                    if ((value.Type & FluidScript.PrimitiveType.Array) == FluidScript.PrimitiveType.Array)
                    {
                        array = (RuntimeObject[])value.Store;
                    }
                }
            }
            return value;
        }

        internal static RuntimeObject[] SetArrayAtIndex(Expression[] args, ref RuntimeObject[] target, RuntimeObject value)
        {
            RuntimeObject current = RuntimeObject.Null;
            var indexes = SkipLast(args).Select(arg => arg.Evaluate().ToInt32()).ToArray();
            var index = args.Last().Evaluate().ToInt32();
            target = GetArray(indexes, ref target);
            if ((target.Length > index) == false)
            {
                System.Array.Resize(ref target, index + 1);
            }
            target[index] = value;
            return target;
        }

        private static RuntimeObject[] GetArray(int[] indexes, ref RuntimeObject[] target)
        {
            RuntimeObject[] array = target;
            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if ((array.Length > index) == false)
                {
                    System.Array.Resize(ref array, index + 1);
                }
                var current = array[index];
                if ((current.Type & FluidScript.PrimitiveType.Array) == FluidScript.PrimitiveType.Array)
                {
                    var innerArray = (RuntimeObject[])current.Store;
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
            return base.ToString();
        }
    }
}
