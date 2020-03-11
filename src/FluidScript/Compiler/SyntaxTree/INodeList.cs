using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public interface INodeList<TNode> : IEnumerable<TNode> where TNode : Node
    {
        TNode this[int index] { get; }
        int Length { get; }
        TElement[] Map<TElement>(System.Func<TNode, TElement> predicate);
        void ForEach(System.Action<TNode> selector);
        void CopyTo(System.Array array, int index);
        TNode[] ToArray();
    }
}
