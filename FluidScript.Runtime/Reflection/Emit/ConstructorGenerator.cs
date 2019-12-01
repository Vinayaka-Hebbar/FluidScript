using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public class ConstructorGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.ConstructorBuilder _builder;
        private readonly System.Type[] _baseParameterTypes;

        public ConstructorGenerator(System.Reflection.Emit.ConstructorBuilder builder, System.Type[] parameters, System.Type[] baseParameterTypes, TypeGenerator generator, Compiler.SyntaxTree.Statement statement) : base(builder, null, parameters, generator, statement)
        {
            _builder = builder;
            _baseParameterTypes = baseParameterTypes;
        }

        public override void Build()
        {
            var body = new MethodBodyGenerator(this, _builder.GetILGenerator());
            foreach (FieldGenerator generator in TypeGenerator.Where(mem => mem.MemberType == System.Reflection.MemberTypes.Field))
            {
                if (IsStatic == generator.IsStatic)
                {
                    generator.MethodBody = body;
                    generator.Build();
                    if (generator.DefaultValue != null)
                    {
                        generator.DefaultValue.GenerateCode(body);
                        body.StoreField(generator.FieldInfo);
                    }
                }
            }
            if (IsStatic == false)
            {
                var baseCtor = TypeGenerator.BaseType.GetConstructor(_baseParameterTypes);
                body.Call(baseCtor);
            }
            body.Build();
        }
    }
}
