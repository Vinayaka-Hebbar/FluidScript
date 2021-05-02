using System;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    public class LamdaGen
    {
        internal static readonly Type ObjectArray = typeof(object[]);
        internal static readonly Type[] CtorSignature = new Type[] { ObjectArray };
        internal const TypeAttributes Attributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass;

        internal LamdaGen(Generators.TypeGenerator type, MethodBuilder method)
        {
            Type = type;
            Method = method;
        }

        public Generators.TypeGenerator Type { get; }
        public MethodBuilder Method { get; }
        public FieldInfo Values { get; internal set; }

        public Type CreateType()
        {
            return Type.CreateType();
        }

        public ConstructorInfo Constructor { get; internal set; }


    }
}
