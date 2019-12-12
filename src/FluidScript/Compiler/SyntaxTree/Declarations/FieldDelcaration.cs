using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class FieldDelcaration : MemberDeclaration
    {
        public readonly VariableDeclarationExpression[] Declarations;

        public FieldDelcaration(VariableDeclarationExpression[] declarations)
        {
            Declarations = declarations;
        }

        public override void Create(TypeGenerator generator)
        {
            var builder = generator.GetBuilder();
            System.Reflection.FieldAttributes attrs = GetAttribute();
            foreach (var field in Declarations)
            {
                generator.Add(new FieldGenerator(generator, attrs, field));
            }
        }

        private System.Reflection.FieldAttributes GetAttribute()
        {
            System.Reflection.FieldAttributes attributes = System.Reflection.FieldAttributes.Public;
            if ((Modifiers & Reflection.Modifiers.Private) == Reflection.Modifiers.Private)
                attributes = System.Reflection.FieldAttributes.Private;
            if ((Modifiers & Reflection.Modifiers.ReadOnly) == Reflection.Modifiers.ReadOnly)
                attributes |= System.Reflection.FieldAttributes.InitOnly;
            if ((Modifiers & Reflection.Modifiers.Static) == Reflection.Modifiers.Static)
                attributes |= System.Reflection.FieldAttributes.Static;
            //todo invalid modifier handle
            return attributes;
        }
    }
}
