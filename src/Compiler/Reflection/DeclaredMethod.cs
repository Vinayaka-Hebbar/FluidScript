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

        public override MemberInfo Memeber => Store;

        internal override void Generate(Emit.TypeProvider typeProvider)
        {
            if (ValueAtTop != null)
            {
                var generator = new Emit.ReflectionILGenerator(Store.GetILGenerator(), false);
                var info = new Emit.OptimizationInfo(typeProvider)
                {
                    SyntaxTree = ValueAtTop,
                    FunctionName = Name,
                    ReturnType = Store.ReturnType
                };
                ValueAtTop.GenerateCode(generator, info);
                generator.Complete();
            }
        }
    }
}
