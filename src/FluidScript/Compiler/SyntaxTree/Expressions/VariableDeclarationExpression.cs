using FluidScript.Runtime;

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
        public override void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodCompileOption options)
        {
            Type = VariableType != null ? VariableType.ResolveType(generator.Context) : TypeProvider.AnyType;
            if (Value != null)
            {
                var defValue = Value.Accept(generator);
                defValue.GenerateCode(generator, AssignOption);
                if (VariableType == null)
                {
                    Type = defValue.Type;
                }
                else if (!TypeUtils.AreReferenceAssignable(Type, defValue.Type) && defValue.Type.TryImplicitConvert(Type, out System.Reflection.MethodInfo opConvert))
                {
                    // When converting value type to Any, must do Box 
                    if (defValue.Type.IsValueType && opConvert.GetParameters()[0].ParameterType.IsValueType == false)
                        generator.Box(defValue.Type);
                    generator.CallStatic(opConvert);
                }
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
