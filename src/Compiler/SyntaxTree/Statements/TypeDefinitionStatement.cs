using System.Collections.Generic;

namespace FluidScript.Compiler.SyntaxTree
{
    public class TypeDefinitionStatement : Statement
    {
        public readonly TypeDeclaration Declaration;
        public readonly Scopes.ObjectScope Scope;

        public Reflection.DeclaredType DeclaredType;

        public readonly IList<Statement> Statements;
        public TypeDefinitionStatement(TypeDeclaration declaration, Statement[] statements, Scopes.ObjectScope scope, Reflection.DeclaredType type) : base(StatementType.Class)
        {
            Declaration = declaration;
            Scope = scope;
            Statements = new List<Statement>(statements);
            DeclaredType = type;

        }

        internal System.Reflection.Emit.TypeBuilder Generate(System.Reflection.Emit.ModuleBuilder builder)
        {
            var scope = Scope;
            var typeBuilder = builder.DefineType(Declaration.Name, System.Reflection.TypeAttributes.Public, builder.GetType(Declaration.BaseTypeName));
            var typeProvider = new Emit.TypeProvider((name, throwOnError) =>
            {
                if (Emit.TypeUtils.PrimitiveNames.ContainsKey(name))
                    return Emit.TypeUtils.PrimitiveNames[name].Type;
                return builder.GetType(name, throwOnError);
            });
            foreach (var member in scope.Members)
            {
                switch (member.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        var method = (Reflection.DeclaredMethod)member;
                        if (method.Declaration is FunctionDeclaration declaration)
                        {
                            method.Store = declaration.Declare(member, typeBuilder, typeProvider);
                        }
                        break;
                }
            }

            foreach (var member in scope.Members)
            {
                switch (member.MemberType)
                {
                    case System.Reflection.MemberTypes.Method:
                        var method = (Reflection.DeclaredMethod)member;
                        member.Generate(typeProvider);
                        break;
                }
            }
            return typeBuilder;
        }
    }
}
