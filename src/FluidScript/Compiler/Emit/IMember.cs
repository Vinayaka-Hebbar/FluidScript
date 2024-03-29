﻿namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Member Generate
    /// </summary>
    public interface IMember
    {
        string Name { get; }
        /// <summary>
        /// Member Builder
        /// </summary>
        System.Reflection.MemberInfo MemberInfo { get; }

        System.Type DeclaringType { get; }

        System.Reflection.MemberTypes MemberType { get; }

        object[] GetCustomAttributes(System.Type attributeType, bool inherit);

        bool IsStatic { get; }

        bool IsPublic { get; }

        bool IsSpecialName { get; }

        void Compile();
    }
}