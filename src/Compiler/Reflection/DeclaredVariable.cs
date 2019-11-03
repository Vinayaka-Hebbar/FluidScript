
using System;

namespace FluidScript.Compiler.Reflection
{
    public sealed class DeclaredVariable
    {
        public readonly string Name;
        public readonly int Index;
        public readonly SyntaxTree.Expression ValueAtTop;
        public readonly VariableType VariableType;

        public Emit.ILLocalVariable Store;

        public PrimitiveType PrimitiveType;
        private System.Type type;
        private Emit.TypeName typeName;

        public DeclaredVariable(string name, Emit.TypeName typeName, int index, SyntaxTree.Expression valueAtTop, VariableType variableType )
        {
            Name = name;
            TypeName = typeName;
            Index = index;
            ValueAtTop = valueAtTop;
            VariableType = variableType;
        }

        public System.Type GetType(Emit.OptimizationInfo info)
        {
            if (type == null && typeName.FullName != null)
            {
                //this is not a primitive
                type = info.GetType(TypeName);
            }

            return type;
        }


        public Emit.TypeName TypeName
        {
            get => typeName;
            set
            {
                typeName = value;
                if (value.FullName != null)
                {
                    var primitive = Emit.TypeUtils.From(value.FullName);
                    type = primitive.Type;
                    PrimitiveType = primitive.Enum;
                }
            }
        }

        /// <summary>
        /// will resolve the delcared type if not declared type there
        /// </summary>
        /// <param name="type"></param>
        internal void ResolveType(Type type)
        {
            this.type = type;
            if (type.IsPrimitive)
            {
                PrimitiveType = Emit.TypeUtils.PrimitiveTypes[type];
            }
            else
            {
                PrimitiveType = PrimitiveType.Any;
            }
        }
    }
}
