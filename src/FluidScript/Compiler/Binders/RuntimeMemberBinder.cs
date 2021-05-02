using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Runtime;
using System;

namespace FluidScript.Compiler.Binders
{
    public
#if LATEST_VS
        readonly
#endif
        struct RuntimeMemberBinder : IBinder
    {
        private readonly IMemberBinder member;

        public RuntimeMemberBinder(IMemberBinder member)
        {
            this.member = member;
        }

        public BindingAttributes Attributes => BindingAttributes.None;

        public Type Type => member.Type;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option = MethodCompileOption.None)
        {
            throw new NotImplementedException();
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option = MethodCompileOption.None)
        {
            throw new NotImplementedException();
        }

        public object Get(object obj)
        {
            return member.Get(obj);
        }

        public void Set(object obj, object value)
        {
            member.Set(obj, value);
        }
    }
}
