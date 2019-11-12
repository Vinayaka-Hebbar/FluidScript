using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class VariableDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public VariableDeclarationStatement(VariableDeclarationExpression[] declarationExpressions) : base(StatementType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            foreach (var declaration in DeclarationExpressions)
            {
                declaration.GenerateCode(generator, info);
            }
        }

        public override RuntimeObject Evaluate()
        {
            RuntimeObject[] objects = new RuntimeObject[DeclarationExpressions.Length];
            for (int i = 0; i < DeclarationExpressions.Length; i++)
            {
                VariableDeclarationExpression declaration = DeclarationExpressions[i];
                objects[i] = declaration.Evaluate();
            }
            return new Core.ArrayObject(objects, RuntimeType.Array);
        }
    }
}
