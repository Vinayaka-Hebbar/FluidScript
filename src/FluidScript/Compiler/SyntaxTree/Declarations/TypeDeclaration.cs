using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    //todo primary constructor
    public class TypeDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly TypeSyntax BaseType;
        public readonly NodeList<TypeSyntax> Implements;

        public readonly NodeList<MemberDeclaration> Members;

        public TypeDeclaration(string name, TypeSyntax baseType, NodeList<TypeSyntax> implements, NodeList<MemberDeclaration> members)
        {
            Name = name;
            BaseType = baseType;
            Implements = implements;
            Members = members;
        }

        public ITextSource Source
        {
            get;
            set;
        }

        public override void Generate(Generators.TypeGenerator generator)
        {
            throw new System.NotImplementedException();
        }

        public System.Type Generate(AssemblyGen assembly)
        {
            System.Type baseType;
            if (BaseType != null)
                baseType = assembly.GetType(BaseType.ToString());
            else
                baseType = typeof(FSObject);
            var generator = assembly.DefineType(Name, baseType, System.Reflection.TypeAttributes.Public);
            generator.Source = Source;
            foreach (var member in Members)
            {
                member.Generate(generator);
            }
            return generator.Create();
        }
    }
}
