﻿using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public static class TypeUtils
    {
        private static readonly IList<InbuiltType> Inbuilts;
        private static readonly IDictionary<string, InbuiltType> InbuiltNames;
        private static readonly IDictionary<string, RuntimeType> RuntimeTypes;

        /// <summary>
        /// Must be primitive
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static InbuiltType From(string typeName)
        {
            if (InbuiltNames.ContainsKey(typeName))
                return InbuiltNames[typeName];
            return InbuiltType.Any;
        }

        static TypeUtils()
        {
            Inbuilts = new List<InbuiltType>
            {
                new InbuiltType("byte", typeof(byte), RuntimeType.UByte),
                new InbuiltType("sbyte", typeof(sbyte), RuntimeType.Byte),
                new InbuiltType("short", typeof(short), RuntimeType.Int16),
                new InbuiltType("ushort", typeof(ushort), RuntimeType.UInt16) ,
                new InbuiltType ("int", typeof(int), RuntimeType.Int32),
                new InbuiltType("uint", typeof(uint), RuntimeType.UInt32),
                new InbuiltType("long", typeof(long), RuntimeType.Int64),
                new InbuiltType("ulong", typeof(ulong), RuntimeType.UInt64) ,
                new InbuiltType("float", typeof(float), RuntimeType.Float) ,
                new InbuiltType("double", typeof(double), RuntimeType.Double) ,
                new InbuiltType("bool", typeof(bool), RuntimeType.Bool),
                new InbuiltType("string", typeof(string), RuntimeType.String) ,
                new InbuiltType("char", typeof(char), RuntimeType.Char) ,
                new InbuiltType("any", typeof(object), RuntimeType.Any) ,
                new InbuiltType("void", typeof(void), RuntimeType.Void)
            };
            InbuiltNames = Inbuilts.ToDictionary(item => item.Name);
            RuntimeTypes = Inbuilts.ToDictionary(item => item.Type.FullName, item => item.Runtime);
        }

        internal static bool HasType(string name)
        {
            return InbuiltNames.ContainsKey(name);
        }

        internal static ITypeInfo GetTypeInfo(RuntimeType runtime)
        {
            var inbuilt = Inbuilts.FirstOrDefault(item => item.Runtime == runtime);
            if (inbuilt.Type != null)
            {
                var type = inbuilt.Type;
                ITypeInfo info = new TypeInfo(type.FullName, runtime);

                if ((runtime & RuntimeType.Array) == RuntimeType.Array)
                {
                    info = new ArrayTypeInfo(type.FullName, info, 1, runtime);
                }
                return info;
            }
            return TypeInfo.Any;
        }

        internal static ITypeInfo GetTypeInfo(System.Type type)
        {
            RuntimeType runtime = RuntimeType.Any;
            if (type.IsPrimitive)
            {
                runtime |= RuntimeTypes[type.FullName];
            }
            else if (type == typeof(string))
            {
                runtime |= RuntimeType.String;
            }
            else if (type == typeof(void))
            {
                runtime |= RuntimeType.Void;
            }
            if (type.IsArray)
            {
                //If array
                var rank = type.GetArrayRank();
                type = type.GetElementType();
                var info = GetTypeInfo(type);
                runtime |= RuntimeType.Array;
                return new ArrayTypeInfo(type.FullName, info, rank, runtime);
            }
            return new TypeInfo(type.FullName, runtime);
        }

        internal static System.Type GetType(string typeName)
        {
            if (InbuiltNames.ContainsKey(typeName))
                return InbuiltNames[typeName].Type;
            return System.Type.GetType(typeName);
        }

        internal static System.Type GetType(ITypeInfo info)
        {
            if (info.FullName == null)
                return null;
            if (InbuiltNames.ContainsKey(info.FullName))
            {
                if (info.IsArray())
                    return InbuiltNames[info.FullName].Type.MakeArrayType();
                return InbuiltNames[info.FullName].Type;
            }
            System.Type type = System.Type.GetType(info.FullName);
            if (info.IsArray())
                return type.MakeArrayType();
            return type;
        }

        internal static RuntimeType GetRuntimeType(ITypeInfo info)
        {
            RuntimeType type = RuntimeType.Any;
            if (info.FullName == null)
                return type;
            if (InbuiltNames.ContainsKey(info.FullName))
            {
                type = InbuiltNames[info.FullName].Runtime;
                if (info.IsArray())
                {
                    type |= RuntimeType.Array;
                }
            }
            return type;
        }

        internal static System.Type GetInbuiltType(string typeName)
        {
            return InbuiltNames[typeName].Type;
        }

        internal static RuntimeType GetRuntimeType(System.Type type)
        {
            if (RuntimeTypes.ContainsKey(type.FullName))
                return RuntimeTypes[type.FullName];
            return RuntimeType.Any;
        }

        internal static void ConvertToPrimitive(ILGenerator generator, System.Type targetType)
        {
            var primitive = RuntimeTypes[targetType.FullName];
            EmitConvertion.ToPrimitive(generator, primitive);
        }

        public static System.Type ToType(RuntimeType type)
        {
            var primitive = Inbuilts.FirstOrDefault(item => item.Runtime == type);
            if (primitive.Type != null)
                return primitive.Type;
            return typeof(object);
        }

        internal static bool IsInbuiltType(string typeName)
        {
            return InbuiltNames.ContainsKey(typeName);
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
                primitive |= RuntimeTypes[type.FullName];
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

        internal static bool TypesEqual(ParameterInfo[] args, RuntimeType[] calledTypes)
        {
            int length = args.Length;
            if (calledTypes.Length < length) return false;

            bool isEquals = true;
            for (int i = 0; i < calledTypes.Length; i++)
            {
                if (i >= length)
                {
                    isEquals = false;
                    break;
                }
                var arg = args[i];
                RuntimeType runtime = arg.Type.RuntimeType;
                if (arg.IsVar && (calledTypes[i] & runtime) == runtime)
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