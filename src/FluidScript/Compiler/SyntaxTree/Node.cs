using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Abstract node for all syntax tree
    /// </summary>
    public abstract class Node
    {
        static readonly Node[] EmptyNodes = new Node[0];

        /// <summary>
        /// Creates new <see cref="Node"/>
        /// </summary>
        protected Node() { }

        /// <summary>
        /// Child node iterator
        /// </summary>
        public virtual IEnumerable<Node> ChildNodes() => EmptyNodes;

        /// <summary>
        /// Makes child nodes
        /// </summary>
        protected static IEnumerable<Node> Childs(params Node[] values)
        {
            return values;
        }
    }
}
