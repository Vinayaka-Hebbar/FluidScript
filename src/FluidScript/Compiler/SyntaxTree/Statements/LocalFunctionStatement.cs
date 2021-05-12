namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LocalFunctionStatement : Statement
    {
        public readonly string Name;

        public readonly INodeList<TypeParameter> Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public LocalFunctionStatement(string name, NodeList<TypeParameter> arguments, TypeSyntax returnType, BlockStatement body) : base(StatementType.Function)
        {
            Name = name;
            Parameters = arguments;
            ReturnType = returnType;
            Body = body;
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Map(arg => arg.ToString())), "):", ReturnType.ToString());
        }

        public override void GenerateCode(Emit.MethodBodyGenerator generator)
        {
            Body.GenerateCode(generator);
            if (generator.ReturnTarget != null)
                generator.DefineLabelPosition(generator.ReturnTarget);
            if (generator.ReturnVariable != null)
                generator.LoadVariable(generator.ReturnVariable);
        }
    }
}
