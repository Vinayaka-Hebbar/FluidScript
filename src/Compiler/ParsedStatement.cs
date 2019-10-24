using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler
{
    public class ParsedStatement
    {
        public readonly Statement Statement;
        public readonly IOperationContext Context;

        public ParsedStatement(IOperationContext context, Statement expression)
        {
            Context = context;
            Statement = expression;
        }

        public Object Evaluate()
        {
            return Statement.Accept(new NodeVisitor(Context));
        }
    }
}
