namespace FluidScript
{
    public sealed class OperationContextWrapper : OperationContext, IOperationContext
    {
        private readonly IOperationContext _context;

        internal OperationContextWrapper() : base(Inbuilt)
        {
            _context = Inbuilt;
            ReadOnlyContext = Inbuilt;
        }

        internal OperationContextWrapper(System.Collections.Generic.IEqualityComparer<string> comparer) : base(Inbuilt, comparer)
        {
            _context = Inbuilt;
            ReadOnlyContext = Inbuilt;
        }

        public OperationContextWrapper(IOperationContext context) : base(context)
        {
            _context = context;
            ReadOnlyContext = context.ReadOnlyContext;
        }

        public override Object this[string name]
        {
            get => Variables[name]; set
            {
                if (_context.Variables.ContainsKey(name))
                    _context[name] = value;
                Variables[name] = value;
            }
        }

        public override IReadOnlyOperationContext ReadOnlyContext { get; }

        public Object GetVariable(string name)
        {
            return Variables[name];
        }
    }
}
