using System.Reflection;

namespace FluidScript.Extensions
{
    internal static class MemberExtensions
    {
        /// <summary>
        /// Matches generating argument 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static bool IsEquals(this Compiler.Emit.IMemberGenerator member, string name, BindingFlags flags)
        {
            if (Utils.TypeUtils.BindingFlagsMatch(member.IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
               && Utils.TypeUtils.BindingFlagsMatch(member.IsStatic, flags, BindingFlags.Static, BindingFlags.Instance))
            {
                var attrs = (System.Attribute[])member.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                if (attrs.Length > 0)
                {
                    return attrs[0].Match(name);
                }
            }
            return false;
        }

        /// <summary>
        /// Matches generating argument 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="name"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static bool BindingFlagsMatch(this Compiler.Emit.IMemberGenerator member, BindingFlags flags)
        {
            return Utils.TypeUtils.BindingFlagsMatch(member.IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
               && Utils.TypeUtils.BindingFlagsMatch(member.IsStatic, flags, BindingFlags.Static, BindingFlags.Instance);
        }
    }
}
