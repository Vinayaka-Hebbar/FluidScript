using System;
using System.Reflection;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.Reflection
{
    internal sealed class DeclaredField : DeclaredMember
    {
        public System.Reflection.Emit.FieldBuilder Store;
        public DeclaredField(SyntaxTree.Declaration declaration, int index, BindingFlags binding) : base(declaration, index, binding, System.Reflection.MemberTypes.Field)
        {
        }

        public override MemberInfo Info => Store;

        public override Type ResolvedType
        {
            get
            {
                if (Store == null)
                    return null;
                return Store.FieldType;
            }
        }

        internal override void Generate(ILGenerator generator, MethodOptimizationInfo info)
        {
            if (ValueAtTop != null)
            {
                generator.LoadArgument(0);
                ValueAtTop.GenerateCode(generator, info);
                generator.StoreField(Store);
            }
            IsGenerated = true;
        }
    }
}
