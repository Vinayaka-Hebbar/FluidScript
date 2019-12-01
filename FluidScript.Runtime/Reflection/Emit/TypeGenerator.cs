using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public class TypeGenerator : ITypeProvider, IEnumerable<IMemberGenerator>
    {
        private const System.Reflection.BindingFlags IgnoreCase = IgnoreCaseInstance | System.Reflection.BindingFlags.Static;
        private const System.Reflection.BindingFlags IgnoreCaseInstance = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase;
        private readonly IList<IMemberGenerator> Members = new List<IMemberGenerator>();
        private readonly System.Reflection.Emit.TypeBuilder _builder;
        public readonly ReflectionModule ReflectionModule;
        public System.Reflection.Module Module { get; }

        public string Name { get; }

        public System.Type Type => _builder;

        public System.Type BaseType { get; }

        public Core.IScriptSource Source
        {
            get;
            set;
        }

        internal bool TryGetProperty(string name, out PropertyGenerator property)
        {
            var member = Members.FirstOrDefault(mem => mem.MemberType == System.Reflection.MemberTypes.Property && string.Equals(mem.Name, name, System.StringComparison.OrdinalIgnoreCase));
            if (member != null)
            {
                property = (PropertyGenerator)member;
                return true;
            }
            property = null;
            return false;
        }

        public System.Reflection.MemberInfo MemberInfo => _builder;

        public TypeGenerator(System.Reflection.Emit.TypeBuilder builder, ReflectionModule module)
        {
            Name = builder.Name;
            _builder = builder;
            BaseType = _builder.BaseType;
            ReflectionModule = module;
            Module = builder.Module;
        }

        internal TypeGenerator()
        {
            Module = typeof(object).Module;
        }

        public void Add(IMemberGenerator generator)
        {
            Members.Add(generator);
        }

        public System.Type Create()
        {
            if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Constructor) == false)
            {
                //default ctor
                var ctor = new ConstructorGenerator(_builder.DefineConstructor(System.Reflection.MethodAttributes.Public, System.Reflection.CallingConventions.Standard, new System.Type[0]), new System.Type[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                ctor.Build();
            }
            if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic))
            {
                //check for static ctor
                if (Members.Any(mem => mem.MemberType == System.Reflection.MemberTypes.Constructor && mem.IsStatic) == false)
                {
                    var ctor = new ConstructorGenerator(_builder.DefineConstructor(System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static, System.Reflection.CallingConventions.Standard, new System.Type[0]), new System.Type[0], new System.Type[0], this, Compiler.SyntaxTree.Statement.Empty);
                    ctor.Build();
                }
            }
            foreach (var generator in Members)
            {
                generator.Build();
            }
#if NETSTANDARD
            return _builder.CreateTypeInfo();
#else
            return _builder.CreateType();
#endif
        }
#if NET40
        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter CreateDocumentWriter()
        {
            return ReflectionModule.DefineDocument(Source.Path);
        }
#endif

        internal System.Reflection.Emit.TypeBuilder GetBuilder()
        {
            return _builder;
        }

        public IEnumerable<System.Reflection.MemberInfo> FindMember(string name)
        {
            var member = Members.Where(mem => mem.Name == name).Select(mem => mem.MemberInfo);
            if (member.Any())
                return member;
            return BaseType.GetMember(name, IgnoreCase);
        }

        public bool TryGetMethod(string name, System.Type[] types, out System.Reflection.MethodBase method)
        {
            var methods = Members.Where(mem => mem.Name == name).OfType<BaseMethodGenerator>().Where(m => Enumerable.SequenceEqual(m.ParameterTypes, types));
            if (methods.Any())
            {
                method = methods.Select(mem => mem.MethodBase).FirstOrDefault();
                return true;
            }
            method = BaseType.GetMethod(name, IgnoreCase, null, types, new System.Reflection.ParameterModifier[0]);
            return method != null;
        }

        internal bool CanImplementMethod(string name, System.Type[] types, out string newName)
        {
            var method = BaseType.GetMethod(name, IgnoreCaseInstance, null, types, new System.Reflection.ParameterModifier[0]);
            bool hasExist = method != null;
            newName = hasExist ? method.Name : name;
            return hasExist;
        }

        internal bool CanImplementProperty(string name, System.Type returnType, System.Type[] parameterTypes, out string newName)
        {
            var property = BaseType.GetProperty(name, IgnoreCaseInstance, null, returnType, parameterTypes, new System.Reflection.ParameterModifier[0]);
            bool hasExist = property != null;
            newName = hasExist ? property.Name : name;
            return hasExist;
        }

        public void Build()
        {
            foreach (var member in Members)
            {
                member.Build();
            }
        }

        public System.Type GetType(string typeName)
        {
            if (TypeUtils.IsInbuiltType(typeName))
                return TypeUtils.GetInbuiltType(typeName);
            return Module.GetType(typeName);
        }

        public IEnumerator<IMemberGenerator> GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Members.GetEnumerator();
        }
    }
}
