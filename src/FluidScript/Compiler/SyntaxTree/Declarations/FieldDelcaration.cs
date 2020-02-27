namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class FieldDelcaration : MemberDeclaration
    {
        public readonly NodeList<VariableDeclarationExpression> Declarations;

        public FieldDelcaration(NodeList<VariableDeclarationExpression> declarations)
        {
            Declarations = declarations;
        }

        public override void Create(Generators.TypeGenerator generator)
        {
            System.Reflection.FieldAttributes attrs = GetAttribute();
            foreach (var field in Declarations)
            {
                generator.Add(new Generators.FieldGenerator(generator, attrs, field));
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
