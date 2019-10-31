using System;
using FluidScript.Compiler.Emit;

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

        public abstract System.Reflection.MemberInfo Memeber { get; }
        protected DeclaredMember(SyntaxTree.Declaration declaration, int index, BindingFlags binding, System.Reflection.MemberTypes memberType)
        {
            Index = index;
            Declaration = declaration;
            Binding = binding;
            MemberType = memberType;
        }

        internal virtual void Generate(TypeProvider typeProvider)
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
        Function = 5,
    }

    
}
