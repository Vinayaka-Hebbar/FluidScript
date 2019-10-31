using FluidScript.Compiler.Reflection;
using FluidScript.Compiler.SyntaxTree;
using System.Collections.Generic;

namespace FluidScript.Compiler.Scopes
{
    public class GlobalScope : Scope
    {
        private  List<DeclaredType> declaredTypes;
        public GlobalScope() : base(null, false)
        {
        }

        internal override DeclaredType DeclareType(Declaration declaration, BindingFlags binding)
        {
            if (declaredTypes == null)
                declaredTypes = new List<DeclaredType>();
            var type = new DeclaredType(declaration, declaredTypes.Count, binding);
            return type;
        }
    }
}
