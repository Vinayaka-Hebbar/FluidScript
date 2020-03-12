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

        public readonly System.Type Type;

        public readonly string Value;

        public readonly int Reason;

        private ExecutionException(System.Type type, string value, int reason, Node[] tree)
        {
            NodeTree = tree;
            Type = type;
            Value = value;
            Reason = reason;
        }

        public override string Message
        {
            get
            {
                switch (Reason)
                {
                    case MissingMethod:
                        return Type == null
                             ? string.Concat("cannot find method '", Value, "' from null")
                             : string.Concat("method '", Value, "' not found in ", Type.Name);
                    case MissingMember:
                        return Type == null
                           ? string.Concat("cannot find member '", Value, "' from null")
                           : string.Concat("member '", Value, "' not found in ", Type.Name);
                    case MissingIndexer:
                        return Type == null
                           ? string.Concat("cannot find '", Value, "' indexer from null")
                           : string.Concat("indexer '", Value, "' not found in ", Type.Name);
                    case InvalidOp:
                        return "Invalid operation";
                    case NullReference:
                        return "Null reference error";
                    case ArgumentMisMatch:
                        return "Argument mismatch";
                    case NotSupported:
                        return "Not suppored";
                    case InvalidCast:
                        return string.Concat("Invalid cast to ", Type);
                }
                return base.Message;
            }
        }

        internal static void ThrowNotSupported(params Node[] tree) => throw new ExecutionException(null, null, NotSupported, tree);

        public override string StackTrace => string.Join("\nin ", System.Linq.Enumerable.Select(NodeTree, node => node.ToString()));

        internal static void ThrowMissingMethod(System.Type target, string name, params Node[] tree) => throw new ExecutionException(target, name, MissingMethod, tree);

        internal static void ThrowMissingMember(System.Type target, string name, params Node[] tree) => throw new ExecutionException(target, name, MissingMember, tree);

        internal static void ThrowMissingIndexer(System.Type target, string type, params Node[] tree) => throw new ExecutionException(target, type, MissingIndexer, tree);

        internal static void ThrowNullError(params Node[] tree) => throw new ExecutionException(null, null, NullReference, tree);

        internal static void ThrowInvalidOp(params Node[] tree) => throw new ExecutionException(null, null, InvalidOp, tree);

        internal static void ThrowArgumentMisMatch(params Node[] tree) => throw new ExecutionException(null, null, ArgumentMisMatch, tree);

        internal static void ThrowInvalidCast(System.Type type, params Node[] tree) => throw new ExecutionException(null, null, InvalidCast, tree);
    }
}
