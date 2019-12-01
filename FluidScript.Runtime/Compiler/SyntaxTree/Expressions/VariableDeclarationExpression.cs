namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationExpression : DeclarationExpression
    {
        public readonly TypeSyntax Type;
        public readonly Expression Value;

        public VariableDeclarationExpression(string name, TypeSyntax type, Expression value) : base(name)
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

        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            //initialize
            if (Value != null)
            {
                Value.GenerateCode(generator);
                System.Type type = Type == null ? Value.ResultType(generator) : Type.GetTypeInfo().ResolvedType(generator.TypeGenerator);
                var variable = generator.DeclareVariable(type, Name);
                generator.StoreVariable(variable);
            }
        }


        public override string ToString()
        {
            //todo for not runtime
            string value = "null";
            if (Value != null)
            {
                value = Value.ToString();
            }

            return string.Concat(Name, Type == null ? null : string.Concat(":", Type), "=", value);
        }
    }
}
