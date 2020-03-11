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
        internal static readonly System.Type FSType;
        internal static readonly System.Type ObjectType;

        internal static readonly System.Type DoubleType;
        internal static readonly System.Type FloatType;
        internal static readonly System.Type LongType;
        internal static readonly System.Type IntType;
        internal static readonly System.Type ShortType;
        internal static readonly System.Type ByteType;

        internal static readonly System.Type StringType;
        internal static readonly System.Type CharType;
        internal static readonly System.Type BooleanType;
        internal static readonly System.Type VoidType;

        internal static readonly Primitive[] Inbuilts;
        private static readonly System.Collections.Generic.IDictionary<string, Primitive> InbuiltMap;

        internal static readonly ITypeProvider Default;

        static TypeProvider()
        {
            Default = new TypeProvider();

            FSType = typeof(FSObject);
            ObjectType = typeof(object);
            // Primitives
            DoubleType = typeof(Double);
            FloatType = typeof(Float);
            LongType = typeof(Long);
            IntType = typeof(Integer);
            ShortType = typeof(Short);
            ByteType = typeof(Byte);
            StringType = typeof(String);
            CharType = typeof(Char);
            BooleanType = typeof(Boolean);
            VoidType = typeof(void);

            Inbuilts = new Primitive[]
            {
                new Primitive("byte", ByteType),
                new Primitive("short", ShortType),
                new Primitive("int", IntType),
                new Primitive("long", LongType),
                new Primitive("float", FloatType),
                new Primitive("double", DoubleType),
                new Primitive("bool", BooleanType),
                new Primitive("char", CharType),
                new Primitive("string", StringType),
                new Primitive("any", typeof(IFSObject)),
                new Primitive("void", VoidType)
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
