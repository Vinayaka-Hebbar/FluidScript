using FluidScript.Compiler.Emit;
using System;

namespace FluidScript.Compiler
{
    /// <summary>
    /// Resolve type
    /// </summary>
    public interface ITypeProvider
    {
        /// <summary>
        /// Get resolved <see cref="Type"/>
        /// </summary>
        Type GetType(TypeName name);
    }

    public sealed class TypeProvider : ITypeProvider
    {
        internal static readonly Type FSType;
        internal static readonly Type ObjectType;

        internal static readonly Type DoubleType;
        internal static readonly Type FloatType;
        internal static readonly Type LongType;
        internal static readonly Type IntType;
        internal static readonly Type ShortType;
        internal static readonly Type ByteType;

        internal static readonly Type StringType;
        internal static readonly Type CharType;
        internal static readonly Type BooleanType;
        internal static readonly Type VoidType;

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
                new Primitive("any", ObjectType),
                new Primitive("void", VoidType)
            };
            InbuiltMap = System.Linq.Enumerable.ToDictionary(Inbuilts, item => item.Name);
        }

        public static Type GetType(string typeName)
        {
            if (InbuiltMap.ContainsKey(typeName))
                return InbuiltMap[typeName].Type;
            var type = Type.GetType(typeName, false);
            if (type != null)
                return type;
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                 type = item.GetType(typeName, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        internal static bool IsInbuiltType(string typeName)
        {
            return InbuiltMap.ContainsKey(typeName);
        }

        public Type GetType(TypeName name)
        {
            return GetType(name.FullName);
        }

        /// <summary>
        /// UnderlyingSystemType for <paramref name="typeCode"/>
        /// </summary>
        public static Type Find(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return BooleanType;
                case TypeCode.Char:
                    return CharType;
                case TypeCode.SByte:
                    return ByteType;
                case TypeCode.Int16:
                    return ShortType;
                case TypeCode.Int32:
                    return IntType;
                case TypeCode.Int64:
                    return LongType;
                case TypeCode.Single:
                    return FloatType;
                case TypeCode.Double:
                    return DoubleType;
                case TypeCode.String:
                    return StringType;
                default:
                    return ObjectType;
            }
        }
    }
}
