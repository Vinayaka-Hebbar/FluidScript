namespace FluidScript.Reflection.Emit
{
    public class FieldGenerator : IMemberGenerator, ITypeProvider
    {
        private readonly System.Reflection.Emit.TypeBuilder _builder;

        public FieldGenerator(System.Reflection.Emit.TypeBuilder builder, System.Reflection.FieldAttributes attributes, Compiler.SyntaxTree.VariableDeclarationExpression expression)
        {
            _builder = builder;
            Attributes = attributes;
            Name = expression.Name;
            DeclarationExpression = expression;
            DefaultValue = expression.Value;
            IsStatic = (attributes & System.Reflection.FieldAttributes.Static) == System.Reflection.FieldAttributes.Static;
            IsPublic = (attributes & System.Reflection.FieldAttributes.Public) == System.Reflection.FieldAttributes.Public;
            MemberType = System.Reflection.MemberTypes.Field;
        }

        public System.Reflection.FieldAttributes Attributes { get; }

        public Compiler.SyntaxTree.Expression DefaultValue { get; }

        public string Name { get; }

        public Compiler.SyntaxTree.VariableDeclarationExpression DeclarationExpression { get; }

        public System.Reflection.MemberInfo MemberInfo => FieldInfo;

        public System.Reflection.FieldInfo FieldInfo { get; private set; }

        public System.Reflection.MemberTypes MemberType { get; }

        public bool IsStatic { get; }

        public bool IsPublic { get; }

        internal MethodBodyGenerator MethodBody { get; set; }

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                && TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public void Build()
        {
            if (FieldInfo == null)
            {
                System.Type type;
                if (DeclarationExpression.Type == null)
                {
                    if (DefaultValue == null)
                        throw new System.ArgumentNullException(nameof(DefaultValue));
                    //literal ok
                    if (DefaultValue.NodeType != Compiler.SyntaxTree.ExpressionType.Literal && MethodBody == null)
                        return;
                    type = DefaultValue.ResultType(MethodBody);

                }
                else
                    type = DeclarationExpression.Type.GetTypeInfo().ResolvedType(this);
                FieldInfo = _builder.DefineField(Name, type, Attributes);
            }
        }

        public System.Type GetType(string typeName)
        {
            if (TypeUtils.IsInbuiltType(typeName))
                return TypeUtils.GetInbuiltType(typeName);
            return _builder.Module.GetType(typeName);
        }
    }
}
