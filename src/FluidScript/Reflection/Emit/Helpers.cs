
using FluidScript.Library;
using System.Reflection;

namespace FluidScript.Reflection.Emit
{
    internal static class Helpers
    {
        private const string Separator = ", ";
        private const BindingFlags DeclaredStatic = DeclaredPublic | BindingFlags.Static;
        private const BindingFlags DeclaredInstance = DeclaredPublic | BindingFlags.Instance;
        private const BindingFlags DeclaredPublic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
        private const BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        #region Object
        internal static readonly MethodInfo ObjectEquals_Two_Object;
        internal static readonly MethodInfo Object_ToString;
        internal static readonly MethodInfo StringConcat_Two_String;
        internal static readonly MethodInfo StringConcat_Two_Object;

        public static readonly ConstructorInfo Double_New;
        public static readonly ConstructorInfo Float_New;
        public static readonly ConstructorInfo Long_New;
        public static readonly ConstructorInfo Integer_New;
        public static readonly ConstructorInfo Short_New;
        public static readonly ConstructorInfo Byte_New;
        public static readonly ConstructorInfo Char_New;
        public static readonly ConstructorInfo String_New;

        public static readonly FieldInfo Bool_True;
        public static readonly FieldInfo Bool_False;

        public static ConstructorInfo Register_Attr_Ctor;

        public static MethodInfo Integer_to_Int32;

        #region Implicit Calls
        public static MethodInfo Booolean_To_Bool;

        #endregion

        #endregion

        static Helpers()
        {
            ObjectEquals_Two_Object = GetInstanceMethod(typeof(object), "Equals", typeof(object));
            Object_ToString = GetInstanceMethod(typeof(object), "ToString");
            StringConcat_Two_String = GetStaticMethod(typeof(string), "Concat", typeof(string), typeof(string));
            StringConcat_Two_Object = GetStaticMethod(typeof(string), "Concat", typeof(object), typeof(object));

            Double_New = GetInstanceCtor(typeof(Double), typeof(double));
            Float_New = GetInstanceCtor(typeof(Float), typeof(float));
            Long_New = GetInstanceCtor(typeof(Long), typeof(long));
            Integer_New = GetInstanceCtor(typeof(Integer), typeof(int));
            Short_New = GetInstanceCtor(typeof(Short), typeof(short));
            Byte_New = GetInstanceCtor(typeof(Byte), typeof(sbyte));
            Char_New = GetInstanceCtor(typeof(Char), typeof(char));
            String_New = GetInstanceCtor(typeof(String), typeof(string));
            Register_Attr_Ctor = GetInstanceCtor(typeof(Runtime.RegisterAttribute), typeof(string));

            Integer_to_Int32 = GetStaticMethod(typeof(Integer), "op_Implicit", typeof(Integer));


            Bool_True = GetField(typeof(Boolean), nameof(Boolean.True), BindingFlags.Public | BindingFlags.Static);
            Bool_False = GetField(typeof(Boolean), nameof(Boolean.False), BindingFlags.Public | BindingFlags.Static);

            Booolean_To_Bool = GetStaticMethod(typeof(Boolean), Conversion.ImplicitConversionName, typeof(Boolean));
        }

        private static ConstructorInfo GetInstanceCtor(System.Type type, params System.Type[] parameterTypes)
        {
            var result = type.GetConstructor(DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the ctor {0}.ctor({1})", type.FullName, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static MethodInfo GetStaticMethod(System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredStatic, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the static method {0}.{1}({2})", type.FullName, name, StringHelpers.Join(Separator, parameterTypes)));
            return result;
        }

        public static FieldInfo GetField(System.Type type, string name, BindingFlags binding)
        {
            FieldInfo result = type.GetField(name, binding);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("the field {0}.{1}", type.FullName, name));
            return result;

        }

        public static MethodInfo GetInstanceMethod(System.Type type, string name, params System.Type[] parameterTypes)
        {
            MethodInfo result = type.GetMethod(name, DeclaredInstance, null, parameterTypes, null);
            if (result == null)
                throw new System.InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
            return result;
        }
    }
}
