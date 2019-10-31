using FluidScript.Compiler.SyntaxTree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public abstract class OperationContext : IOperationContext, IEnumerable<KeyValuePair<string, Object>>
    {
        internal static readonly Keywords Inbuilt = new Keywords();
        public IDictionary<string, IFunction> Functions { get; }
        public IDictionary<string, Object> Constants { get; }
        public IDictionary<string, Object> Variables { get; }

        public abstract IReadOnlyOperationContext ReadOnlyContext { get; }

        public virtual Object this[string name] { get => Variables[name]; set => Variables[name] = value; }

        public OperationContext(IOperationContext context)
        {
            Functions = context.Functions.ToDictionary(function => function.Key, function => function.Value.Clone());
            Variables = new Dictionary<string, Object>(context.Variables);
            Constants = new Dictionary<string, Object>(context.Constants);
        }

        public OperationContext(IOperationContext context, IEqualityComparer<string> comparer)
        {
            Functions = context.Functions.ToDictionary(function => function.Key, function => function.Value.Clone(), comparer);
            Variables = new Dictionary<string, Object>(context.Variables, comparer);
            Constants = new Dictionary<string, Object>(context.Constants, comparer);
        }

        public OperationContext(IDictionary<string, IFunction> functions, IDictionary<string, Object> constants)
        {
            Functions = functions.ToDictionary(function => function.Key, function => function.Value.Clone());
            Constants = new Dictionary<string, Object>(constants);
            Variables = new Dictionary<string, Object>();
        }
        public OperationContext(IDictionary<string, IFunction> functions, IDictionary<string, Object> constants, IEqualityComparer<string> comparer)
        {
            Functions = functions.ToDictionary(function => function.Key, function => function.Value.Clone(), comparer);
            Constants = new Dictionary<string, Object>(constants, comparer);
            Variables = new Dictionary<string, Object>(comparer);
        }

        public OperationContext() : this(Inbuilt)
        {
        }

        public Object GetConstant(string name)
        {
            return Constants[name];
        }

        public bool ContainsConstant(string name)
        {
            return Constants.ContainsKey(name);
        }

        public IFunctionPart<Node, Object> GetFunctionPart(string name, int argCount, CodeScope scope)
        {
            return Functions[name].Having(argCount, scope);
        }

        public bool Contains(string name)
        {
            return Variables.ContainsKey(name);
        }

        public IEnumerator<KeyValuePair<string, Object>> GetEnumerator() => Constants.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Concat<TSource>(IEnumerable<KeyValuePair<string, TSource>> other)
        {
            foreach (var item in other)
            {
                if (Constants.ContainsKey(item.Key))
                {
                    var value = Constants[item.Key];
                    if (value.IsNull)
                    {
                        Constants[item.Key] = new Object(item.Value);
                    }
                }
                else
                {
                    Constants.Add(item.Key, new Object(item.Value));
                }
            }
        }

        public void Add(string name, FunctionPartBuilder function)
        {
            if (Functions.ContainsKey(name))
            {
                Functions[name].Add(function);
                return;
            }
            Functions.Add(name, new Function(name, function));
        }
    }
}
