using FluidScript.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    //todo primary constructor
    public class TypeDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly TypeSyntax BaseType;
        public readonly TypeSyntax[] Implements;

        public readonly MemberDeclaration[] Members;

        public TypeDeclaration(string name, TypeSyntax baseType, TypeSyntax[] implements, MemberDeclaration[] members)
        {
            Name = name;
            BaseType = baseType;
            Implements = implements;
            Members = members;
        }

        public Library.IScriptSource Source
        {
            get;
            set;
        }

        public override void Create(TypeGenerator generator)
        {
            throw new System.NotImplementedException();
        }

        public System.Type Generate(ReflectionModule module)
        {
            System.Type baseType;
            if (BaseType != null)
                baseType = module.GetType(BaseType.ToString());
            else
                baseType = typeof(FSObject);
            var generator = module.DefineType(Name, System.Reflection.TypeAttributes.Public, baseType);
            generator.Source = Source;
            foreach (var member in Members)
            {
                member.Create(generator);
            }
            return generator.Create();
        }
    }
}
