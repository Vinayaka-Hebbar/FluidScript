using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript
{
    public interface IFunction<TArg, TResult> : IEnumerable<IFunctionPart<TArg, TResult>>
    {
        string Name { get; }
        IFunctionPart<TArg, TResult> Having(int count, Scope scope);
    }

    public interface IFunctionPart<TArg, TResult>
    {
        IFunction Function { get; }
        int Count { get; }
        Scope Scope { get; }
        TResult Invoke(NodeVisitor context, TArg[] args);
    }

    public interface IFunction : IFunction<IExpression, Object>, ICollection<IFunctionPart<IExpression, Object>>
    {
        void Add(FunctionPartBuilder builder);
        IFunction Clone();
    }

    public struct FunctionPartBuilder
    {
        public readonly int Count;
        public readonly Func<NodeVisitor, IExpression[], Object> Invoke;
        public readonly Scope PartType;

        public FunctionPartBuilder(int count, Func<NodeVisitor, IExpression[], Object> invoke, Scope partType = Scope.Any)
        {
            PartType = partType;
            Count = count;
            Invoke = invoke;
        }
    }

    public sealed class FunctionPart : IFunctionPart<IExpression, Object>
    {
        private readonly Func<NodeVisitor, IExpression[], Object> _onInvoke;
        public FunctionPart(IFunction function, int count, Func<NodeVisitor, IExpression[], Object> onInvoke)
        {
            Function = function;
            Count = count;
            _onInvoke = onInvoke;
        }

        public FunctionPart(IFunction function, FunctionPartBuilder builder)
        {
            Function = function;
            Count = builder.Count;
            _onInvoke = builder.Invoke;
            Scope = builder.PartType;
        }

        public IFunction Function { get; }
        public int Count { get; }

        public Scope Scope { get; } = Scope.Any;

        public Object Invoke(NodeVisitor context, IExpression[] args)
        {
            return _onInvoke(context, args);
        }
    }

    public sealed class Function : IFunction
    {
        public static readonly FunctionPartBuilder Empty = new FunctionPartBuilder(-1, ThrowNotFound, Scope.Global);


        public readonly List<IFunctionPart<IExpression, Object>> Parts;
        public string Name { get; }

        public int Count => Parts.Count;

        public bool IsReadOnly => false;

        public static readonly IExpression[] NoArguments = new IExpression[] { };

        public Function(string name, params FunctionPartBuilder[] parts)
        {
            Name = name;
            Parts = new List<IFunctionPart<IExpression, Object>>(parts.Select(GetFunctionPart));
        }

        private IFunctionPart<IExpression, Object> GetFunctionPart(FunctionPartBuilder part)
        {
            return new FunctionPart(this, part);
        }

        public Function(IFunction function)
        {
            Name = function.Name;
            Parts = new List<IFunctionPart<IExpression, Object>>(function);
        }

        public Function(string name, int count, Func<NodeVisitor, IExpression[], Object> onInvoke)
        {
            Name = name;
            Parts = new List<IFunctionPart<IExpression, Object>>
            {
                new FunctionPart(this,count,onInvoke)
            };
        }

        public IEnumerator<IFunctionPart<IExpression, Object>> GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        public IFunctionPart<IExpression, Object> Having(int count, Scope scope)
        {
            //todo var args
            var selected = Parts.Where(function => function.Count == count || function.Count == -1);
            var matched = selected.FirstOrDefault(function => function.Count == count && (function.Scope & scope) == scope);
            if (matched == null)
            {
                matched = selected.FirstOrDefault(function => function.Count == count && (function.Scope & Scope.Any) == Scope.Any);
                if (matched == null)
                {
                    matched = selected.FirstOrDefault(function => function.Count == -1);
                    if (matched == null)
                        throw new MissingMethodException("Method Not Found");
                }
            }

            return matched;
        }

        public void Add(IFunctionPart<IExpression, Object> item)
        {
            Parts.Add(item);
        }

        public void Clear()
        {
            Parts.Clear();
        }

        public bool Contains(IFunctionPart<IExpression, Object> item)
        {
            return Parts.Contains(item);
        }

        public void CopyTo(IFunctionPart<IExpression, Object>[] array, int arrayIndex)
        {
            Parts.CopyTo(array, arrayIndex);
        }

        public bool Remove(IFunctionPart<IExpression, Object> item)
        {
            return Parts.Remove(item);
        }

        public void Add(FunctionPartBuilder builder)
        {
            Parts.Add(new FunctionPart(this, builder));
        }

        private static Object ThrowNotFound(NodeVisitor arg1, IExpression[] arg2)
        {
            throw new Exception("method not found");
        }

        public IFunction Clone()
        {
            return new Function(this);
        }
    }

    public enum Scope { Any = 1, Global = Any | 2, Program = Any | 4, Local = Any | Program | 8 }
}
