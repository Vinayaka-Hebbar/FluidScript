using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Node
    {
        static readonly IEnumerable<Node> emptyNodes = Enumerable.Empty<Node>();

        protected Node() { }

        public virtual IEnumerable<Node> ChildNodes
        {
            get
            {
                return emptyNodes;
            }
        }

        public bool ContainsNodeOfType<T>() where T : Node
        {
            if (this is T)
                return true;
            foreach (var child in ChildNodes)
            {
                if (child.ContainsNodeOfType<T>())
                    return true;
            }
            return false;
        }

        public static IEnumerable<Node> Childs(params Node[] values)
        {
            return values;
        }

        /// <summary>
        /// Get Value for the scope
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public virtual RuntimeObject Evaluate()
        {
            return RuntimeObject.Null;
        }
    }
}
