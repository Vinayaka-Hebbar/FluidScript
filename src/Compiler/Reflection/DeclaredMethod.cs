using System;
using System.Reflection;

namespace FluidScript.Compiler.Reflection
{
    internal sealed class DeclaredMethod : DeclaredMember
    {
        public System.Reflection.Emit.MethodBuilder Store;

        public SyntaxTree.ArgumentInfo[] Arguments { get; set; }
        public DeclaredMethod(SyntaxTree.Declaration declaration, int index, BindingFlags binding) : base(declaration, index, binding, System.Reflection.MemberTypes.Method)
        {
        }

        public override MemberInfo Info
        {
            get
            {
                return Store;
            }
        }

        public override Type ResolvedType
        {
            get
            {
                if (Store == null)
                    return null;
                return Store.ReturnType;
            }
        }

        internal override void Generate(Emit.OptimizationInfo info)
        {
            if (ValueAtTop != null)
            {
                var generator = new Emit.ReflectionILGenerator(Store.GetILGenerator(), false);
                var methodInfo = new Emit.MethodOptimizationInfo(info)
                {
                    SyntaxTree = ValueAtTop,
                    FunctionName = Name,
                    ReturnType = Store.ReturnType
                };
                ValueAtTop.GenerateCode(generator, methodInfo);

                if (methodInfo.ReturnTarget != null)
                    generator.DefineLabelPosition(methodInfo.ReturnTarget);
                if (methodInfo.ReturnVariable != null)
                    generator.LoadVariable(methodInfo.ReturnVariable);
                generator.Complete();
            }
            IsGenerated = true;
        }
    }
}
