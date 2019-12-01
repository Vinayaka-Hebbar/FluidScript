using FluidScript.Reflection.Emit;
using FluidScript.Compiler.Metadata;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class LocalDeclarationStatement : Statement
    {
        public readonly VariableDeclarationExpression[] DeclarationExpressions;
        public readonly bool IsReadOnly;

        public LocalDeclarationStatement(VariableDeclarationExpression[] declarationExpressions, bool isReadOnly) : base(StatementType.Declaration)
        {
            DeclarationExpressions = declarationExpressions;
            IsReadOnly = isReadOnly;
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            foreach (var declaration in DeclarationExpressions)
            {
                declaration.GenerateCode(generator);
            }
        }

#if Runtime
        internal override RuntimeObject Evaluate(RuntimeObject instance, Prototype prototype)
        {
            RuntimeObject[] objects = new RuntimeObject[DeclarationExpressions.Length];
            for (int i = 0; i < DeclarationExpressions.Length; i++)
            {
                var declaration = DeclarationExpressions[i];
                //todo remove value at top
                Reflection.ITypeInfo type = declaration.Type == null ? Reflection.TypeInfo.Any : declaration.Type.GetTypeInfo();
                prototype.DeclareLocalVariable(declaration.Name, type, declaration.Value);
                instance.Append(declaration.Name, objects[i] = declaration.Evaluate(instance), IsReadOnly);
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
