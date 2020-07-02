using FluidScript.Compiler;
using FluidScript.Extensions;
using System.Reflection;

namespace FluidScript.Utils
{
    public static class ReflectionHelpers
    {

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
                    m_register_ctor = typeof(Runtime.RegisterAttribute).GetInstanceCtor(typeof(string));
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
                    m_booleanToBool = TypeProvider.BooleanType.GetStaticMethod(ReflectionUtils.ImplicitConversionName, TypeProvider.BooleanType);
                return m_booleanToBool;
            }
        }

        private static MethodInfo m_intergerToInt32;
        internal static MethodInfo IntegerToInt32
        {
            get
            {
                if (m_intergerToInt32 == null)
                    m_intergerToInt32 = TypeProvider.IntType.GetImplicitConversion(ReflectionUtils.ImplicitConversionName, typeof(int), TypeProvider.IntType);
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
                    m_logicalAnd = TypeProvider.BooleanType.GetStaticMethod("OpLogicalAnd", TypeProvider.BooleanType, TypeProvider.BooleanType);
                return m_logicalAnd;
            }
        }

        private static MethodInfo m_logicalOr;
        internal static MethodInfo LogicalOr
        {
            get
            {
                if (m_logicalOr == null)
                    m_logicalOr = TypeProvider.BooleanType.GetStaticMethod( "OpLogicalOr", TypeProvider.BooleanType, TypeProvider.BooleanType);
                return m_logicalOr;
            }
        }

        private static MethodInfo m_toBoolean;
        internal static MethodInfo ToBoolean
        {
            get
            {
                if (m_toBoolean == null)
                    m_toBoolean = typeof(FSConvert).GetStaticMethod(nameof(FSConvert.ToBoolean), TypeProvider.ObjectType);
                return m_toBoolean;
            }
        }

        private static MethodInfo m_mathPow;
        internal static MethodInfo MathPow
        {
            get
            {
                if (m_mathPow == null)
                    m_mathPow = typeof(Math).GetStaticMethod(nameof(Math.Pow), TypeProvider.DoubleType, TypeProvider.DoubleType);
                return m_mathPow;
            }
        }

        private static MethodInfo m_isEquals;
        internal static MethodInfo IsEquals
        {
            get
            {
                if (m_isEquals == null)
                    m_isEquals = TypeProvider.FSType.GetStaticMethod(nameof(FSObject.IsEquals), TypeProvider.ObjectType, TypeProvider.ObjectType);
                return m_isEquals;
            }
        }

        private static MethodInfo m_logicalNot;
        internal static MethodInfo LogicalNot
        {
            get
            {
                if (m_logicalNot == null)
                    m_logicalNot = TypeProvider.BooleanType.GetStaticMethod("op_LogicalNot", TypeProvider.BooleanType);
                return m_logicalNot;
            }
        }

        static MethodInfo m_toAny;
        internal static MethodInfo ToAny
        {
            get
            {
                if (m_toAny == null)
                    m_toAny = typeof(FSConvert).GetStaticMethod(nameof(FSConvert.ToAny), TypeProvider.ObjectType);
                return m_toAny;
            }
        }

        #endregion

        #endregion

        static ReflectionHelpers()
        {
            Double_New = TypeProvider.DoubleType.GetInstanceCtor(typeof(double));
            Float_New = TypeProvider.FloatType.GetInstanceCtor(typeof(float));
            Long_New = TypeProvider.LongType.GetInstanceCtor(typeof(long));
            Integer_New = TypeProvider.IntType.GetInstanceCtor(typeof(int));
            Short_New = TypeProvider.ShortType.GetInstanceCtor(typeof(short));
            Byte_New = TypeProvider.ByteType.GetInstanceCtor(typeof(sbyte));
            Char_New = TypeProvider.CharType.GetInstanceCtor(typeof(char));
            String_New = TypeProvider.StringType.GetInstanceCtor(typeof(string));

            Bool_True = TypeProvider.BooleanType.GetField(nameof(Boolean.True), BindingFlags.Public | BindingFlags.Static);
            Bool_False = TypeProvider.BooleanType.GetField(nameof(Boolean.False), BindingFlags.Public | BindingFlags.Static);

        }

        
    }
}
