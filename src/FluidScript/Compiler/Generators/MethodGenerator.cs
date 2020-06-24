using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Compiler.Generators
{
    // Temporary Method info wrapper
    public abstract class BaseMethodGenerator : System.Reflection.MethodInfo, IMethodBaseGenerator
    {
        protected IList<AttributeGenerator> _CustomAttributes;

        private readonly System.Reflection.MethodInfo methodInfo;
        internal readonly Type Declaring;

        public BaseMethodGenerator(System.Reflection.MethodInfo method, ParameterInfo[] parameters, Type declaring)
        {
            Name = method.Name;
            methodInfo = method;
            Parameters = parameters;
            ReturnType = method.ReturnType;
            Declaring = declaring;
            Attributes = method.Attributes;
            MemberType = System.Reflection.MemberTypes.Method;
        }

        public override System.Reflection.ICustomAttributeProvider ReturnTypeCustomAttributes => methodInfo.ReturnTypeCustomAttributes;

        public override System.Reflection.MethodInfo GetBaseDefinition()
        {
            return methodInfo.GetBaseDefinition();
        }

        public override System.Reflection.CallingConventions CallingConvention { get; }

        public override RuntimeMethodHandle MethodHandle => methodInfo.MethodHandle;

        public override System.Reflection.MethodAttributes Attributes { get; }

        public override Type DeclaringType => Declaring;

        public override Type ReflectedType => Declaring;

        public override string Name { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public override Type ReturnType { get; }

        public ParameterInfo[] Parameters { get; }

        public override System.Reflection.MemberTypes MemberType { get; }

        public SyntaxTree.Statement SyntaxBody { get; set; }

        public System.Reflection.MethodBase MethodBase => methodInfo;

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return Utils.TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                 && Utils.TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public virtual void SetCustomAttribute(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters)
        {
            if (_CustomAttributes == null)
                _CustomAttributes = new List<AttributeGenerator>();
            _CustomAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        public abstract void Generate();
        public abstract void EmitParameterInfo();

        public IProgramContext Context { get; set; }

        public Type GetType(TypeName typeName)
        {
            if (Context != null)
                return Context.GetType(typeName);
            return TypeProvider.Default.GetType(typeName.FullName);
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
                    parametersInstance = new System.Reflection.ParameterInfo[Parameters.Length];
                    for (int index = 0; index < Parameters.Length; index++)
                    {
                        parametersInstance[index] = new RuntimeParameterInfo(Parameters[index]);
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

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, ParameterInfo[] parameters, Type declaring) : base(builder, parameters, declaring)
        {
            _builder = builder;
        }

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, ParameterInfo[] parameters, TypeGenerator declaring) : base(builder, parameters, declaring)
        {
            _builder = builder;
            Context = declaring.Context;
        }

        public override void Generate()
        {
            EmitParameterInfo();
            new MethodBodyGenerator(this, _builder.GetILGenerator()).EmitBody();
        }

        public override void EmitParameterInfo()
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
        }

        internal System.Reflection.Emit.MethodBuilder GetBuilder()
        {
            return _builder;
        }
    }

    public sealed class DynamicMethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.DynamicMethod _builder;
        public DynamicMethodGenerator(System.Reflection.Emit.DynamicMethod builder, ParameterInfo[] parameters, Type declaring) : base(builder, parameters, declaring)
        {
            _builder = builder;
        }

        public override void Generate()
        {
            EmitParameterInfo();
            new MethodBodyGenerator(this, _builder.GetILGenerator()).EmitBody();
        }

        public override void EmitParameterInfo()
        {
#if NETFRAMEWORK || NETSTANDARD
            foreach (var para in Parameters)
            {
                _builder.DefineParameter(para.Index, System.Reflection.ParameterAttributes.In, para.Name);
            }
#endif
        }
    }
}
