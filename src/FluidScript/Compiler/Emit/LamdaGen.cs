using FluidScript.Extensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    public class LamdaGen
    {
        internal static readonly Type ObjectArray = typeof(object[]);
        static readonly Type[] _CtorSignature = new Type[] { ObjectArray };

        private LamdaGen(TypeBuilder type, MethodBuilder method)
        {
            Type = type;
            Method = method;
        }

        public TypeBuilder Type { get; }
        public MethodBuilder Method { get; }
        public FieldInfo Values { get; private set; }

        public Type CreateType()
        {
#if NETFRAMEWORK || MONOANDROID
            return Type.CreateType();
#else
            return Type.CreateTypeInfo();
#endif
        }

        public ConstructorInfo Constructor { get; private set; }

        /// <summary>
        /// Define Anonymous class
        /// </summary>
        /// <param name="types">Ctor types</param>
        /// <param name="returnType">Return Type of Lamda</param>
        /// <returns>Type builder</returns>
        public static LamdaGen DefineAnonymousMethod(Type[] types, Type returnType)
        {
            TypeBuilder builder = AssemblyGen.DynamicAssembly
                .DefineDynamicType("DisplayClass_" + types.Length, typeof(object), TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass);
            var values = builder.DefineField("Values", ObjectArray, FieldAttributes.Private);
            ConstructorBuilder ctor = builder.DefineConstructor(DelegateGen.CtorAttributes, CallingConventions.Standard, _CtorSignature);
            var method = builder.DefineMethod("Invoke", MethodAttributes.HideBySig, CallingConventions.Standard, returnType, types);
            var iLGen = ctor.GetILGenerator();
            iLGen.Emit(OpCodes.Ldarg_0);
            iLGen.Emit(OpCodes.Call, typeof(object).GetInstanceCtor());
            iLGen.Emit(OpCodes.Ldarg_0);
            iLGen.Emit(OpCodes.Ldarg_1);
            iLGen.Emit(OpCodes.Stfld, values);
            iLGen.Emit(OpCodes.Ret);

            // Values = values;
            return new LamdaGen(builder, method)
            {
                Constructor = ctor,
                Values = values
            };
        }
    }
}
