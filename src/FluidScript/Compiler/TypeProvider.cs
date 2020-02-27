using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Resolve type
    /// </summary>
    public interface ITypeProvider
    {
        /// <summary>
        /// Get resolved <see cref="System.Type"/>
        /// </summary>
        System.Type GetType(TypeName name);
    }

    public sealed class TypeProvider : ITypeProvider
    {
        internal static readonly System.Type BooleanType = typeof(Boolean);
        internal static readonly System.Type FSType = typeof(FSObject);
        internal static readonly System.Type ObjectType = typeof(object);

        internal static readonly Primitive[] Inbuilts;
        private static readonly System.Collections.Generic.IDictionary<string, Primitive> InbuiltMap;

        internal static readonly ITypeProvider Default;

        static TypeProvider()
        {
            Default = new TypeProvider();

            Inbuilts = new Primitive[]
            {
                new Primitive("byte", typeof(Byte)),
                new Primitive("short", typeof(Short)),
                new Primitive("int", typeof(Integer)),
                new Primitive("long", typeof(Long)),
                new Primitive("float", typeof(Float)),
                new Primitive("double", typeof(Double)),
                new Primitive( "bool", BooleanType),
                new Primitive("char", typeof(Char)),
                new Primitive("string", typeof(String)),
                new Primitive("any", typeof(IFSObject)),
                new Primitive("void", typeof(void))
            };
            InbuiltMap = System.Linq.Enumerable.ToDictionary(Inbuilts, item => item.Name);
        }

        public static System.Type GetType(string typeName)
        {
            if (InbuiltMap.ContainsKey(typeName))
                return InbuiltMap[typeName].Type;
            return System.Type.GetType(typeName);
        }

        internal static bool IsInbuiltType(string typeName)
        {
            return InbuiltMap.ContainsKey(typeName);
        }

        public System.Type GetType(TypeName name)
        {
            string fullName = name.FullName;
            return GetType(fullName);
        }
    }
}
