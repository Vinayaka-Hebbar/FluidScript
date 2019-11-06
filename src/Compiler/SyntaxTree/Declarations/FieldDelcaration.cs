using FluidScript.Compiler.Emit;
using System.Reflection.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FieldDelcaration : Declaration
    {
        public FieldDelcaration(string name, System.Type type) : base(name)
        {
            ResolvedType = type;
        }

        public FieldDelcaration(string name, TypeName typeName) : base(name, typeName)
        {
        }

        internal FieldBuilder Declare(Reflection.DeclaredMember member, System.Reflection.Emit.TypeBuilder builder, Emit.OptimizationInfo info)
        {
            if (ResolvedType == null)
            {
                if (TypeName.FullName == null)
                {
                    if (member.ValueAtTop != null)
                    {
                        if (member.ValueAtTop.NodeType == StatementType.Expression)
                        {
                            var statement = (ExpressionStatement)member.ValueAtTop;
                            ResolvedType = statement.Expression.ResultType(info);
                        }
                    }
                }
                else
                {
                    TryResolveType(info);
                }
            }
            return builder.DefineField(Name, ResolvedType, System.Reflection.FieldAttributes.Private);
        }
    }
}
