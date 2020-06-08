using FluidScript.Compiler.Emit;
using FluidScript.Utils;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly NodeList<TypeParameter> Parameters;
        public readonly BlockStatement Body;
        public readonly TypeSyntax ReturnType;

        public FunctionDeclaration(string name, NodeList<TypeParameter> parameters, TypeSyntax returnType, BlockStatement body)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public override System.Collections.Generic.IEnumerable<Node> ChildNodes() => Childs(Body);

        public bool IsGetter => (Modifiers & Compiler.Modifiers.Getter) == Compiler.Modifiers.Getter;

        public bool IsSetter => (Modifiers & Modifiers.Setter) == Modifiers.Setter;

        public override void Compile(Generators.TypeGenerator generator)
        {
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetType(generator);
            else
                returnType = TypeProvider.ObjectType;
            var parameters = Parameters.Map(para => para.GetParameterInfo(generator));
            var parameterTypes = parameters.Map(para => para.Type);
            if (IsGetter || IsSetter)
                CreateProperty(generator, returnType, parameters, parameterTypes);
            else
                CreateFunction(generator, returnType, parameters, parameterTypes);
        }

        private void CreateProperty(Generators.TypeGenerator generator, System.Type returnType, ParameterInfo[] parameters, System.Type[] parameterTypes)
        {
            Generators.PropertyGenerator.PropertyHolder accessor = null;
            System.Type type = null;
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            var builder = generator.Builder;
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if (IsGetter)
            {
                type = returnType;
                System.Reflection.Emit.MethodBuilder getBul = builder.DefineMethod(string.Concat("get_", name), attributes, returnType, parameterTypes);
                accessor = new Generators.PropertyGenerator.PropertyHolder(Generators.PropertyType.Get,
                    new Generators.MethodGenerator(getBul, parameters, generator)
                    {
                        SyntaxBody = Body
                    });
            }
            if (IsSetter)
            {
                type = parameterTypes.FirstOrDefault();
                accessor = new Generators.PropertyGenerator.PropertyHolder(Generators.PropertyType.Set,
                    new Generators.MethodGenerator(builder.DefineMethod(string.Concat("set_", name), attributes, returnType, parameterTypes), parameters, generator)
                    {
                        SyntaxBody = Body
                    });
            }
            if (generator.TryGetProperty(Name, out Generators.PropertyGenerator property) == false)
            {
                var pb = generator.Builder.DefineProperty(name, System.Reflection.PropertyAttributes.None, type, null);
                property = new Generators.PropertyGenerator(generator, pb);
                property.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Utils.ReflectionHelpers.Register_Attr_Ctor, new[] { Name });
                generator.Add(property);
            }

            System.Reflection.Emit.PropertyBuilder propertyBuilder = property.GetBuilder();
            if (IsGetter)
                propertyBuilder.SetGetMethod(accessor.Method.GetBuilder());
            else if (IsSetter)
                propertyBuilder.SetSetMethod(accessor.Method.GetBuilder());
            else
                throw new System.Exception("Accessor not found");
            property.Accessors.Add(accessor);
        }

        private void CreateFunction(Generators.TypeGenerator generator, System.Type returnType, ParameterInfo[] parameters, System.Type[] parameterTypes)
        {
            //todo override toString and others
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                generator.CanImplementMethod(Name, parameterTypes, out name);
            var method = generator.Builder.DefineMethod(name, attributes, returnType, parameterTypes);
            //set runtime method name
            Generators.MethodGenerator methodGen = new Generators.MethodGenerator(method, parameters, generator)
            {
                SyntaxBody = Body
            };
            methodGen.SetCustomAttribute(typeof(Runtime.RegisterAttribute), Utils.ReflectionHelpers.Register_Attr_Ctor, new object[] { Name });
            generator.Add(methodGen);
        }

        public System.Reflection.MethodInfo CreateMethod()
        {
            var provider = TypeProvider.Default;
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetType(provider);
            else
                returnType = TypeProvider.ObjectType;
            var parameters = Parameters.Map(para => para.GetParameterInfo(provider));
            var parameterTypes = parameters.Map(para => para.Type);
            var method = new System.Reflection.Emit.DynamicMethod(Name, returnType, parameterTypes);
            var methodOpt = new Generators.DynamicMethodGenerator(method, parameters, null)
            {
                SyntaxBody = Body,
                Provider = provider
            };
            methodOpt.Generate();
            return method;
        }

        public System.Reflection.MethodAttributes GetAttributes()
        {
            System.Reflection.MethodAttributes attributes = System.Reflection.MethodAttributes.Public;
            if ((Modifiers & Modifiers.Private) == Modifiers.Private)
                attributes = System.Reflection.MethodAttributes.Private;
            if ((Modifiers & Modifiers.Static) == Modifiers.Static)
                attributes |= System.Reflection.MethodAttributes.Static;
            if ((Modifiers & Modifiers.Implement) == Modifiers.Implement)
                attributes |= System.Reflection.MethodAttributes.Virtual | System.Reflection.MethodAttributes.HideBySig;
            if ((Modifiers & Modifiers.Abstract) == Modifiers.Abstract)
                attributes |= System.Reflection.MethodAttributes.Abstract;
            return attributes;
        }

        public System.Delegate Compile()
        {
            var visitor = ScriptCompiler.Default;
            //pass scoped arguments // refer System.Linq.Expression.Compiler folder
            var provider = TypeProvider.Default;
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetType(provider);
            else
                returnType = typeof(object);
            var names = Parameters.Map(para => para.Name).AddFirst("closure");
            int length = Parameters.Count;
            System.Type[] types = new System.Type[length];
            var parameters = new Emit.ParameterInfo[length];
            for (int i = 0; i < Parameters.Count; i++)
            {
                var para = Parameters[i];
                System.Type type = para.Type == null ? TypeProvider.ObjectType : para.Type.GetType(provider);
                parameters[i] = new Emit.ParameterInfo(para.Name, i + 1, type, para.IsVar);
                types[i] = type;
            }
            // Emit First Argument
            var lamdaVisit = new LamdaVisitor(names);
            Body.Accept(lamdaVisit);
            var parameterTypes = types.AddFirst(typeof(Runtime.Closure));
            var method = new System.Reflection.Emit.DynamicMethod(Name, returnType, parameterTypes, true);

            var methodGen = new Generators.DynamicMethodGenerator(method, parameters, null)
            {
                SyntaxBody = Body,
                Provider = provider
            };
            methodGen.EmitParameterInfo();
            var bodyGen = new Emit.MethodBodyGenerator(methodGen, method.GetILGenerator());
            object[] values = new object[lamdaVisit.HoistedLocals.Count];
            if (values.Length > 0)
            {
                int index = 0;
                var field = typeof(Runtime.Closure).GetField("Values");
                foreach (var item in lamdaVisit.HoistedLocals)
                {
                    var value = item.Value;
                    values[index] = visitor.Visit(value);
                    var variable = bodyGen.DeclareVariable(value.Type, item.Key);
                    bodyGen.LoadArgument(0);
                    bodyGen.LoadField(field);
                    bodyGen.LoadInt32(index);
                    bodyGen.LoadArrayElement(typeof(object));
                    bodyGen.UnboxObject(value.Type);
                    bodyGen.StoreVariable(variable);
                    index++;
                }
            }
            bodyGen.EmitBody();
            var delgateType = DelegateGen.MakeNewDelegate(types, returnType);
            return method.CreateDelegate(delgateType, new Runtime.Closure(values));
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType == null ? "any" : ReturnType.ToString());
        }
    }
}
