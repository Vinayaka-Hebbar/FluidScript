using System.Reflection;

namespace FluidScript.Extensions
{
    internal static class MemberExtensions
    {
        /// <summary>
        /// Matches generating argument 
        /// </summary>
        /// <param name="member">Member to check</param>
        /// <param name="name">Name to match</param>
        /// <param name="flags">Binding flag to match</param>
        /// <returns></returns>
        internal static bool IsEquals(this Compiler.Emit.IMember member, string name, BindingFlags flags)
        {
            if (Utils.ReflectionUtils.BindingFlagsMatch(member.IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
               && Utils.ReflectionUtils.BindingFlagsMatch(member.IsStatic, flags, BindingFlags.Static, BindingFlags.Instance))
            {
                var attrs = (System.Attribute[])member.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false);
                if (attrs.Length > 0)
                {
                    return attrs[0].Match(name);
                }
                else if (member.IsSpecialName)
                {
                    return member.Name.Equals(name);
                }
            }
            return false;
        }

        /// <summary>
        /// Matches generating argument 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="flags">Binding flags to match</param>
        /// <returns></returns>
        internal static bool BindingFlagsMatch(this Compiler.Emit.IMember member, BindingFlags flags)
        {
            return Utils.ReflectionUtils.BindingFlagsMatch(member.IsPublic, flags, BindingFlags.Public, BindingFlags.NonPublic)
               && Utils.ReflectionUtils.BindingFlagsMatch(member.IsStatic, flags, BindingFlags.Static, BindingFlags.Instance);
        }
    }
}
