using FluidScript.Compiler.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    var assemblyImport = Assembly.ReflectionOnlyLoad(lib.Key);
                    foreach (var import in lib.Value)
                    {
                        TypeName name = import.Name;
                        Type type;
                        if (name.Namespace == null)
                        {
                            type = assemblyImport.GetTypes().FirstOrDefault(t => t.Name == import.Name);
                        }
                        else
                        {
                            type = assemblyImport.GetType(name.FullName);
                        }
                        assembly.Context.Register(import.ToString(), type);
                    }
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

        internal void Import(string lib, TypeImport[] imports)
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
                types = new NodeList<TypeImport>(imports);
                Imports.Add(lib, types);
            }
        }
    }
}
