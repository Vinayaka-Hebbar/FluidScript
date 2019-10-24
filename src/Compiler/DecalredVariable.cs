using FluidScript.Compiler.SyntaxTree;
using FluidScript.Core;

namespace FluidScript.Compiler
{
    public sealed class DecalredVariable
    {
        public readonly Scopes.Scope Scope;
        public readonly int Index;
        public readonly string Name;
        public Expression Expression;
        public Core.FieldAttributes Flags;
        public object Value = null;

        public readonly ObjectType Type = ObjectType.Object;

        public DecalredVariable(Scopes.Scope scope, int index, string name, Expression expression, FieldAttributes flags, ObjectType type)
        {
            Scope = scope;
            Index = index;
            Name = name;
            Expression = expression;
            Flags = flags;
            Type = type;
        }

        public DecalredVariable(Scopes.Scope scope, int index, string name, Expression expression, FieldAttributes flags)
        {
            Scope = scope;
            Index = index;
            Name = name;
            Expression = expression;
            Flags = flags;
        }

        internal bool IsWritable()
        {
            return (Flags & FieldAttributes.Writable) == FieldAttributes.Writable;
        }
    }
}
