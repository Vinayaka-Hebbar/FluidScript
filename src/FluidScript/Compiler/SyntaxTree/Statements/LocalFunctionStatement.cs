using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class LocalFunctionStatement : Statement
    {
        public readonly string Name;

        public readonly TypeParameter[] Parameters;

        public readonly TypeSyntax ReturnType;

        public readonly BlockStatement Body;

        public LocalFunctionStatement(string name, TypeParameter[] arguments, TypeSyntax returnType, BlockStatement body) : base(StatementType.Function)
        {
            Name = name;
            Parameters = arguments;
            ReturnType = returnType;
            Body = body;
        }

        public override string ToString()
        {
            return string.Concat("(", string.Join(",", Parameters.Select(arg => arg.ToString())), "):", ReturnType.ToString());
        }

        public override void GenerateCode(Compiler.Emit.MethodBodyGenerator generator)
        {
            Body.GenerateCode(generator);
            if (generator.ReturnTarget != null)
                generator.DefineLabelPosition(generator.ReturnTarget);
            if (generator.ReturnVariable != null)
                generator.LoadVariable(generator.ReturnVariable);
        }
    }
}
