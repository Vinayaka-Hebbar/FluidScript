using System;

namespace FluidScript.Compiler.Reflection
{
    /// <summary>
    /// A method declared 
    /// </summary>
    public abstract class DeclaredMember
    {
        public string Name => Declaration.Name;


        public SyntaxTree.Declaration Declaration;
        public readonly int Index;
        public readonly BindingFlags Binding;
        public readonly System.Reflection.MemberTypes MemberType;
        /// <summary>
        /// May be field , property or method initailization
        /// </summary>
        public SyntaxTree.Statement ValueAtTop;

        public abstract System.Reflection.MemberInfo Info { get; }
        protected DeclaredMember(SyntaxTree.Declaration declaration, int index, BindingFlags binding, System.Reflection.MemberTypes memberType)
        {
            Index = index;
            Declaration = declaration;
            Binding = binding;
            MemberType = memberType;
        }

        public bool IsGenerated { get; protected set; }

        /// <summary>
        /// Either return type , property type or field type
        /// </summary>
        public abstract Type ResolvedType { get; }
        public bool IsField => MemberType == System.Reflection.MemberTypes.Field;

        public bool IsMethod => MemberType == System.Reflection.MemberTypes.Method;

        internal virtual void Generate(Emit.ILGenerator generator, Emit.MethodOptimizationInfo info)
        {
            throw new NotImplementedException();
        }
        internal virtual void Generate(Emit.OptimizationInfo info)
        {
            throw new NotImplementedException();
        }
    }

    public enum BindingFlags
    {
        None = 0,
        Instance = 1,
        Static = 2,
        Public = 4,
        Private = 8,
    }

    public enum MemberTypes
    {
        Field = 2,
        Property = 4,
        Function = 8,
        Type = 16
    }


}
