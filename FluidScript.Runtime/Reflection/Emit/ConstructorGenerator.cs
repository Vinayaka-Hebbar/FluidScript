using System;
using System.Globalization;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public class ConstructorGenerator : System.Reflection.ConstructorInfo, IMemberGenerator, IMethodBaseGenerator
    {
        private readonly System.Reflection.Emit.ConstructorBuilder _builder;
        private readonly System.Type[] _baseParameterTypes;

        public ConstructorGenerator(System.Reflection.Emit.ConstructorBuilder builder, System.Type[] parameters, System.Type[] baseParameterTypes, TypeGenerator generator, Compiler.SyntaxTree.Statement statement)
        {
            _builder = builder;
            _baseParameterTypes = baseParameterTypes;
            Name = builder.Name;
            ParameterTypes = parameters;
            TypeGenerator = generator;
            SyntaxTree = statement;
            Attributes = builder.Attributes;
            MemberType = System.Reflection.MemberTypes.Method;
        }

        public Type[] ParameterTypes { get; }

        public TypeGenerator TypeGenerator { get; }

        public override System.Reflection.MemberTypes MemberType { get; }

        public Compiler.SyntaxTree.Statement SyntaxTree { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public System.Reflection.MethodBase MethodBase => this;

        public override RuntimeMethodHandle MethodHandle => _builder.MethodHandle;

        public override System.Reflection.MethodAttributes Attributes { get; }

        public override string Name { get; }

        public override Type DeclaringType => TypeGenerator;

        public override Type ReflectedType => TypeGenerator;

        public System.Collections.Generic.IEnumerable<ParameterInfo> Parameters { get; internal set; }

        public Type ReturnType => null;

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                 && TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _builder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _builder.GetCustomAttributes(attributeType, inherit);
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return _builder.GetMethodImplementationFlags();
        }

        public override System.Reflection.ParameterInfo[] GetParameters()
        {
            return _builder.GetParameters();
        }

        public override object Invoke(System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return _builder.Invoke(invokeAttr, binder, parameters, culture);
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return _builder.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            //todo custome attirbutes
            return attributeType == typeof(Runtime.RegisterAttribute);
        }

        public Type GetType(string typeName)
        {
            if (TypeGenerator != null)
                return TypeGenerator.GetType(typeName);
            return TypeUtils.GetType(typeName);
        }

        public void Build()
        {
            var body = new MethodBodyGenerator(this, _builder.GetILGenerator());
            foreach (FieldGenerator generator in TypeGenerator.Members.Where(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic == IsStatic))
            {
                if (IsStatic == false)
                    body.LoadArgument(0);
                generator.MethodBody = body;
                generator.Build();
                if (generator.DefaultValue != null)
                {
                    generator.DefaultValue.GenerateCode(body);
                    body.StoreField(generator.FieldInfo);
                }
            }
            if (IsStatic == false)
            {
                var baseCtor = TypeGenerator.BaseType.GetConstructor(_baseParameterTypes);
                body.LoadArgument(0);
                body.Call(baseCtor);
            }
            body.Build();
        }
    }
}
