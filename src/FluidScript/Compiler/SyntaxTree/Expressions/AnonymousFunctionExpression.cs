﻿using FluidScript.Utils;
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
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):any");
        }


        //todo working on scope for better emit
        public System.Delegate Compile(IExpressionVisitor<object> visitor)
        {
            //pass scoped arguments // refer System.Linq.Expression.Compiler folder
            var provider = TypeProvider.Default;
            System.Type returnType;
            if (ReturnType != null)
                returnType = ReturnType.GetType(provider);
            else
                returnType = typeof(object);
            var names = Parameters.Map(para => para.Name).AddFirst("closure");
            int length = Parameters.Length;
            System.Type[] types = new System.Type[length];
            var parameters = new Emit.ParameterInfo[length + 1];
            for (int i = 0; i < Parameters.Length; i++)
            {

                var para = Parameters[i];
                var index = i + 1;
                System.Type type = para.Type == null ? TypeProvider.ObjectType : para.Type.GetType(provider);
                parameters[index] = new Emit.ParameterInfo(para.Name, index, type, para.IsVar);
                types[i] = type;
            }
            // Emit First Argument
            parameters[0] = new Emit.ParameterInfo(null, 0, typeof(Runtime.Closure), false);
            var lamdaVisit = new LamdaVisitor(names);
            Body.Accept(lamdaVisit);
            parameters = parameters.AddFirst(new Emit.ParameterInfo(null, 0, typeof(Runtime.Closure), false));
            var parameterTypes = types.AddFirst(typeof(Runtime.Closure));
            var method = new System.Reflection.Emit.DynamicMethod("lambda_method", returnType, parameterTypes, true);

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
            var delgateType = Emit.DelegateGen.MakeNewDelegate(types, returnType);
            Type = delgateType;
            return method.CreateDelegate(delgateType, new Runtime.Closure(values));
        }
    }
}
