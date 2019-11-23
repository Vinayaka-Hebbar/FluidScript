using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class BlockExpression : Expression
    {
        public readonly Statement[] Statements;
        public readonly Metadata.ObjectPrototype Prototype;
        public BlockExpression(Statement[] expressions, Metadata.ObjectPrototype prototype) : base(ExpressionType.Block)
        {
            Statements = expressions;
            Prototype = prototype;
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var local = new Core.LocalInstance(Prototype, instance);
            foreach (var statement in Statements)
            {
                statement.Evaluate(local);
            }
            return local;
        }
#endif

        public override string ToString()
        {
            return string.Concat("{", string.Join(",", Statements.Select(s => s.ToString())), "}");
        }
    }
}
