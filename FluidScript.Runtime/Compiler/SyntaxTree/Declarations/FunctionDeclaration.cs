using FluidScript.Reflection;
using FluidScript.Reflection.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly TypeParameter[] Parameters;
        public readonly BlockStatement Body;
        public readonly TypeSyntax ReturnType;

        public FunctionDeclaration(string name, TypeParameter[] parameters, TypeSyntax returnType, BlockStatement body)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public override System.Collections.Generic.IEnumerable<Node> ChildNodes() => Childs(Body);

        public bool IsGetter => (Modifiers & Reflection.Modifiers.Getter) == Reflection.Modifiers.Getter;

        public bool IsSetter => (Modifiers & Reflection.Modifiers.Setter) == Reflection.Modifiers.Setter;

        public override void Create(TypeGenerator generator)
        {
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetTypeInfo().ResolvedType(generator);
            else
                returnType = typeof(void);
            var parameters = Parameters.Select(para => para.GetParameterInfo());
            var parameterTypes = parameters.Select(para => para.Type.ResolvedType(generator)).ToArray();
            if (IsGetter || IsSetter)
                CreateProperty(generator, returnType, parameters, parameterTypes);
            else
                CreateFunction(generator, returnType, parameters, parameterTypes);
        }

        private void CreateProperty(TypeGenerator generator, System.Type returnType, System.Collections.Generic.IEnumerable<ParameterInfo> parameters, System.Type[] parameterTypes)
        {
            MethodGenerator accessor = null;
            System.Type type = null;
            var name = Name;
            var builder = generator.GetBuilder();
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                generator.CanImplementProperty(name, returnType, parameterTypes, out name);
            if (IsGetter)
            {
                type = returnType;
                System.Reflection.Emit.MethodBuilder builder1 = builder.DefineMethod(string.Concat("get_", name), attributes, returnType, parameterTypes);
                accessor = new MethodGenerator(builder1, parameterTypes, generator, Body);
            }
            if (IsSetter)
            {
                type = parameterTypes.FirstOrDefault();
                accessor = new MethodGenerator(builder.DefineMethod(string.Concat("set_", name), attributes, returnType, parameterTypes), parameterTypes, generator, Body);
            }
            if (generator.TryGetProperty(Name, out PropertyGenerator property) == false)
            {
                var pb = generator.GetBuilder().DefineProperty(name, System.Reflection.PropertyAttributes.None, type, null);
                property = new PropertyGenerator(pb);
                generator.Add(property);
            }

            System.Reflection.Emit.PropertyBuilder propertyBuilder = property.GetBuilder();
            if (IsGetter)
                propertyBuilder.SetGetMethod(accessor.GetBuilder());
            else if (IsSetter)
                propertyBuilder.SetSetMethod(accessor.GetBuilder());
            else
                throw new System.Exception("Accessor not found");
            accessor.Parameters = parameters;
            property.Accessors.Add(accessor);
        }

        private void CreateFunction(TypeGenerator generator, System.Type returnType, System.Collections.Generic.IEnumerable<Reflection.ParameterInfo> parameters, System.Type[] parameterTypes)
        {
            var name = Name;
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                generator.CanImplementMethod(name, parameterTypes, out name);
            var method = generator.GetBuilder().DefineMethod(name, attributes, returnType, parameterTypes);
            generator.Add(new MethodGenerator(method, parameterTypes, generator, Body)
            {
                Parameters = parameters,
            });
        }

        public System.Reflection.MethodInfo Create()
        {
            var generator = new TypeGenerator();
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetTypeInfo().ResolvedType(generator);
            else
                returnType = typeof(object);
            var parameters = Parameters.Select(para => para.GetParameterInfo());
            var parameterTypes = parameters.Select(para => para.Type.ResolvedType(generator)).ToArray();
            var method = new System.Reflection.Emit.DynamicMethod(Name, returnType, parameterTypes);
            var methodOpt = new DynamicMethodGenerator(method, parameterTypes, generator, Body);
            methodOpt.Build();
            return method;
        }

        public System.Reflection.MethodAttributes GetAttributes()
        {
            System.Reflection.MethodAttributes attributes = System.Reflection.MethodAttributes.Public;
            if ((Modifiers & Reflection.Modifiers.Private) == Reflection.Modifiers.Private)
                attributes = System.Reflection.MethodAttributes.Private;
            if ((Modifiers & Reflection.Modifiers.Static) == Reflection.Modifiers.Static)
                attributes |= System.Reflection.MethodAttributes.Static;
            if ((Modifiers & Reflection.Modifiers.Implement) == Reflection.Modifiers.Implement)
                attributes |= System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.HideBySig;
            if ((Modifiers & Reflection.Modifiers.Abstract) == Reflection.Modifiers.Abstract)
                attributes |= System.Reflection.MethodAttributes.Abstract;
            return attributes;
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType == null ? "any" : ReturnType.ToString());
        }
    }
}
