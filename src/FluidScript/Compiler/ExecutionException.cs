using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public sealed class ExecutionException : System.SystemException
    {
        const int MissingMethod = 1;
        const int MissingMember = 2;
        const int NullReference = 3;
        const int InvalidOp = 4;
        const int MissingIndexer = 6;
        const int ArgumentMisMatch = 7;
        const int NotSupported = 8;
        const int InvalidCast = 9;

        public readonly Node[] NodeTree;

        public readonly int Reason;

        private ExecutionException(int reason, Node[] tree, string message) : base(message)
        {
            NodeTree = tree;
            Reason = reason;
        }

        public override string StackTrace => string.Join("\nin ", System.Linq.Enumerable.Select(NodeTree, node => node.ToString()));

        internal static void ThrowNotSupported(params Node[] tree)
        {
           throw new ExecutionException(NotSupported, tree, "Not suppored");
        }

        internal static System.Exception ThrowMissingMethod(System.Type target, string name, params Node[] tree)
        {
            throw new ExecutionException(MissingMethod, tree, target == null
                             ? string.Concat("cannot find method '", name, "' from null")
                             : string.Concat("method '", name, "' not found in ", target.Name));
        }

        internal static System.Exception ThrowMissingMember(System.Type target, string name, params Node[] tree)
        {
            throw new ExecutionException(MissingMember, tree, target == null
                           ? string.Concat("cannot find member '", name, "' from null")
                           : string.Concat("member '", name, "' not found in ", target.Name));
        }

        internal static System.Exception ThrowMissingIndexer(System.Type target, string type, params Node[] tree)
        {
            throw new ExecutionException( MissingIndexer, tree, target == null
                           ? string.Concat("cannot find '", type, "' indexer from null")
                           : string.Concat("indexer '", type, "' not found in ", target.Name));
        }

        internal static System.Exception ThrowNullError(params Node[] tree) => throw new ExecutionException(NullReference, tree, "Null reference error");

        internal static System.Exception ThrowInvalidOp(params Node[] tree) => throw new ExecutionException(InvalidOp, tree, "Invalid operation");

        internal static System.Exception ThrowArgumentMisMatch(params Node[] tree) => throw new ExecutionException(ArgumentMisMatch, tree, "Argument mismatch");

        internal static System.Exception ThrowInvalidCast(System.Type type, params Node[] tree) => throw new ExecutionException(InvalidCast, tree, "Invalid cast to " + type);
    }
}
