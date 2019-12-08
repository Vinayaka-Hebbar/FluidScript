using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class IndexExpression : Expression
    {
        public readonly Expression Target;

        public readonly Expression[] Arguments;

        public System.Reflection.PropertyInfo Indexer { get; internal set; }

        public IndexExpression(Expression target, Expression[] arguments) : base(ExpressionType.Indexer)
        {
            Target = target;
            Arguments = arguments;
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            Target.GenerateCode(generator);
            System.Type type = Target.Type;
            if (type.IsArray)
            {
                Iterate(Arguments, (arg) =>
                {
                    arg.GenerateCode(generator);
                    generator.CallStatic(ReflectionHelpers.Integer_to_Int32);
                });
                System.Type elementType = type.GetElementType();
                generator.LoadArrayElement(elementType);
            }
            else
            {

                Iterate(Arguments, (arg) => arg.GenerateCode(generator));
                System.Reflection.MethodInfo indexer = Indexer.GetGetMethod(true);
                //todo indexer argument convert
                generator.Call(indexer);
            }
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitIndex(this);
        }

        #region Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var args = new RuntimeObject[Arguments.Length];
            for (int i = 0; i < Arguments.Length; i++)
            {
                args[i] = Arguments[i].Evaluate(instance);
            }
            var value = Target.Evaluate(instance);
            return value.DynamicInvoke(args);
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
        #endregion
    }
}
