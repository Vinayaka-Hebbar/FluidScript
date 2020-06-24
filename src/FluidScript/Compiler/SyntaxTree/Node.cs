using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Abstract node for all syntax tree
    /// </summary>
    public abstract class Node
    {
        static readonly IEnumerable<Node> EmptyNodes = Enumerable.Empty<Node>();

        /// <summary>
        /// Creates new <see cref="Node"/>
        /// </summary>
        protected Node() { }

        /// <summary>
        /// Child node iterator
        /// </summary>
        public virtual IEnumerable<Node> ChildNodes() => EmptyNodes;

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
    }
}
