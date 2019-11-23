using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : DeclarationExpression
    {
        public readonly Reflection.DeclaredLocalVariable Variable;

        public VariableDeclarationExpression(string name, Reflection.DeclaredLocalVariable variable) : base(name)
        {
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
