using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler
{
    public interface IFunction<TArg, TResult> : IEnumerable<IFunctionPart<TArg, TResult>>
    {
        string Name { get; }
        IFunctionPart<TArg, TResult> Having(int count, CodeScope scope);
    }

    public interface IFunctionPart<TArg, TResult>
    {
        IFunction Function { get; }
        int Count { get; }
        CodeScope Scope { get; }
        TResult Invoke(INodeVisitor<Object> context, TArg[] args);
    }

    public interface IFunction : IFunction<Node, Object>, ICollection<IFunctionPart<Node, Object>>
    {
        void Add(FunctionPartBuilder builder);
        IFunction Clone();
    }

    public struct FunctionPartBuilder
    {
        public readonly int Count;
        public readonly Func<INodeVisitor<Object>, Node[], Object> Invoke;
        public readonly CodeScope PartType;

        public FunctionPartBuilder(int count, Func<INodeVisitor<Object>, Node[], Object> invoke, CodeScope partType = CodeScope.Any)
        {
            PartType = partType;
            Count = count;
            Invoke = invoke;
        }
    }

    public sealed class FunctionPart : IFunctionPart<Node, Object>
    {
        private readonly Func<INodeVisitor<Object>, Node[], Object> _onInvoke;
        public FunctionPart(IFunction function, int count, Func<INodeVisitor<Object>, Node[], Object> onInvoke)
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

        public CodeScope Scope { get; } = CodeScope.Any;

        public Object Invoke(INodeVisitor<Object> context, Node[] args)
        {
            return _onInvoke(context, args);
        }
    }

    public sealed class Function : IFunction
    {
        public static readonly FunctionPartBuilder Empty = new FunctionPartBuilder(-1, ThrowNotFound, CodeScope.Global);


        public readonly List<IFunctionPart<Node, Object>> Parts;
        public string Name { get; }

        public int Count => Parts.Count;

        public bool IsReadOnly => false;

        public static readonly Node[] NoArguments = new Node[] { };

        public Function(string name, params FunctionPartBuilder[] parts)
        {
            Name = name;
            Parts = new List<IFunctionPart<Node, Object>>(parts.Select(GetFunctionPart));
        }

        private IFunctionPart<Node, Object> GetFunctionPart(FunctionPartBuilder part)
        {
            return new FunctionPart(this, part);
        }

        public Function(IFunction function)
        {
            Name = function.Name;
            Parts = new List<IFunctionPart<Node, Object>>(function);
        }

        public Function(string name, int count, Func<INodeVisitor<Object>, Node[], Object> onInvoke)
        {
            Name = name;
            Parts = new List<IFunctionPart<Node, Object>>
            {
                new FunctionPart(this,count,onInvoke)
            };
        }

        public IEnumerator<IFunctionPart<Node, Object>> GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Parts.GetEnumerator();
        }

        public IFunctionPart<Node, Object> Having(int count, CodeScope scope)
        {
            //todo var args
            var selected = Parts.Where(function => function.Count == count || function.Count == -1);
            var matched = selected.FirstOrDefault(function => function.Count == count && (function.Scope & scope) == scope);
            if (matched == null)
            {
                matched = selected.FirstOrDefault(function => function.Count == count && (function.Scope & CodeScope.Any) == CodeScope.Any);
                if (matched == null)
                {
                    matched = selected.FirstOrDefault(function => function.Count == -1);
                    if (matched == null)
                        throw new MissingMethodException("Method Not Found");
                }
            }

            return matched;
        }

        public void Add(IFunctionPart<Node, Object> item)
        {
            Parts.Add(item);
        }

        public void Clear()
        {
            Parts.Clear();
        }

        public bool Contains(IFunctionPart<Node, Object> item)
        {
            return Parts.Contains(item);
        }

        public void CopyTo(IFunctionPart<Node, Object>[] array, int arrayIndex)
        {
            Parts.CopyTo(array, arrayIndex);
        }

        public bool Remove(IFunctionPart<Node, Object> item)
        {
            return Parts.Remove(item);
        }

        public void Add(FunctionPartBuilder builder)
        {
            Parts.Add(new FunctionPart(this, builder));
        }

        private static Object ThrowNotFound(INodeVisitor<Object> arg1, Node[] arg2)
        {
            throw new Exception("method not found");
        }

        public IFunction Clone()
        {
            return new Function(this);
        }
    }

    public enum CodeScope { Any = 0, Global = 1, Class = Global | 4, Local =  Class | 8 }
}
