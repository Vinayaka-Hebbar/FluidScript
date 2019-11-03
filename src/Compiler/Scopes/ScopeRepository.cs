using System.Collections.Generic;

namespace FluidScript.Compiler.Scopes
{
    public static class ScopeRepository
    {
        public static Dictionary<System.Type, Scopes.ObjectScope> Scopes;

        /// <summary>
        /// Current generative type and scope
        /// </summary>
        /// <param name="type"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static System.IDisposable Add(System.Type type, Scopes.ObjectScope scope)
        {
            if (Scopes == null)
                Scopes = new Dictionary<System.Type, ObjectScope>();
            Scopes.Add(type, scope);
            return new ScopeSubscription(() =>
            {
                Scopes.Remove(type);
            });
        }
    }
}
