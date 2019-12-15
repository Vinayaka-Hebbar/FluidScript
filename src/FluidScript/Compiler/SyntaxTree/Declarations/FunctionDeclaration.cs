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
                returnType = ReturnType.GetType(generator);
            else
                returnType = typeof(void);
            var parameters = Parameters.Select(para => para.GetParameterInfo(generator));
            var parameterTypes = parameters.Select(para => para.Type).ToArray();
            if (IsGetter || IsSetter)
                CreateProperty(generator, returnType, parameters, parameterTypes);
            else
                CreateFunction(generator, returnType, parameters, parameterTypes);
        }

        private void CreateProperty(TypeGenerator generator, System.Type returnType, System.Collections.Generic.IEnumerable<ParameterInfo> parameters, System.Type[] parameterTypes)
        {
            PropertyGenerator.PropertyHolder accessor = null;
            System.Type type = null;
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            var builder = generator.GetBuilder();
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if (IsGetter)
            {
                type = returnType;
                System.Reflection.Emit.MethodBuilder getBul = builder.DefineMethod(string.Concat("get_", name), attributes, returnType, parameterTypes);
                accessor = new PropertyGenerator.PropertyHolder(PropertyType.Get,
                    new MethodGenerator(getBul, parameterTypes, generator, Body));
            }
            if (IsSetter)
            {
                type = parameterTypes.FirstOrDefault();
                accessor = new PropertyGenerator.PropertyHolder(PropertyType.Set,
                    new MethodGenerator(builder.DefineMethod(string.Concat("set_", name), attributes, returnType, parameterTypes), parameterTypes, generator, Body));
            }
            if (generator.TryGetProperty(Name, out PropertyGenerator property) == false)
            {
                var pb = generator.GetBuilder().DefineProperty(name, System.Reflection.PropertyAttributes.None, type, null);
                property = new PropertyGenerator(generator, pb);
                property.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Helpers.Register_Attr_Ctor, new[] { Name });
                generator.Add(property);
            }

            System.Reflection.Emit.PropertyBuilder propertyBuilder = property.GetBuilder();
            if (IsGetter)
                propertyBuilder.SetGetMethod(accessor.Method.GetBuilder());
            else if (IsSetter)
                propertyBuilder.SetSetMethod(accessor.Method.GetBuilder());
            else
                throw new System.Exception("Accessor not found");
            accessor.Method.Parameters = parameters;
            property.Accessors.Add(accessor);
        }

        private void CreateFunction(TypeGenerator generator, System.Type returnType, System.Collections.Generic.IEnumerable<ParameterInfo> parameters, System.Type[] parameterTypes)
        {
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                generator.CanImplementMethod(Name, parameterTypes, out name);
            var method = generator.GetBuilder().DefineMethod(name, attributes, returnType, parameterTypes);
            //set runtime method name
            MethodGenerator methodGen = new MethodGenerator(method, parameterTypes, generator, Body)
            {
                Parameters = parameters
            };
            methodGen.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Helpers.Register_Attr_Ctor, new object[] { Name });
            generator.Add(methodGen);
        }

        public System.Reflection.MethodInfo Create()
        {
            var generator = new TypeGenerator();
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetTypeInfo().ResolvedType(generator);
            else
                returnType = typeof(object);
            var parameters = Parameters.Select(para => para.GetParameterInfo(generator));
            var parameterTypes = parameters.Select(para => para.Type).ToArray();
            var method = new System.Reflection.Emit.DynamicMethod(Name, returnType, parameterTypes);
            var methodOpt = new DynamicMethodGenerator(method, parameterTypes, generator, Body)
            {
                Parameters = parameters
            };
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
