using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System.Globalization;

namespace FluidScript.Compiler.Generators
{
    public class ConstructorGenerator : System.Reflection.ConstructorInfo, IMember, IMethodBase
    {
        private readonly System.Reflection.Emit.ConstructorBuilder builder;
        private readonly TypeGenerator Declaring;

        public ConstructorGenerator(System.Reflection.Emit.ConstructorBuilder builder, ParameterInfo[] parameters, TypeGenerator generator)
        {
            this.builder = builder;
            Name = builder.Name;
            Parameters = parameters;
            Declaring = generator;
            Context = generator.Context;
            Attributes = builder.Attributes;
        }

        public override System.Reflection.MemberTypes MemberType => System.Reflection.MemberTypes.Constructor;

        public Statement SyntaxBody { get; set; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public System.Reflection.MethodBase MethodBase => builder;

        public override System.RuntimeMethodHandle MethodHandle => builder.MethodHandle;

        public override System.Reflection.MethodAttributes Attributes { get; }

        public override string Name { get; }

        public override System.Type DeclaringType => Declaring;

        public override System.Type ReflectedType => Declaring;

        public ParameterInfo[] Parameters { get; }

        public System.Type ReturnType => null;

        public ITypeContext Context { get; set; }

        public override int MetadataToken => builder.MetadataToken;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return builder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            return builder.GetCustomAttributes(attributeType, inherit);
        }

        public override System.Reflection.MethodImplAttributes GetMethodImplementationFlags()
        {
            return builder.GetMethodImplementationFlags();
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

        public override object Invoke(System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return builder.Invoke(invokeAttr, binder, parameters, culture);
        }

        public override object Invoke(object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object[] parameters, CultureInfo culture)
        {
            return builder.Invoke(obj, invokeAttr, binder, parameters, culture);
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
            return TypeProvider.GetType(typeName.FullName);
        }

        void IMember.Compile()
        {
            var bodyGen = new MethodBodyGenerator(this, builder.GetILGenerator());
            foreach (FieldGenerator generator in Declaring.Members.FindAll(mem => mem.MemberType == System.Reflection.MemberTypes.Field && mem.IsStatic == IsStatic))
            {
                if (generator.DefaultValue != null)
                {
                    if (IsStatic == false)
                        bodyGen.LoadArgument(0);
                    generator.MethodBody = bodyGen;
                    ((IMember)generator).Compile();
                    generator.DefaultValue.GenerateCode(bodyGen, Expression.AssignOption);
                    bodyGen.StoreField(generator.FieldInfo);
                }
            }
            if (IsStatic == false && Declaring.IsClass)
            {
                // if not call to super or this call
                if (!(SyntaxBody is BlockStatement body && body.Statements.Count > 0 && body.Statements[0] is ExpressionStatement statement && statement.Expression is InvocationExpression exp && (exp.Target.NodeType == ExpressionType.Super || exp.Target.NodeType == ExpressionType.This)))
                {
                    var baseCtor = Declaring.BaseType.GetConstructor(new System.Type[0]);
                    bodyGen.LoadArgument(0);
                    bodyGen.Call(baseCtor);
                }
            }
            bodyGen.Compile();
        }
    }
}
