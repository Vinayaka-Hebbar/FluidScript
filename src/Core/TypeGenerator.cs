namespace FluidScript.Core
{
    public class TypeGenerator
    {
        public readonly ScriptEngine engine;

        public TypeGenerator(ScriptEngine engine, IScriptSource source, Compiler.Metadata.Scope scope)
        {
            this.engine = engine;
            SyntaxVisitor = new Compiler.SyntaxVisitor(source, scope, engine.Settings);
        }

        public readonly Compiler.SyntaxVisitor SyntaxVisitor;

        public Compiler.SyntaxTree.TypeDefinitionStatement Generate()
        {
            SyntaxVisitor.Reset();
            SyntaxVisitor.MoveNext();
            if(SyntaxVisitor.TokenType == Compiler.TokenType.Identifier)
            {
                var name = SyntaxVisitor.GetName();
                if(name == "class")
                {
                    return SyntaxVisitor.VisitTypeDeclaration();

                }
            }
            return null;
        }
    }
}
