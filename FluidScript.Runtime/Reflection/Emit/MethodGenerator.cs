using System;
using System.Collections.Generic;

namespace FluidScript.Reflection.Emit
{
    public abstract class BaseMethodGenerator : IMemberGenerator, ITypeProvider
    {
        public BaseMethodGenerator(System.Reflection.MethodBase method, Type returnType, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement)
        {
            Name = method.Name;
            MethodBase = method;
            ParameterTypes = parameters;
            ReturnType = returnType;
            TypeGenerator = declaring;
            SyntaxTree = statement;
            IsStatic = method.IsStatic;
            MemberType = System.Reflection.MemberTypes.Method;
        }

        public string Name { get; }

        public System.Reflection.MethodBase MethodBase { get; }

        public System.Reflection.MemberInfo MemberInfo => MethodBase;

        public IEnumerable<ParameterInfo> Parameters { get; internal set; }

        public Type ReturnType { get; }

        public Type[] ParameterTypes { get; }

        public TypeGenerator TypeGenerator { get; }

        public System.Reflection.MemberTypes MemberType { get; }

        public Compiler.SyntaxTree.Statement SyntaxTree { get; }

        public bool IsStatic { get; }

        public abstract void Build();

        public Type GetType(string typeName)
        {
            if (TypeGenerator != null)
                return TypeGenerator.GetType(typeName);
            return TypeUtils.GetType(typeName);
        }
    }

    public sealed class MethodGenerator : BaseMethodGenerator
    {
        private readonly System.Reflection.Emit.MethodBuilder _builder;
        public MethodGenerator(System.Reflection.Emit.MethodBuilder builder, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement) : base(builder, builder.ReturnType, parameters, declaring, statement)
        {
            _builder = builder;
        }

        public override void Build()
        {
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
        public DynamicMethodGenerator(System.Reflection.Emit.DynamicMethod builder, Type[] parameters, TypeGenerator declaring, Compiler.SyntaxTree.Statement statement) : base(builder, builder.ReturnType, parameters, declaring, statement)
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
