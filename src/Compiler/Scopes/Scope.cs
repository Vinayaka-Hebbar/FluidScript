using System;
using FluidScript.Compiler.Reflection;

namespace FluidScript.Compiler.Scopes
{
    public abstract class Scope : System.IDisposable
    {
        public readonly Scope ParentScope;
        private readonly SyntaxVisitor visitor;

        protected Scope(SyntaxVisitor visitor)
        {
            ParentScope = visitor.Scope;
            visitor.Scope = this;
        }

        /// <summary>
        /// Build the scoped
        /// </summary>
        public void Dispose()
        {
            Build();
            visitor.Scope = ParentScope;
        }


        public virtual void DeclareVariable(LocalVariableInfo variable)
        {
            throw new NotImplementedException(nameof(DecalredVariable));
        }

        protected virtual void Build()
        {

        }

        public virtual TypeInfo GetTypeInfo()
        {
            throw new NotImplementedException(nameof(GetTypeInfo));
        }

        public virtual ModuleInfo GetModuleInfo()
        {
            throw new NotImplementedException(nameof(GetModuleInfo));
        }

        public virtual AssemblyInfo GetAssemblyInfo()
        {
            throw new NotImplementedException(nameof(GetAssemblyInfo));
        }
    }
}
