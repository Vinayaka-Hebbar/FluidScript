namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class VariableDeclarationExpression : DeclarationExpression
    {
        public readonly TypeSyntax VariableType;
        public readonly Expression Value;

        public VariableDeclarationExpression(string name, TypeSyntax type, Expression value) : base(name)
        {
            VariableType = type;
            Value = value;
        }

        /// <inheritdoc/>
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitDeclaration(this);
        }

        /// <inheritdoc/>
        public override void GenerateCode(Reflection.Emit.MethodBodyGenerator generator)
        {
            //initialize
            if (Value != null)
            {
                var defValue = Value.Accept(generator);
                System.Type type = VariableType == null ? defValue.Type : VariableType.GetType(generator.TypeGenerator);
                defValue.GenerateCode(generator);
                var variable = generator.DeclareVariable(type, Name);
                generator.StoreVariable(variable);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            //todo for not runtime
            string value = "null";
            if (Value != null)
            {
                value = Value.ToString();
            }

            return string.Concat(Name, VariableType == null ? null : string.Concat(":", VariableType), "=", value);
        }
    }
}
