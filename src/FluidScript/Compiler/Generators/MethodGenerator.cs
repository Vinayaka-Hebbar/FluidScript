using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace FluidScript.Compiler.Generators
{
    // Temporary Method info wrapper
    public abstract class BaseMethodGenerator : System.Reflection.MethodInfo, IMethodBase
    {
        internal List<AttributeGenerator> _customAttributes;

        private readonly System.Reflection.MethodInfo methodInfo;
        private readonly Type declaring;

        private SyntaxTree.Statement syntaxBody;

        public BaseMethodGenerator(System.Reflection.MethodInfo method, ParameterInfo[] parameters, Type returnType, Type declaring)
        {
            this.declaring = declaring;
            Name = method.Name;
            methodInfo = method;
            Parameters = parameters;
            ReturnType = returnType;
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

        public override Type DeclaringType => declaring;

        public override Type ReflectedType => declaring;

        public override string Name { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public override Type ReturnType { get; }

        public ParameterInfo[] Parameters { get; }

        public override System.Reflection.MemberTypes MemberType { get; }

        public SyntaxTree.Statement SyntaxBody
        {
            get => syntaxBody;
            set => syntaxBody = value;
        }

        public System.Reflection.MethodBase MethodBase => methodInfo;

        public virtual void SetCustomAttribute(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters)
        {
            if (_customAttributes == null)
                _customAttributes = new List<AttributeGenerator>();
            _customAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        void IMember.Compile()
        {
            EmitParameterInfo();
            EmitBody();
        }

        public virtual void EmitBody()
        {
            if (SyntaxBody != null)
            {
                GetMethodBodyGenerator().Compile();
            }
        }

        public abstract void EmitParameterInfo();

        public abstract MethodBodyGenerator GetMethodBodyGenerator();

        public ITypeContext Context { get; set; }

        public Type GetType(TypeName typeName)
        {
            if (Context != null)
                return Context.GetType(typeName);
            return TypeProvider.GetType(typeName.FullName);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (_customAttributes != null)
                return _customAttributes.Map(att => att.Instance);
            return new Attribute[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_customAttributes != null)
            {
                return _customAttributes
                    .FindAll(att => att.Type == attributeType || (inherit && att.Type.IsAssignableFrom(attributeType)))
                    .Map(att => att.Instance);
            }
            return new Attribute[0];
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return methodInfo.GetMethodImplementationFlags();
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return methodInfo.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        private System.Reflection.ParameterInfo[] parametersInstance;
        public override System.Reflection.ParameterInfo[] GetParameters()
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

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _customAttributes != null && _customAttributes.Exists(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class MethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.MethodBuilder _builder;

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, ParameterInfo[] parameters, Type returnType, Type declaring) : base(builder, parameters, returnType, declaring)
        {
            _builder = builder;
        }

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, ParameterInfo[] parameters, Type returnType, TypeGenerator declaring) : base(builder, parameters, returnType, declaring)
        {
            _builder = builder;
            Context = new TypeContext(declaring.Context);
        }

        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, ParameterInfo[] parameters, TypeGenerator declaring) : base(builder, parameters, builder.ReturnType, declaring)
        {
            _builder = builder;
            Context = new TypeContext(declaring.Context);
        }

        public MethodGenerator SetBody(SyntaxTree.Statement body)
        {
            SyntaxBody = body;
            return this;
        }

        public override void EmitParameterInfo()
        {
            if (_customAttributes != null)
            {
                foreach (var attr in _customAttributes)
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

        public override void EmitBody()
        {
            base.EmitBody();
            if (IsFinal && IsVirtual
                && (Attributes & System.Reflection.MethodAttributes.NewSlot) == System.Reflection.MethodAttributes.NewSlot)
            {
                var type = DeclaringType as TypeGenerator;
                if (type == null)
                    return;
                int dot = Name.LastIndexOf('.');
                var implType = type.GetInterface(Name.Substring(0, dot));
                var method = implType.FindSystemMethod(Name.Substring(dot + 1), Parameters.Map(p => p.Type));
                if (method == null)
                    throw new NullReferenceException(nameof(method));
                type.Builder.DefineMethodOverride(_builder, method);

            }
        }

        public override MethodBodyGenerator GetMethodBodyGenerator()
        {
            return new MethodBodyGenerator(this, _builder.GetILGenerator());
        }

        internal System.Reflection.Emit.MethodBuilder GetBuilder()
        {
            return _builder;
        }

#if NETFRAMEWORK
        public System.Reflection.Emit.MethodToken GetToken()
        {
            return _builder.GetToken();
        }
#endif
    }

    public sealed class DynamicMethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.DynamicMethod _builder;
        public DynamicMethodGenerator(System.Reflection.Emit.DynamicMethod builder, ParameterInfo[] parameters, Type returnType, Type declaring) : base(builder, parameters, returnType, declaring)
        {
            _builder = builder;
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

        public override MethodBodyGenerator GetMethodBodyGenerator()
        {
            return new MethodBodyGenerator(this, _builder.GetILGenerator());
        }
    }
}
