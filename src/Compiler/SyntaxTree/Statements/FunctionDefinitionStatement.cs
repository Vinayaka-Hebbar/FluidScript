using System.Collections.Generic;
using System.Linq;
using FluidScript.Compiler.Emit;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDefinitionStatement : FunctionDeclarationStatement
    {
        public readonly Scopes.DeclarativeScope Scope;

        internal readonly Reflection.DeclaredMember Member;

        public BlockStatement Body { get; }

        internal FunctionDefinitionStatement(FunctionDeclaration declaration, BlockStatement body, Reflection.DeclaredMember member) : base(declaration, StatementType.Function)
        {
            Body = body;
            Scope = declaration.Scope;
            Member = member;
        }

        public override bool Equals(object obj)
        {
            return Name.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }

        public override void GenerateCode(ILGenerator generator, MethodOptimizationInfo info)
        {
            Body.GenerateCode(generator, info);
            if (info.ReturnTarget != null)
                generator.DefineLabelPosition(info.ReturnTarget);
            if (info.ReturnVariable != null)
                generator.LoadVariable(info.ReturnVariable);
        }

        public override int GetHashCode()
        {
            var hashCode = 1062545247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<ArgumentInfo[]>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<Statement>.Default.GetHashCode(Body);
            hashCode = hashCode * -1521134295 + NodeType.GetHashCode();
            return hashCode;
        }
    }
}
