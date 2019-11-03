using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Emit
{
    public static class TypeUtils
    {

        internal static readonly IDictionary<string, Primitive> PrimitiveNames;
        internal static readonly IDictionary<System.Type, PrimitiveType> PrimitiveTypes;

        /// <summary>
        /// Must be primitive
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Primitive From(string typeName)
        {
            if (PrimitiveNames.ContainsKey(typeName))
                return PrimitiveNames[typeName];
            return Primitive.Any;
        }

        static TypeUtils()
        {
            PrimitiveNames = new Dictionary<string, Primitive>
            {
                {"byte", new Primitive(typeof(byte), PrimitiveType.UByte) },
                {"sbyte", new Primitive(typeof(sbyte), PrimitiveType.Byte) },
                {"short", new Primitive(typeof(short), PrimitiveType.Int16) },
                {"ushort", new Primitive(typeof(ushort), PrimitiveType.UInt16) },
                {"int",new Primitive ( typeof(int) , PrimitiveType.Int32)},
                {"uint",new Primitive(typeof(uint), PrimitiveType.UInt32)},
                {"long", new Primitive(typeof(long) , PrimitiveType.Int64)},
                {"ulong", new Primitive(typeof(ulong), PrimitiveType.UInt64) },
                {"float", new Primitive(typeof(float), PrimitiveType.Float) },
                {"double",new Primitive( typeof(double), PrimitiveType.Double) },
                {"bool", new Primitive(typeof(bool) , PrimitiveType.Bool)},
                {"string", new Primitive(typeof(string), PrimitiveType.String) },
                {"char", new Primitive(typeof(char), PrimitiveType.Char) },
                {"object", new Primitive(typeof(object), PrimitiveType.Any) }
            };
            PrimitiveTypes = PrimitiveNames
                .Select(item => item.Value)
                .ToDictionary(element => element.Type, element => element.Enum);
        }

        internal static Type GetType(Emit.TypeName name)
        {
            if (name.FullName == null)
                return null;
            if (PrimitiveNames.ContainsKey(name.FullName))
            {
                if (name.IsArray())
                    PrimitiveNames[name.FullName].Type.MakeArrayType();
                return PrimitiveNames[name.FullName].Type;
            }
            System.Type type = System.Type.GetType(name.FullName);
            if (name.IsArray())
                return type.MakeArrayType();
            return type;
        }

        internal static Type GetPrimitive(string typeName)
        {
            return PrimitiveNames[typeName].Type;
        }

        internal static void ConvertToPrimitive(ILGenerator generator, Type targetType)
        {
            var primitive = PrimitiveTypes[targetType];
            EmitConvertion.ToPrimitive(generator, primitive);
        }

        public static Type ToType(PrimitiveType type)
        {
            var primitive = PrimitiveNames.Values.FirstOrDefault(item => item.Enum == type);
            if (primitive.Type != null)
                return primitive.Type;
            return typeof(object);
        }

        internal static bool IsPrimitiveType(string typeName)
        {
            return PrimitiveNames.ContainsKey(typeName);
        }

        public static bool CheckType(PrimitiveType leftType, PrimitiveType expected)
        {
            return (leftType & expected) == expected;
        }

        public static bool IsValueType(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Any:
                case PrimitiveType.Null:
                case PrimitiveType.Array:
                    return false;
                default:
                    return true;
            }
        }
    }
}
