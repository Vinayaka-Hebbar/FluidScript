using FluidScript.Utils;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class AnonymousFunctionExpression : Expression
    {
        public readonly NodeList<TypeParameter> Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public AnonymousFunctionExpression(NodeList<TypeParameter> parameters, TypeSyntax returnType, BlockStatement body) : base(ExpressionType.Function)
        {
            Parameters = parameters;
            ReturnType = returnType;
            Body = body;
        }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitAnonymousFunction(this);
        }

        public override string ToString()
        {
            //todo return type
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType);
        }


        //todo working on scope for better emit
        public System.Delegate Compile(System.Type target, IExpressionVisitor<object> visitor)
        {
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
            var method = new System.Reflection.Emit.DynamicMethod("lambda_method", returnType, parameterTypes, true);

            var methodGen = new Generators.DynamicMethodGenerator(method, parameters, target)
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
            var delgateType = Emit.DelegateGen.MakeNewDelegate(types, returnType);
            Type = delgateType;
            return method.CreateDelegate(delgateType, new Runtime.Closure(values));
        }
    }
}
