using FluidScript.Compiler;
using System.Reflection;

namespace FluidScript.Utils
{
    internal static class ReflectionHelpers
    {
        private const string Separator = ", ";
        private const BindingFlags DeclaredStatic = DeclaredPublic | BindingFlags.Static;
        private const BindingFlags DeclaredInstance = DeclaredPublic | BindingFlags.Instance;
        private const BindingFlags DeclaredPublic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;


        #region Members

        internal static readonly ConstructorInfo Double_New;
        internal static readonly ConstructorInfo Float_New;
        internal static readonly ConstructorInfo Long_New;
        internal static readonly ConstructorInfo Integer_New;
        internal static readonly ConstructorInfo Short_New;
        internal static readonly ConstructorInfo Byte_New;
        internal static readonly ConstructorInfo Char_New;
        internal static readonly ConstructorInfo String_New;

        internal static readonly FieldInfo Bool_True;
        internal static readonly FieldInfo Bool_False;

        #region Constructors

        static ConstructorInfo m_register_ctor;
        internal static ConstructorInfo Register_Attr_Ctor
        {
            get
            {
                if (m_register_ctor == null)
                    m_register_ctor = GetInstanceCtor(typeof(Runtime.RegisterAttribute), typeof(string));
                return m_register_ctor;
            }
        }
        #endregion

        #region Implicit Calls

        private static MethodInfo m_booleanToBool;
        internal static MethodInfo BoooleanToBool
        {
            get
            {
                if (m_booleanToBool == null)
                    m_booleanToBool = GetStaticMethod(TypeProvider.BooleanType, TypeUtils.ImplicitConversionName, TypeProvider.BooleanType);
                return m_booleanToBool;
            }
        }

        private static MethodInfo m_intergerToInt32;
        internal static MethodInfo IntegerToInt32
        {
            get
            {
                if (m_intergerToInt32 == null)
                    m_intergerToInt32 = GetImplicitConversion(TypeProvider.IntType, TypeUtils.ImplicitConversionName, typeof(int), TypeProvider.IntType);
                return m_intergerToInt32;
            }
        }

        #endregion

        #region Logical Convert

        private static MethodInfo m_logicalAnd;
        internal static MethodInfo LogicalAnd
        {
            get
            {
                if (m_logicalAnd == null)
                    m_logicalAnd = GetStaticMethod(TypeProvider.BooleanType, "OpLogicalAnd", TypeProvider.BooleanType, TypeProvider.BooleanType);
                return m_logicalAnd;
            }
        }

        private static MethodInfo m_logicalOr;
        internal static MethodInfo LogicalOr
        {
            get
            {
                if (m_logicalOr == null)
                    m_logicalOr = GetStaticMethod(TypeProvider.BooleanType, "OpLogicalOr", TypeProvider.BooleanType, TypeProvider.BooleanType);
                return m_logicalOr;
            }
        }

        private static MethodInfo m_toBoolean;
        internal static MethodInfo ToBoolean
        {
            get
            {
                if (m_toBoolean == null)
                    m_toBoolean = GetStaticMethod(typeof(FSConvert), nameof(FSConvert.ToBoolean), TypeProvider.ObjectType);
                return m_toBoolean;
            }
        }

        private static MethodInfo m_mathPow;
        internal static MethodInfo MathPow
        {
            get
            {
                if (m_mathPow == null)
                    m_mathPow = GetStaticMethod(typeof(Math), nameof(Math.Pow), TypeProvider.DoubleType, TypeProvider.DoubleType);
                return m_mathPow;
            }
        }

        private static MethodInfo m_isEquals;
        internal static MethodInfo IsEquals
        {
            get
            {
                if (m_isEquals == null)
                    m_isEquals = GetStaticMethod(TypeProvider.FSType, nameof(FSObject.IsEquals), TypeProvider.ObjectType, TypeProvider.ObjectType);
                return m_isEquals;
            }
        }

        private static MethodInfo m_logicalNot;
        internal static MethodInfo LogicalNot
        {
            get
            {
                if (m_logicalNot == null)
                    m_logicalNot = GetStaticMethod(TypeProvider.BooleanType, "op_LogicalNot", TypeProvider.BooleanType);
                return m_logicalNot;
            }
        }

        static MethodInfo m_toAny;
        internal static MethodInfo ToAny
        {
            get
            {
                if (m_toAny == null)
                    m_toAny = GetStaticMethod(typeof(FSConvert), nameof(FSConvert.ToAny), TypeProvider.ObjectType);
                return m_toAny;
            }
        }

        #endregion

        #endregion

        static ReflectionHelpers()
        {

            Double_New = GetInstanceCtor(TypeProvider.DoubleType, typeof(double));
            Float_New = GetInstanceCtor(TypeProvider.FloatType, typeof(float));
            Long_New = GetInstanceCtor(TypeProvider.LongType, typeof(long));
            Integer_New = GetInstanceCtor(TypeProvider.IntType, typeof(int));
            Short_New = GetInstanceCtor(TypeProvider.ShortType, typeof(short));
            Byte_New = GetInstanceCtor(TypeProvider.ByteType, typeof(sbyte));
            Char_New = GetInstanceCtor(TypeProvider.CharType, typeof(char));
            String_New = GetInstanceCtor(TypeProvider.StringType, typeof(string));

            Bool_True = GetField(TypeProvider.BooleanType, nameof(Boolean.True), BindingFlags.Public | BindingFlags.Static);
            Bool_False = GetField(TypeProvider.BooleanType, nameof(Boolean.False), BindingFlags.Public | BindingFlags.Static);

        }

        private static ConstructorInfo GetInstanceCtor(System.Type type, params System.Type[] parameterTypes)
        {
            var result = type.GetConstructor(DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the ctor {0}.ctor({1})", type.FullName, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        internal static MethodInfo GetStaticMethod(System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredStatic, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the static method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        internal static MethodInfo GetImplicitConversion(System.Type type, string name, System.Type returnType, params System.Type[] parameterTypes)
        {
            var results = type.GetMember(name, MemberTypes.Method, DeclaredStatic);
            foreach (MethodInfo method in results)
            {
                if (TypeUtils.MatchesArgumentTypes(method, parameterTypes) && TypeUtils.AreReferenceAssignable(method.ReturnType, returnType))
                {
                    return method;
                }
            }
            throw new System.InvalidOperationException(string.Format("the convertion method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
        }

        internal static FieldInfo GetField(System.Type type, string name, BindingFlags binding)
        {
            FieldInfo result = type.GetField(name, binding);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the field {0}.{1}", type.FullName, name));
            return result;

        }

        internal static MethodInfo GetInstanceMethod(System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
            return result;
        }
    }
}
