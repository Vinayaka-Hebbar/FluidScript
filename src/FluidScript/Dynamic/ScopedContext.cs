using System;

namespace FluidScript.Dynamic
{
    internal
#if LATEST_VS
        readonly
#endif
        struct ScopedContext : IDisposable
    {
        private readonly DynamicContext _context;
        private readonly DynamicObject previous;
        private readonly DynamicObject current;

        internal ScopedContext(DynamicContext context)
        {
            _context = context;
            previous = context.Current;
            current = _context.Current = new DynamicObject(_context.Data);
        }

        public void Dispose()
        {
            current.Detach();
            _context.Current = previous;

        }
    }
}
