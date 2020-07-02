using FluidScript.Compiler.Emit;
using FluidScript.Extensions;

namespace FluidScript.Compiler.SyntaxTree
{
    //todo primary constructor
    public class TypeDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly TypeSyntax BaseType;
        public readonly INodeList<TypeSyntax> Implements;

        public readonly INodeList<MemberDeclaration> Members;

        public TypeDeclaration(string name, TypeSyntax baseType, INodeList<TypeSyntax> implements, NodeList<MemberDeclaration> members) : base(DeclarationType.Class)
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

        public override void CreateMember(Generators.TypeGenerator generator)
        {
            throw new System.NotImplementedException();
        }

        public System.Type Compile(AssemblyGen assembly)
        {
            System.Type baseType;
            if (BaseType != null)
                baseType = BaseType.ResolveType(assembly.Context);
            else
                baseType = typeof(FSObject);
            var generator = assembly.DefineType(Name, baseType, System.Reflection.TypeAttributes.Public);
            System.Type[] types = null;
            if (Implements != null)
            {
                types = Implements.Map(impl => impl.ResolveType(generator.Context)).AddLast(typeof(IFSObject));
            }
            else
            {
                types = new System.Type[1] { typeof(IFSObject) };
            }
            generator.SetInterfaces(types);
            generator.Source = Source;
            generator.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Utils.ReflectionHelpers.Register_Attr_Ctor, new object[] { Name });
            foreach (var member in Members)
            {
                member.CreateMember(generator);
            }
            return generator.CreateType();
        }
    }
}
