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


        public readonly Node[] NodeTree;

        public readonly object Target;

        public readonly string Value;

        public readonly int Reason;

        private ExecutionException(object target, string value, int reason, Node[] tree)
        {
            NodeTree = tree;
            Target = target;
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
                        return Target == null
                             ? string.Concat("cannot find method '", Value, "' from null")
                             : string.Concat("method '", Value, "' not found in ", Target.GetType().Name);
                    case MissingMember:
                        return Target == null
                           ? string.Concat("cannot find member '", Value, "' from null")
                           : string.Concat("member '", Value, "' not found in ", Target.GetType().Name);
                    case MissingIndexer:
                        return Target == null
                           ? string.Concat("cannot find '", Value, "' indexer from null")
                           : string.Concat("indexer '", Value, "' not found in ", Target.GetType().Name);
                    case InvalidOp:
                        return "Invalid operation";
                    case NullReference:
                        return "null reference error";
                    case ArgumentMisMatch:
                        return "argument mismatch";
                }
                return base.Message;
            }
        }

        public override string StackTrace => string.Join("\nin ", System.Linq.Enumerable.Select(NodeTree, node => node.ToString()));

        internal static void ThrowMissingMethod(object target, string name, params Node[] tree) => throw new ExecutionException(target, name, MissingMethod, tree);

        internal static void ThrowMissingMember(object target, string name, params Node[] tree) => throw new ExecutionException(target, name, MissingMember, tree);

        internal static void ThrowMissingIndexer(object target, string type, params Node[] tree) => throw new ExecutionException(target, type, MissingIndexer, tree);

        internal static void ThrowNullError(params Node[] tree) => throw new ExecutionException(null, null, NullReference, tree);

        internal static void ThrowInvalidOp(params Node[] tree) => throw new ExecutionException(null, null, InvalidOp, tree);

        internal static void ThrowArgumentMisMatch(params Node[] tree) => throw new ExecutionException(null, null, ArgumentMisMatch, tree);
    }
}
