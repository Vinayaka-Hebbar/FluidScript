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
        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption options)
        {
            Type = VariableType != null ? VariableType.ResolveType((ITypeContext)generator.Context) : TypeProvider.ObjectType;
            if (Value != null)
            {
                var defValue = Value.Accept(generator);
                defValue.GenerateCode(generator);
                if (VariableType == null)
                    Type = defValue.Type;
                else if (!Runtime.TypeUtils.AreReferenceAssignable(Type, defValue.Type) && Runtime.TypeUtils.TryImplicitConvert(defValue.Type, Type, out System.Reflection.MethodInfo opConvert))
                    generator.CallStatic(opConvert);
                else if (defValue.Type.IsValueType && !Type.IsValueType)
                    generator.Box(defValue.Type);
            }
            //initialize
            var variable = generator.DeclareVariable(Type, Name);
            generator.StoreVariable(variable);
            return;
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
