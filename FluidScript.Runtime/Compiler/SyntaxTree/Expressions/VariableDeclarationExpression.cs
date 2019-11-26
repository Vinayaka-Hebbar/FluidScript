namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : DeclarationExpression
    {
        public readonly Emit.TypeName Type;
        public readonly Expression Value;

        public VariableDeclarationExpression(string name, Emit.TypeName type, Expression value) : base(name)
        {
            Type = type;
            Value = value;

        }


#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            return Value.Evaluate(instance);
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
            if (Value != null)
            {
                value = Value.ToString();
            }

            return string.Concat(Name, "=", value);
        }
    }
}
