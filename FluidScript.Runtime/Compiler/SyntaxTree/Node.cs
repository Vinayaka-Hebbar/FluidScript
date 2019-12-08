using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Node
    {
        static readonly IEnumerable<Node> emptyNodes = Enumerable.Empty<Node>();

        protected Node() { }

        public virtual IEnumerable<Node> ChildNodes() => emptyNodes;

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

        protected static IEnumerable<Node> Childs(params Node[] values)
        {
            return values;
        }

        public static void Iterate<TSource>(IEnumerable<TSource> sources, System.Action<TSource> callback) where TSource : Node
        {
            foreach (var item in sources)
            {
                callback(item);
            }
        }

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
