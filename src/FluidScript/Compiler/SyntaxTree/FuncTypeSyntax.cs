using System;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FuncTypeSyntax : TypeSyntax
    {
        public readonly NodeList<TypeParameter> Parameters;

        public readonly TypeSyntax ReturnSyntax;

        public FuncTypeSyntax(NodeList<TypeParameter> parameters, TypeSyntax returnSyntax)
        {
            Parameters = parameters;
            ReturnSyntax = returnSyntax;
        }

        public override Type ResolveType(ITypeContext provider)
        {
            return Emit.DelegateGen.MakeNewDelegate(Parameters.Map(p => p.Type.ResolveType(provider)), ReturnSyntax.ResolveType(provider));
        }

        public override string ToString()
        {
            return $"({string.Join(",", Parameters)}) => {ReturnSyntax}";
        }
    }
}
