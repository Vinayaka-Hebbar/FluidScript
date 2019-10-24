using FluidScript.Compiler.Reflection;

namespace FluidScript.Compiler.Scopes
{
    public class MethodScope : Scope
    {
        private readonly MethodInfo method;
        public MethodScope(string name, SyntaxVisitor visitor, System.Reflection.MethodAttributes attributes = System.Reflection.MethodAttributes.Private) : base(visitor)
        {
            method = new MethodInfo(name, attributes)
            {
                DeclaredType = ParentScope.GetTypeInfo()
            };
        }

        protected override void Build()
        {
            var type = GetTypeInfo();
            type.DeclareMethod(method);
        }
    }
}
