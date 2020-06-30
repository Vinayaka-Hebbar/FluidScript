using FluidScript.Compiler.Emit;
using System.Globalization;
using System.Linq;

namespace FluidScript.Compiler.Generators
{
    public class ConstructorGenerator : System.Reflection.ConstructorInfo, IMemberGenerator, IMethodBaseGenerator
    {
        private readonly System.Reflection.Emit.ConstructorBuilder _builder;
        private readonly System.Type[] _baseParameterTypes;
        private readonly TypeGenerator Declaring;

        public ConstructorGenerator(System.Reflection.Emit.ConstructorBuilder builder, ParameterInfo[] parameters, System.Type[] baseParameterTypes, TypeGenerator generator)
        {
            _builder = builder;
            _baseParameterTypes = baseParameterTypes;
            Name = builder.Name;
            Parameters = parameters;
            Declaring = generator;
            Context = generator.Context;
            Attributes = builder.Attributes;
            MemberType = System.Reflection.MemberTypes.Method;
        }

        public override System.Reflection.MemberTypes MemberType { get; }

        public SyntaxTree.Statement SyntaxBody { get; set; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public System.Reflection.MethodBase MethodBase => this;

        public override System.RuntimeMethodHandle MethodHandle => _builder.MethodHandle;

        public override System.Reflection.MethodAttributes Attributes { get; }

        public override string Name { get; }

        public override System.Type DeclaringType => Declaring;

        public override System.Type ReflectedType => Declaring;

        public ParameterInfo[] Parameters { get; }

        public System.Type ReturnType => null;

        public IProgramContext Context { get; set; }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _builder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
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

        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            //todo custome attirbutes
            return attributeType == typeof(Runtime.RegisterAttribute);
        }

        public System.Type GetType(TypeName typeName)
        {
            if (Context != null)
                return Context.GetType(typeName);
            return TypeProvider.Default.GetType(typeName.FullName);
        }

        public void Generate()
        {
            var body = new MethodBodyGenerator(this, _builder.GetILGenerator());
            foreach (FieldGenerator generator in Declaring.Members.Where(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic == IsStatic))
            {
                if (generator.DefaultValue != null)
                {
                    if (IsStatic == false)
                        body.LoadArgument(0);
                    generator.MethodBody = body;
                    generator.Generate();
                    generator.DefaultValue.GenerateCode(body);
                    body.StoreField(generator.FieldInfo);
                }
            }
            if (IsStatic == false)
            {
                var baseCtor = Declaring.BaseType.GetConstructor(_baseParameterTypes);
                body.LoadArgument(0);
                body.Call(baseCtor);
            }
            body.EmitBody();
        }
    }
}
