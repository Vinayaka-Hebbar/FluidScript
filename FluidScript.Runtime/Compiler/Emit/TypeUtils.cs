using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Emit
{
    public static class TypeUtils
    {
        internal static readonly IDictionary<string, Primitive> PrimitiveNames;
        internal static readonly IDictionary<System.Type, RuntimeType> PrimitiveTypes;

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
                {"byte", new Primitive(typeof(byte), RuntimeType.UByte) },
                {"sbyte", new Primitive(typeof(sbyte), RuntimeType.Byte) },
                {"short", new Primitive(typeof(short), RuntimeType.Int16) },
                {"ushort", new Primitive(typeof(ushort), RuntimeType.UInt16) },
                {"int",new Primitive ( typeof(int) , RuntimeType.Int32)},
                {"uint",new Primitive(typeof(uint), RuntimeType.UInt32)},
                {"long", new Primitive(typeof(long) , RuntimeType.Int64)},
                {"ulong", new Primitive(typeof(ulong), RuntimeType.UInt64) },
                {"float", new Primitive(typeof(float), RuntimeType.Float) },
                {"double",new Primitive( typeof(double), RuntimeType.Double) },
                {"bool", new Primitive(typeof(bool) , RuntimeType.Bool)},
                {"string", new Primitive(typeof(string), RuntimeType.String) },
                {"char", new Primitive(typeof(char), RuntimeType.Char) },
                {"any", new Primitive(typeof(object), RuntimeType.Any) },
                {"void", new Primitive(typeof(void), RuntimeType.Void) }
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
                    return PrimitiveNames[name.FullName].Type.MakeArrayType();
                return PrimitiveNames[name.FullName].Type;
            }
            System.Type type = System.Type.GetType(name.FullName);
            if (name.IsArray())
                return type.MakeArrayType();
            return type;
        }

        internal static RuntimeType GetPrimitiveType(Emit.TypeName name)
        {
            RuntimeType type = RuntimeType.Any;
            if (name.FullName == null)
                return type;
            if (PrimitiveNames.ContainsKey(name.FullName))
            {
                type = PrimitiveNames[name.FullName].Enum;
                if (name.IsArray())
                {
                    type |= RuntimeType.Array;
                }
            }
            return type;
        }

        internal static Type GetPrimitiveType(string typeName)
        {
            return PrimitiveNames[typeName].Type;
        }

        internal static void ConvertToPrimitive(ILGenerator generator, Type targetType)
        {
            var primitive = PrimitiveTypes[targetType];
            EmitConvertion.ToPrimitive(generator, primitive);
        }

        public static Type ToType(RuntimeType type)
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

        internal static RuntimeType ToPrimitive(System.Type type)
        {
            RuntimeType primitive = RuntimeType.Any;
            if (type.IsArray)
            {
                type = type.GetElementType();
                primitive |= RuntimeType.Array;
            }
            if (type.IsPrimitive)
            {
                primitive |= PrimitiveTypes[type];
            }
            else if (type == typeof(string))
            {
                primitive |= RuntimeType.String;
            }
            else if (type == typeof(void))
            {
                primitive |= RuntimeType.Void;
            }
            return primitive;
        }

        public static bool CheckType(RuntimeType leftType, RuntimeType expected)
        {
            return (leftType & expected) == expected;
        }

        public static bool IsValueType(RuntimeType type)
        {
            switch (type)
            {
                case RuntimeType.Any:
                case RuntimeType.Undefined:
                case RuntimeType.Array:
                    return false;
                default:
                    return true;
            }
        }

        internal static bool TypesEqual(ArgumentType[] types, RuntimeType[] calledTypes)
        {
            int length = types.Length;
            if (calledTypes.Length < length) return false;

            bool isEquals = true;
            for (int i = 0; i < calledTypes.Length; i++)
            {
                if (i >= length)
                {
                    isEquals = false;
                    break;
                }
                var type = types[i];
                var runtime = type.RuntimeType;
                if (type.Flags == Reflection.ArgumentFlags.VarArgs && (calledTypes[i] & runtime) == runtime)
                {
                    break;
                }

                if ((calledTypes[i] & runtime) != runtime)
                {
                    isEquals = false;
                    break;
                }
            }
            return isEquals;
        }
    }
}
