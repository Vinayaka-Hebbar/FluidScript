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

        public TypeDeclaration(string name, TypeSyntax baseType, INodeList<TypeSyntax> implements, INodeList<MemberDeclaration> members) : base(DeclarationType.Class)
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
            System.Type baseType;
            if (BaseType != null)
                baseType = BaseType.ResolveType(generator.Context);
            else
                baseType = typeof(FSObject);
            var type = generator.DefineNestedType(Name, baseType, System.Reflection.TypeAttributes.Public);
            System.Type[] types = null;
            if (Implements != null)
            {
                types = Implements.Map(impl => impl.ResolveType(generator.Context)).AddLast(typeof(IFSObject));
            }
            else
            {
                types = new System.Type[1] { typeof(IFSObject) };
            }
            type.SetInterfaces(types);
            type.Source = Source;
            type.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Utils.ReflectionHelpers.Register_Attr_Ctor, new object[] { Name });
            foreach (var member in Members)
            {
                member.CreateMember(type);
            }
            generator.Add(type);
        }

        public System.Type Compile(AssemblyGen assembly)
        {
            return Compile(assembly, assembly.Context);
        }

        public System.Type Compile(AssemblyGen assembly, ITypeContext context)
        {
            System.Type baseType;
            if (BaseType != null)
                baseType = BaseType.ResolveType(context);
            else
                baseType = typeof(FSObject);
            var generator = assembly.DefineType(Name, baseType, System.Reflection.TypeAttributes.Public, context);
            System.Type[] types = null;
            if (Implements != null)
            {
                types = Implements.Map(impl => impl.ResolveType(context)).AddLast(typeof(IFSObject));
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
