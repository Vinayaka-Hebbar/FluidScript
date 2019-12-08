using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public abstract class BaseMethodGenerator : System.Reflection.MethodInfo, IMemberGenerator, ITypeProvider, IMethodBaseGenerator
    {
        protected IList<AttributeGenerator> _CustomAttributes;

        private readonly System.Reflection.MethodInfo methodInfo;

        public BaseMethodGenerator(System.Reflection.MethodInfo method, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement)
        {
            Name = method.Name;
            methodInfo = method;
            ParameterTypes = parameters;
            ReturnType = method.ReturnType;
            TypeGenerator = declaring;
            SyntaxTree = statement;
            Attributes = method.Attributes;
            MemberType = System.Reflection.MemberTypes.Method;
        }

        public override System.Reflection.ICustomAttributeProvider ReturnTypeCustomAttributes => methodInfo.ReturnTypeCustomAttributes;

        public override System.Reflection.MethodInfo GetBaseDefinition()
        {
            return methodInfo.GetBaseDefinition();
        }

        public override RuntimeMethodHandle MethodHandle => methodInfo.MethodHandle;

        public override System.Reflection.MethodAttributes Attributes { get; }

        public override Type DeclaringType => TypeGenerator;

        public override Type ReflectedType => TypeGenerator;

        public override string Name { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public IEnumerable<ParameterInfo> Parameters { get; internal set; }

        public override Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        public TypeGenerator TypeGenerator { get; }

        public override System.Reflection.MemberTypes MemberType { get; }

        public Compiler.SyntaxTree.Statement SyntaxTree { get; }

        public System.Reflection.MethodBase MethodBase => methodInfo;

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                 && TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public virtual void SetCustomAttribute(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters)
        {
            if (_CustomAttributes == null)
                _CustomAttributes = new List<AttributeGenerator>();
            _CustomAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        public abstract void Build();

        public Type GetType(string typeName)
        {
            if (TypeGenerator != null)
                return TypeGenerator.GetType(typeName);
            return TypeUtils.GetType(typeName);
        }


        public override object[] GetCustomAttributes(bool inherit)
        {
            if (_CustomAttributes != null)
                return _CustomAttributes.Select(att => att.Instance).ToArray();
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_CustomAttributes != null)
            {
                IEnumerable<AttributeGenerator> enumerable = _CustomAttributes.Where(att => att.Type == attributeType);
                return enumerable.Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return methodInfo.GetMethodImplementationFlags();
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override System.Reflection.ParameterInfo[] GetParameters()
        {
            return ParametersInstance;
        }

        private System.Reflection.ParameterInfo[] parametersInstance;
        public System.Reflection.ParameterInfo[] ParametersInstance
        {
            get
            {
                if (parametersInstance == null)
                {
                    parametersInstance = new System.Reflection.ParameterInfo[ParameterTypes.Length];
                    for (int index = 0; index < ParameterTypes.Length; index++)
                    {
                        parametersInstance[index] = new RuntimeParameterInfo(ParameterTypes[index]);
                    }
                }
                return parametersInstance;
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _CustomAttributes != null && _CustomAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class MethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.MethodBuilder _builder;

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement) : base(builder, parameters, declaring, statement)
        {
            _builder = builder;
        }

        public override void Build()
        {
            if (_CustomAttributes != null)
            {
                foreach (var attr in _CustomAttributes)
                {
                    var cuAttr = new System.Reflection.Emit.CustomAttributeBuilder(attr.Ctor, attr.Parameters);
                    _builder.SetCustomAttribute(cuAttr);
                }
            }
            foreach (var para in Parameters)
            {
                _builder.DefineParameter(para.Index, System.Reflection.ParameterAttributes.In, para.Name);
            }
            new MethodBodyGenerator(this, _builder.GetILGenerator()).Build();
        }

        internal System.Reflection.Emit.MethodBuilder GetBuilder()
        {
            return _builder;
        }
    }

    public sealed class DynamicMethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.DynamicMethod _builder;
        public DynamicMethodGenerator(System.Reflection.Emit.DynamicMethod builder, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement) : base(builder, parameters, declaring, statement)
        {
            _builder = builder;
        }

        public override void Build()
        {
#if NET40 || NETSTANDARD
            foreach (var para in Parameters)
            {
                _builder.DefineParameter(para.Index, System.Reflection.ParameterAttributes.In, para.Name);
            }
#endif
            new MethodBodyGenerator(this, _builder.GetILGenerator()).Build();
        }
    }
}
