using FluidScript.Compiler.Emit;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class DeclarationStatement : Statement
    {
        public readonly DeclarationExpression[] DeclarationExpressions;
        public DeclarationStatement(DeclarationExpression[] declarationExpressions) : base(StatementType.Declaration)
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

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            RuntimeObject[] objects = new RuntimeObject[DeclarationExpressions.Length];
            for (int i = 0; i < DeclarationExpressions.Length; i++)
            {
                var declaration = DeclarationExpressions[i];
                objects[i] = declaration.Evaluate(instance);
            }
            return new Library.ArrayObject(objects, RuntimeType.Any);
        }
#endif

        public override string ToString()
        {
            return string.Concat("var ", string.Join(",", DeclarationExpressions.Select(e => e.ToString())));
        }
    }
}
