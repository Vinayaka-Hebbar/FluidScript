using System;
using FluidScript.Compiler.SyntaxTree;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Information about one or more code generation optimizations
    /// </summary>
    public sealed class OptimizationInfo
    {
        public OptimizationInfo()
        {

        }

        public Node SyntaxTree { get; set; }

        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter DebugDoument { get; set; }

        public string Source
        {
            get;
            set;
        }

        internal Type ToType(string typeName)
        {
            var type = Object.GetType(typeName);
            if (type == ObjectType.Object)
            {
                //Todo if typename is class type
                return typeof(object);
            }

            return TypeUtils.ToType(type);
        }
    }
}
