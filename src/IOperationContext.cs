using System.Collections.Generic;

namespace FluidScript
{
    /// <summary>
    /// Store Variables and constants
    /// </summary>
    public interface IOperationContext
    {
        IDictionary<string, IFunction> Functions { get; }
        IDictionary<string, Object> Variables { get; }
        IDictionary<string, Object> Constants { get; }
        IReadOnlyOperationContext ReadOnlyContext { get; }
        IFunctionPart<IExpression, Object> GetFunctionPart(string name, int argCount, Scope scope);
        Object this[string name] { get; set; }
        bool Contains(string name);
        void Concat<TSource>(IEnumerable<KeyValuePair<string, TSource>> values);

        Object GetConstant(string name);
        bool ContainsConstant(string name);
        void Add(string name, FunctionPartBuilder function);
        IDictionary<string, TSource> UserDefinedValues<TSource>();
    }

    public interface IReadOnlyOperationContext
    {
        bool TryGetIdentifier(string name, out IdentifierType type);
    }
}
