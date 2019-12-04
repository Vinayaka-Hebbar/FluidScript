using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluidScript.Reflection.Emit
{
    public sealed class PropertyGenerator : IMemberGenerator
    {
        private readonly System.Reflection.Emit.PropertyBuilder _builder;

        public PropertyGenerator(System.Reflection.Emit.PropertyBuilder builder)
        {
            _builder = builder;
            Name = builder.Name;
            MemberType = MemberTypes.Property;
        }

        public string Name { get; }

        public MemberInfo MemberInfo => _builder;

        public MemberTypes MemberType { get; }

        public bool IsStatic
        {
            get
            {
                var first = Accessors.FirstOrDefault();
                if (first == null)
                    throw new Exception("Can't decide wether property is static or not");
                return first.IsStatic;
            }
        }

        public bool IsPublic
        {
            get
            {
                var first = Accessors.FirstOrDefault();
                if (first == null)
                    throw new Exception("Can't decide wether property is static or not");
                return first.IsPublic;
            }
        }

        public IList<MethodGenerator> Accessors { get; } = new List<MethodGenerator>(2);

        public bool BindingFlagsMatch(BindingFlags flags)
        {
            return TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                           && TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public void Build()
        {
            foreach (var accessor in Accessors)
            {
                accessor.Build();
            }
        }

        internal System.Reflection.Emit.PropertyBuilder GetBuilder()
        {
            return _builder;
        }
    }
}
