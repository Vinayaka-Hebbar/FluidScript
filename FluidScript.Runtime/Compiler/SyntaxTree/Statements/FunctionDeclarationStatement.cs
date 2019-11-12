﻿namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDeclarationStatement : Statement
    {
        public readonly string Name;

        public readonly ArgumentInfo[] Arguments;

        public readonly Emit.TypeName ReturnTypeName;

        public readonly FunctionDeclaration Declaration;

        public FunctionDeclarationStatement(FunctionDeclaration declaration) : base(StatementType.Declaration)
        {
            Declaration = declaration;
            Name = declaration.Name;
            ReturnTypeName = declaration.TypeName;
            Arguments = declaration.Arguments;
        }

        protected FunctionDeclarationStatement(FunctionDeclaration declaration, StatementType nodeType) : base(nodeType)
        {
            Declaration = declaration;
            Name = declaration.Name;
            ReturnTypeName = declaration.TypeName;
            Arguments = declaration.Arguments;
        }

        public virtual System.Reflection.MethodInfo Create()
        {
            throw new System.Exception("Method not declared");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
