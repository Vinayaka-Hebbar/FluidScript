using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Abstract node for all syntax tree
    /// </summary>
    public abstract class Node
    {
        static readonly IEnumerable<Node> emptyNodes = Enumerable.Empty<Node>();

        /// <summary>
        /// Creates new <see cref="Node"/>
        /// </summary>
        protected Node() { }

        /// <summary>
        /// Child node iterator
        /// </summary>
        public virtual IEnumerable<Node> ChildNodes() => emptyNodes;

        /// <summary>
        /// Contains a specified node <typeparamref name="T"/>
        /// </summary>
        public bool ContainsNodeOfType<T>() where T : Node
        {
            if (this is T)
                return true;
            foreach (var child in ChildNodes())
            {
                if (child.ContainsNodeOfType<T>())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Makes child nodes
        /// </summary>
        protected static IEnumerable<Node> Childs(params Node[] values)
        {
            return values;
        }

        /// <summary>
        /// Iterate items
        /// </summary>
        public static void Iterate<TSource>(IEnumerable<TSource> sources, System.Action<TSource> callback) where TSource : Node
        {
            foreach (var item in sources)
            {
                callback(item);
            }
        }

        /// <summary>
        /// Iterate items
        /// </summary>
        public static void Iterate<TSource>(IEnumerable<TSource> sources, System.Action<TSource, int> callback) where TSource : Node
        {
            int index = 0;
            foreach (var item in sources)
            {
                callback(item, index++);
            }
        }
    }
}
