using System.Runtime.InteropServices;
using FluidScript.Compiler.Emit;
using FluidScript.Core;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : Expression
    {
        public readonly string Name;
        public readonly Reflection.DeclaredVariable Variable;

        public VariableDeclarationExpression(string name, Reflection.DeclaredVariable variable) : base(ExpressionType.Declaration)
        {
            Name = name;
            Variable = variable;

        }

        protected override void ResolveType(OptimizationInfo info)
        {
            Expression valueAtTop = Variable.ValueAtTop;
            if (valueAtTop == null)
                return;
            else
            {
                ResolvedPrimitiveType = valueAtTop.PrimitiveType(info);
                ResolvedType = valueAtTop.ResultType(info);
            }
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var value = Variable.Evaluate(instance);
            instance[Variable.Name] = value;
            return value;
        }
#endif

#if Emit
        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            //initialize
            if (Variable.ValueAtTop != null)
            {
                Variable.ValueAtTop.GenerateCode(generator, info);
            }
            if (Variable.Store == null)
            {
                Type type = Variable.ResolveType(info);
                if (type == null)
                {
                    type = ResultType(info);
                    Variable.ResolveType(type);
                }
                Variable.Store = generator.DeclareVariable(type, Name);
            }
            generator.StoreVariable(Variable.Store);
        }

#endif

        public override string ToString()
        {
            //todo for not runtime
            string value = "null";
            if (Variable.ValueAtTop != null)
            {
                value = Variable.ValueAtTop.ToString();
            }
#if Runtime
            else if (Variable.DefaultValue is object)
            {
                value = Variable.DefaultValue.ToString();
            }
#endif

            return string.Concat(Variable.Name, "=", value);
        }
    }
}
