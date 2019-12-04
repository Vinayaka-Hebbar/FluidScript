using FluidScript.Reflection.Emit;
using System;
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

        public System.Type[] ArgumentTypes(MethodBodyGenerator info)
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
                if ((current.ReflectedType & RuntimeType.Array) == FluidScript.RuntimeType.Array)
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

        protected override void ResolveType(MethodBodyGenerator generator)
        {
            if (NodeType == ExpressionType.Invocation)
            {
                var types = GetTypes(generator, Arguments);
                System.Type resultType = null;
                string name = null;
                if (Target.NodeType == ExpressionType.Identifier)
                {
                    resultType = generator.TypeGenerator;
                    name = Target.ToString();
                }
                else if (Target.NodeType == ExpressionType.MemberAccess)
                {
                    var exp = (QualifiedExpression)Target;
                    resultType = exp.ResultType(generator);
                    name = exp.Name;
                }
                bool HasAttribute(System.Reflection.MethodInfo m)
                {
                    if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                    {
                        var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                        if (data != null)
                            return data.Match(name);
                    }
                    return false;
                }
                var methods = resultType
                    .GetMethods(TypeUtils.Any)
                    .Where(HasAttribute).ToArray();
                System.Reflection.MethodBase method = (System.Reflection.MethodInfo)Type.DefaultBinder.SelectMethod(TypeUtils.Any, methods, types, null);
                if (method is IMethodBaseGenerator baseGenerator)
                    method = baseGenerator.MethodBase;
                //todo method.DeclaringType == resultType
                if (method is System.Reflection.MethodInfo)
                    ResolvedType = ((System.Reflection.MethodInfo)method).ReturnType;
            }
            else if (NodeType == ExpressionType.Indexer)
            {
                System.Type type = Target.ResultType(generator);
                ResolvedType = type.GetElementType();
            }
        }


        public override void GenerateCode(MethodBodyGenerator generator)
        {
            if (NodeType == ExpressionType.Invocation)
            {
                var types = GetTypes(generator, Arguments);
                System.Type resultType = null;
                string name = null;
                if (Target.NodeType == ExpressionType.Identifier)
                {
                    resultType = generator.TypeGenerator;
                    name = Target.ToString();
                }
                else if (Target.NodeType == ExpressionType.MemberAccess)
                {
                    var exp = (QualifiedExpression)Target;
                    resultType = exp.ResultType(generator);
                    name = exp.Name;
                    exp.GenerateCode(generator);
                }
                bool HasAttribute(System.Reflection.MethodInfo m)
                {
                    if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                    {
                        var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                        if (data != null)
                            return data.Match(name);
                    }
                    return false;
                }
                var methods = resultType
                    .GetMethods(TypeUtils.Any)
                    .Where(HasAttribute).ToArray();
                System.Reflection.MethodBase method = (System.Reflection.MethodInfo)Type.DefaultBinder.SelectMethod(TypeUtils.Any, methods, types, null);
                if (method is IMethodBaseGenerator baseGenerator)
                    method = baseGenerator.MethodBase;
                //todo method.DeclaringType == resultType
                if(Target.NodeType == ExpressionType.Identifier)
                {
                    if (method.IsStatic == false)
                        generator.LoadArgument(0);
                }
                foreach (var item in Arguments)
                {
                    item.GenerateCode(generator);
                }
                generator.Call(method);
                if (method is System.Reflection.MethodInfo)
                    ResolvedType = ((System.Reflection.MethodInfo)method).ReturnType;
            }
            else if (NodeType == ExpressionType.Indexer)
            {
                Target.GenerateCode(generator);
                foreach (var expr in Arguments)
                {
                    expr.GenerateCode(generator);
                }
                System.Type type = Target.ResultType(generator);
                ResolvedType = type.GetElementType();
                generator.LoadArrayElement(type);
            }
        }

        internal static System.Type[] GetTypes(MethodBodyGenerator generator, System.Collections.Generic.IEnumerable<Expression> expressions)
        {
            return expressions.Select(arg => arg.ResultType(generator)).ToArray();
        }

        internal static void GenerateCall(MethodBodyGenerator generator, System.Reflection.MethodBase method, System.Collections.Generic.IEnumerable<Expression> arguments)
        {
            foreach (var item in arguments)
            {
                item.GenerateCode(generator);
            }
            generator.Call(method);
        }

        public override string ToString()
        {
            return string.Concat(Target.ToString(), "(", string.Join(",", Arguments.Select(arg => arg.ToString())), ")");
        }
    }
}
