using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScript.Extensions
{
    public static class NodeExtensions
    {
        /// <summary>
        /// Contains a specified node <typeparamref name="T"/>
        /// </summary>
        public static bool ContainsNodeOfType<T>(this Node node) where T : Node
        {
            if (node is T)
                return true;
            foreach (var child in node.ChildNodes())
            {
                if (ContainsNodeOfType<T>(child))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Contains a specified node <typeparamref name="T"/>
        /// </summary>
        public static bool ContainsNodeOfType<T>(this Node node, Predicate<T> predicate) where T : Node
        {
            if (node is T && predicate((T)node))
                return true;
            foreach (var child in node.ChildNodes())
            {
                if (ContainsNodeOfType<T>(child))
                    return true;
            }
            return false;
        }
    }
}
