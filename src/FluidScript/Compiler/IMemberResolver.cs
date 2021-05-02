using FluidScript.Compiler.Binders;
using FluidScript.Compiler.SyntaxTree;
using System.Reflection;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Member resolver if fails to find
    /// </summary>
    public interface IMemberResolver
    {
        /// <summary>
        /// this will call if compiler fails to find member
        /// </summary>
        /// <param name="node">node require to resolve</param>
        /// <returns>Binder to member</returns>
        Runtime.IMemberBinder Resolve(NameExpression node);

        /// <summary>
        /// this will call if compiler fails to find member
        /// </summary>
        /// <param name="node">node require to resolve</param>
        /// <returns>Binder to member</returns>
        Runtime.IMemberBinder Resolve(MemberExpression node);
        /// <summary>
        /// this will call if compiler fails to find method
        /// </summary>
        /// <param name="node">node required to resolve</param>
        /// <param name="name">name of the method</param>
        /// <param name="obj">instance for the method</param>
        /// <param name="args">arguments for the call</param>
        /// <returns>MethodInfo for the call</returns>
        MethodInfo Resolve(InvocationExpression node, string name, object obj, object[] args);
    }

    internal
#if LATEST_VS
        readonly
#endif
        struct DefaultResolver : IMemberResolver
    {
        internal static readonly IMemberResolver Instance = default(DefaultResolver);

        public Runtime.IMemberBinder Resolve(NameExpression node)
        {
            return MemberBinder.Empty;
        }

        public Runtime.IMemberBinder Resolve(MemberExpression node)
        {
            return MemberBinder.Empty;
        }

        public MethodInfo Resolve(InvocationExpression node, string name, object obj, object[] args)
        {
            throw ExecutionException.ThrowMissingMethod(obj.GetType(), name, node);
        }
    }
}
