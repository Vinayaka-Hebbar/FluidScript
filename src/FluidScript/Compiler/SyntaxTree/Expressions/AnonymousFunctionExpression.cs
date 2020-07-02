using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using FluidScript.Runtime;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousFunctionExpression : Expression
    {
        public readonly NodeList<TypeParameter> Parameters;

        public readonly TypeSyntax ReturnSyntax;

        public readonly BlockStatement Body;

        public ParameterInfo[] ParameterInfos { get; set; }

        public System.Type[] Types { get; set; }

        public System.Type ReturnType { get; set; }

        public AnonymousFunctionExpression(NodeList<TypeParameter> parameters, TypeSyntax returnSyntax, BlockStatement body) : base(ExpressionType.Function)
        {
            Parameters = parameters;
            ReturnSyntax = returnSyntax;
            Body = body;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAnonymousFunction(this);
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnSyntax);
        }

        public override void GenerateCode(MethodBodyGenerator generator, MethodGenerateOption option)
        {
            var target = generator.Method.DeclaringType;
            IExpressionVisitor<object> visitor = ScriptCompiler.Instance;
            //pass scoped arguments // refer System.Linq.Expression.Compiler folder
            var names = Parameters.Map(para => para.Name).AddFirst("closure");
            // Emit First Argument
            var lamdaVisit = new LamdaVisitor(names);
            Body.Accept(lamdaVisit);
            var lamdaGen = LamdaGen.DefineAnonymousMethod(Types, ReturnType);
            var methodGen = new Generators.MethodGenerator(lamdaGen.Method, ParameterInfos, lamdaGen.Type)
            {
                SyntaxBody = Body,
                Context = generator.Context
            };
            methodGen.EmitParameterInfo();
            var bodyGen = new MethodBodyGenerator(methodGen, lamdaGen.Method.GetILGenerator());
            var values = lamdaVisit.HoistedLocals.Values;
            var valVar = generator.DeclareVariable(LamdaGen.ObjectArray);
            ArrayListExpression.MakeObjectArray(generator, values);
            generator.StoreVariable(valVar);
            if (values.Count > 0)
            {
                int index = 0;
                var field = lamdaGen.Values;
                foreach (var item in lamdaVisit.HoistedLocals)
                {
                    var value = item.Value;
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
            bodyGen.Compile();
            var type = lamdaGen.CreateType();
            generator.LoadVariable(valVar);
            generator.NewObject(type.GetConstructors()[0]);
            generator.LoadFunction(type.GetInstanceMethod("Invoke", Types), Type);
        }

        //todo working on scope for better emit
        public System.Delegate Compile(System.Type target, IExpressionVisitor<object> visitor)
        {
            //pass scoped arguments // refer System.Linq.Expression.Compiler folder
            var context = TypeContext.Default;
            System.Type returnType;
            if (ReturnSyntax != null)
                returnType = ReturnSyntax.ResolveType((ITypeContext)context);
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
                parameters[i] = new ParameterInfo(para.Name, i + 1, type, para.IsVarArgs);
                types[i] = type;
            }
            // Emit First Argument
            var lamdaVisit = new LamdaVisitor(names);
            Body.Accept(lamdaVisit);
            var parameterTypes = types.AddFirst(typeof(Closure));
            var method = new System.Reflection.Emit.DynamicMethod("lambda_method", returnType, parameterTypes, true);

            var methodGen = new Generators.DynamicMethodGenerator(method, parameters, target)
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
                var field = typeof(Closure).GetField("Values");
                foreach (var item in lamdaVisit.HoistedLocals)
                {
                    var value = item.Value;
                    values[index] = value.Accept(visitor);
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
            bodyGen.Compile();
            var delgateType = DelegateGen.MakeNewDelegate(types, returnType);
            Type = delgateType;
            return method.CreateDelegate(delgateType, new Runtime.Closure(values));
        }
    }
}
