namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class FieldDelcaration : MemberDeclaration
    {
        public readonly NodeList<VariableDeclarationExpression> Declarations;

        public FieldDelcaration(NodeList<VariableDeclarationExpression> declarations):base(DeclarationType.Field)
        {
            Declarations = declarations;
        }

        public override void CreateMember(Generators.TypeGenerator generator)
        {
            System.Reflection.FieldAttributes attrs = GetAttribute();
            foreach (var field in Declarations)
            {
                Generators.FieldGenerator fieldGen = new Generators.FieldGenerator(generator, attrs, field);
                fieldGen.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Utils.ReflectionHelpers.Register_Attr_Ctor, new[] { field.Name });
                generator.Add(fieldGen);
            }
        }

        private System.Reflection.FieldAttributes GetAttribute()
        {
            System.Reflection.FieldAttributes attributes = System.Reflection.FieldAttributes.Public;
            if ((Modifiers & Modifiers.Private) == Modifiers.Private)
                attributes = System.Reflection.FieldAttributes.Private;
            if ((Modifiers & Modifiers.ReadOnly) == Modifiers.ReadOnly)
                attributes |= System.Reflection.FieldAttributes.InitOnly;
            if ((Modifiers & Modifiers.Static) == Modifiers.Static)
                attributes |= System.Reflection.FieldAttributes.Static;
            //todo invalid modifier handle
            return attributes;
        }
    }
}
