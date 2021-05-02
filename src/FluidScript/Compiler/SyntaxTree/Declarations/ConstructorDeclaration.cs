using FluidScript.Compiler.Generators;
using FluidScript.Extensions;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ConstructorDeclaration : FunctionDeclaration
    {
        public ConstructorDeclaration(NodeList<TypeParameter> parameters, BlockStatement body) : base(string.Empty, parameters, null, body)
        {
        }

        public override void CreateMember(TypeGenerator generator)
        {
            var parameters = Parameters.Map(para => para.GetParameterInfo(generator.Context));
            var parameterTypes = parameters.Map(para => para.Type);
            var ctor = generator.Builder.DefineConstructor(GetAttributes(), System.Reflection.CallingConventions.Standard, parameterTypes);
            var ctorGen = new ConstructorGenerator(ctor, parameters, generator)
            {
                SyntaxBody = Body
            };
            generator.Add(ctorGen);
        }

        public override System.Reflection.MethodAttributes GetAttributes()
        {
            return base.GetAttributes() | System.Reflection.MethodAttributes.HideBySig;
        }

    }
}
