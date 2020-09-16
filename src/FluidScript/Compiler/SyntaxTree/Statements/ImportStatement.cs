using FluidScript.Compiler.Emit;
using System;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ImportStatement : Statement
    {
        public readonly string Library;
        public readonly INodeList<TypeImport> Imports;

        public ImportStatement(string library, INodeList<TypeImport> imports) : base(StatementType.Import)
        {
            Library = library;
            Imports = imports;
        }

        protected internal override void Accept(IStatementVisitor visitor)
        {
            visitor.VisitImport(this);
        }

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            TypeContext.Register(generator.Context, Library, Imports);
        }
    }
}
