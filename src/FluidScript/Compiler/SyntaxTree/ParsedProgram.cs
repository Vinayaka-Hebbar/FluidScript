using FluidScript.Compiler.Emit;
using System.Collections.Generic;
using System.Reflection;

namespace FluidScript.Compiler.SyntaxTree
{
    public class ParsedProgram : Node
    {
        Dictionary<string, NodeList<TypeImport>> imports;
        public Dictionary<string, NodeList<TypeImport>> Imports
        {
            get
            {
                if (imports == null)
                    imports = new Dictionary<string, NodeList<TypeImport>>();
                return imports;
            }
        }

        public readonly NodeList<MemberDeclaration> Members;

        public ParsedProgram()
        {
            Members = new NodeList<MemberDeclaration>();
        }

        public Assembly Compile(string name)
        {
            AssemblyGen assembly = new AssemblyGen(name, "1.0");
            Compile(assembly);
            return assembly;
        }

        public void Compile(AssemblyGen assembly)
        {
            if (imports != null)
            {
                foreach (var lib in Imports)
                {
                    TypeContext.Register(assembly.Context, lib.Key, lib.Value);
                }
            }
            foreach (var member in Members)
            {
                switch (member.DeclarationType)
                {
                    case DeclarationType.Class:
                        ((TypeDeclaration)member).Compile(assembly);
                        break;
                }
            }
        }

        internal void Import(string lib, NodeList<TypeImport> imports)
        {
            if (Imports.TryGetValue(lib, out NodeList<TypeImport> types))
            {
                foreach (var import in imports)
                {
                    types.Add(import);
                }
            }
            else
            {
                Imports.Add(lib, imports);
            }
        }
    }
}
