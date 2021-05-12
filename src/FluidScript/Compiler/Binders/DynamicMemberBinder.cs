using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;

namespace FluidScript.Compiler.Binders
{
    internal struct DynamicMemberBinder : IBinder
    {
        public readonly string Name;

        public DynamicMemberBinder(string name)
        {
            Name = name;
        }

        public Type Type => TypeProvider.AnyType;

        public BindingAttributes Attributes => BindingAttributes.Dynamic;

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option = MethodCompileOption.None)
        {
            if (target != null && target.Type.IsValueType)
                generator.Box(target.Type);
            generator.LoadString(Name);
            generator.Call(typeof(IDynamicInvocable).GetInstanceMethod(nameof(IDynamicInvocable.SafeGetValue), typeof(string)));
            // since it is a value type load the address
            if ((option & MethodCompileOption.EmitStartAddress) == MethodCompileOption.EmitStartAddress)
            {
                var temp = generator.DeclareVariable(TypeProvider.AnyType);
                generator.StoreVariable(temp);
                generator.LoadAddressOfVariable(temp);
            }
        }

        public void GenerateSet(Expression right, MethodBodyGenerator generator, MethodCompileOption option = MethodCompileOption.None)
        {
            generator.LoadString(Name);
            generator.Call(typeof(IDynamicInvocable).GetInstanceMethod(nameof(IDynamicInvocable.SafeSetValue), TypeProvider.AnyType, typeof(string)));
            if ((option & MethodCompileOption.Return) == 0)
            {
                generator.Pop();
                return;
            }
        }

        public object Get(object obj)
        {
            throw new NotImplementedException();
        }

        public void Set(object obj, object value)
        {
            throw new NotImplementedException();
        }
    }
}