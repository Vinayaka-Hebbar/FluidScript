using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Reflection;

namespace FluidScript.Compiler.Binders
{
    #region Field Binder
    public
#if LATEST_VS
        readonly
#endif
        struct FieldBinder : IBinder
    {
        readonly FieldInfo field;

        public FieldBinder(FieldInfo field)
        {
            this.field = field;
        }

        public Type Type => field.FieldType;

        public BindingAttributes Attributes
        {
            get
            {
                return BindingAttributes.Member | (field.IsStatic ? BindingAttributes.None : BindingAttributes.HasThis);
            }
        }

        public void GenerateGet(Expression target, MethodBodyGenerator generator, MethodCompileOption option)
        {
            var field = this.field;
            if (field.FieldType == null)
                throw new InvalidOperationException(string.Concat("Use of undeclared field ", field));
            if (field is Generators.FieldGenerator)
                field = ((Generators.FieldGenerator)field).FieldInfo;
            if ((option & MethodCompileOption.EmitStartAddress) == MethodCompileOption.EmitStartAddress && field.FieldType.IsValueType)
                generator.LoadFieldAddress(field);
            else
                generator.LoadField(field);
        }

        public void GenerateSet(Expression value, MethodBodyGenerator generator, MethodCompileOption option)
        {
            var field = this.field;
            if (field.IsInitOnly && !(generator.Method is ConstructorInfo))
                throw new FieldAccessException("A readonly field cannot be assigned to (except in a constructor of the class in which the field is defined or a variable initializer))");
            if (field is Generators.FieldGenerator)
                field = ((Generators.FieldGenerator)field).FieldInfo;
            if((option & MethodCompileOption.Dupplicate) == 0)
            {
                generator.StoreField(field);
                return;
            }
            generator.Duplicate();
            var temp = generator.CreateTemporaryVariable(value.Type);
            generator.StoreVariable(temp);
            generator.StoreField(field);
            generator.LoadVariable(temp);


        }

        public object Get(object obj)
        {
            return field.GetValue(obj);
        }

        public void Set(object obj, object value)
        {
            field.SetValue(obj, value);
        }
    }
    #endregion

}
