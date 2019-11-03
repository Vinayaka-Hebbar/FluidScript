namespace FluidScript.Compiler.Scopes
{
    public class ScopeSubscription : System.IDisposable
    {
        public readonly System.Action OnComplete;

        public ScopeSubscription(System.Action onComplete)
        {
            OnComplete = onComplete;
        }

        public void Dispose()
        {
            OnComplete();
        }
    }
}
