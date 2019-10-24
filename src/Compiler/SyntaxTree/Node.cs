using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public abstract class Node
    {
        static readonly IEnumerable<Node> emptyNodes = Enumerable.Empty<Node>();

        protected Node(NodeType nodeType)
        {
            NodeType = nodeType;
        }

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

        public abstract void GenerateCode(ILGenerator generator, OptimizationInfo info);

        public NodeType NodeType { get; }

        public abstract Object GetValue();

        public abstract TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor) where TReturn : IRuntimeObject;
    }
}
