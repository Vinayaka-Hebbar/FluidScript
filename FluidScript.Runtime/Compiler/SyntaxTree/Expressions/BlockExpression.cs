using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockExpression : Expression
    {
        public readonly Statement[] Statements;
        public readonly Metadata.Prototype Prototype;
        public BlockExpression(Statement[] expressions, Metadata.Prototype prototype) : base(ExpressionType.Block)
        {
            Statements = expressions;
            Prototype = prototype;
        }

        public override RuntimeObject Evaluate()
        {
            foreach (var item in Statements)
            {
                switch (item.NodeType)
                {
                    case StatementType.Labeled:
                        var variable = Prototype.GetVariable(item.ToString());
                        variable.Value = variable.Evaluate();
                        break;
                    case StatementType.Function:
                        var declaration = ((FunctionDeclarationStatement)item).Declaration;
                        var method = Prototype.GetMethod(declaration.Name, declaration.PrimitiveArguments().ToArray());
                        method.Delegate = method.Create(Prototype);
                        break;
                }
            }
            return Prototype;
        }
    }
}
