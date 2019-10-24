namespace FluidScript.Compiler.Scopes
{
    public sealed class ScopedContext : System.IDisposable
    {
        private readonly SyntaxVisitor Visitor;
        private readonly Scope prevScope;
        private readonly Scope currentScope;

        public ScopedContext(SyntaxVisitor visitor, Scope scope)
        {
            Visitor = visitor;
            currentScope = scope;
            prevScope = visitor.Scope;
            visitor.Scope = currentScope;
        }

        public void Dispose()
        {
            Visitor.Scope = prevScope;
        }
    }
}
