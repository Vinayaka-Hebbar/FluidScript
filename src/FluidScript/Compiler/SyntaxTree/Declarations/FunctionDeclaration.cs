using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclaration : MemberDeclaration
    {
        public readonly string Name;
        public readonly NodeList<TypeParameter> Parameters;
        public readonly BlockStatement Body;
        public readonly TypeSyntax ReturnType;

        public FunctionDeclaration(string name, NodeList<TypeParameter> parameters, TypeSyntax returnType, BlockStatement body) : base(DeclarationType.Function)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public override System.Collections.Generic.IEnumerable<Node> ChildNodes() => Childs(Body);

        public bool IsGetter => (Modifiers & Modifiers.Getter) == Modifiers.Getter;

        public bool IsSetter => (Modifiers & Modifiers.Setter) == Modifiers.Setter;

        public override void CreateMember(Generators.TypeGenerator generator)
        {
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.ResolveType((ITypeContext)generator.Context);
            else
                returnType = TypeProvider.ObjectType;
            var parameters = Parameters.Map(para => para.GetParameterInfo(generator.Context));
            if (IsGetter || IsSetter)
                CreateProperty(generator, returnType, parameters);
            else
                CreateFunction(generator, returnType, parameters);
        }

        #region Property Get Set
        private void CreateProperty(Generators.TypeGenerator generator, System.Type returnType, ParameterInfo[] parameters)
        {
            var parameterTypes = parameters.Map(p => p.Type);
            Generators.PropertyGenerator.PropertyHolder accessor = null;
            System.Type type = null;
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            var builder = generator.Builder;
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if (IsGetter)
            {
                type = returnType;
                string hiddenName = string.Concat("get_", name);
                if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                    generator.CheckImplementMethod(Name, parameterTypes, ref hiddenName, ref returnType, ref attributes);
                System.Reflection.Emit.MethodBuilder getBul = builder.DefineMethod(hiddenName, attributes, returnType, parameterTypes);
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
        #endregion

        private void CreateFunction(Generators.TypeGenerator generator, System.Type returnType, ParameterInfo[] parameters)
        {
            var parameterTypes = parameters.Map(p => p.Type);
            //todo override toString and others
            var name = string.Concat(char.ToUpper(Name.First()), Name.Substring(1));
            System.Reflection.MethodAttributes attributes = GetAttributes();
            if ((attributes & System.Reflection.MethodAttributes.Virtual) == System.Reflection.MethodAttributes.Virtual)
                generator.CheckImplementMethod(Name, parameterTypes, ref name, ref returnType, ref attributes);
            // create method
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
            var context = TypeContext.Default;
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.ResolveType((ITypeContext)context);
            else
                returnType = TypeProvider.ObjectType;
            var parameters = Parameters.Map(para => para.GetParameterInfo(context));
            var parameterTypes = parameters.Map(para => para.Type);
            var method = new System.Reflection.Emit.DynamicMethod(Name, returnType, parameterTypes);
            IMember methodOpt = new Generators.DynamicMethodGenerator(method, parameters, null)
            {
                SyntaxBody = Body,
                Context = context
            };
            methodOpt.Compile();
            return method;
        }

        public virtual System.Reflection.MethodAttributes GetAttributes()
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
            // pass scoped arguments // refer System.Linq.Expression.Compiler folder
            var context = TypeContext.Default;
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.ResolveType((ITypeContext)context);
            else
                returnType = typeof(object);
            var names = Parameters.Map(para => para.Name).AddFirst("closure");
            int length = Parameters.Count;
            System.Type[] types = new System.Type[length];
            var parameters = new ParameterInfo[length];
            for (int i = 0; i < Parameters.Count; i++)
            {
                var para = Parameters[i];
                System.Type type = para.Type == null ? TypeProvider.ObjectType : para.Type.ResolveType((ITypeContext)context);
                parameters[i] = new ParameterInfo(para.Name, i + 1, type);
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
                Context = context
            };
            methodGen.EmitParameterInfo();
            var bodyGen = new MethodBodyGenerator(methodGen, method.GetILGenerator());
            object[] values = new object[lamdaVisit.HoistedLocals.Count];
            if (values.Length > 0)
            {
                int index = 0;
                var field = typeof(Runtime.Closure).GetField("Values");
                foreach (var item in lamdaVisit.HoistedLocals)
                {
                    var value = item.Value;
                    values[index] = value.Accept(ScriptCompiler.Instance);
                    var variable = bodyGen.DeclareVariable(value.Type, item.Key);
                    // load closure argument
                    bodyGen.LoadArgument(0);
                    bodyGen.LoadField(field);
                    bodyGen.LoadInt32(index);
                    bodyGen.LoadArrayElement(typeof(object));
                    bodyGen.UnboxObject(value.Type);
                    bodyGen.StoreVariable(variable);
                    index++;
                }
            }
            bodyGen.Compile();
            return method.CreateDelegate(DelegateGen.MakeNewDelegate(types, returnType), new Runtime.Closure(values));
        }

        public override string ToString()
        {
            return string.Concat(Name, "(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType == null ? "any" : ReturnType.ToString());
        }
    }
}
