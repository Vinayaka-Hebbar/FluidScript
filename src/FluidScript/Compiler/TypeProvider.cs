using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;

namespace FluidScript.Compiler
{

    public class TypeProvider 
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

        internal static readonly Dictionary<string, Type> Inbuilts;

        static TypeProvider()
        {
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

            Inbuilts = new Dictionary<string, Type>()
            {
                {"any", ObjectType },
                {"object", ObjectType },
                {"void", VoidType },
                {"byte", ByteType },
                {"short", ShortType },
                {"int", IntType },
                {"long", LongType },
                {"float", FloatType },
                {"double", DoubleType },
                {"bool", BooleanType },
                {"char", CharType },
                {"string", StringType }
            };
        }

        public static Type GetType(TypeName typeName)
        {
            if (typeName.Namespace == null && Inbuilts.TryGetValue(typeName.Name, out Type t))
                return t;
            var type = Type.GetType(typeName.FullName, false);
            if (type != null)
                return type;
            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = item.GetType(typeName.FullName, false);
                if (type != null)
                    return type;
            }
            return null;
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
