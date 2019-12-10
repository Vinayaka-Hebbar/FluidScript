using System;

namespace FluidScript.Reflection.Emit
{
    public abstract class Binding
    {
        public abstract void GenerateGet(MethodBodyGenerator generator);

        public abstract void GenerateSet(MethodBodyGenerator generator);

        public abstract Type Type { get; }

        public abstract bool IsMember { get; } 
    }

    public sealed class VariableBinding : Binding
    {
        private readonly ILLocalVariable _variable;


        public VariableBinding(ILLocalVariable variable)
        {
            _variable = variable;
            Type = variable.Type;
        }

        public override bool IsMember { get; } = false;

        public override Type Type { get; }

        public override void GenerateGet(MethodBodyGenerator generator)
        {
            generator.LoadVariable(_variable);
        }

        public override void GenerateSet(MethodBodyGenerator generator)
        {
            generator.StoreVariable(_variable);
        }
    }

    public sealed class ArgumentBinding : Binding
    {
        private readonly ParameterInfo _parameter;

        public ArgumentBinding(ParameterInfo parameter)
        {
            _parameter = parameter;
            Type = parameter.Type;
        }

        public override bool IsMember { get; } = false;

        public override Type Type { get; }

        public override void GenerateGet(MethodBodyGenerator generator)
        {
            if (generator.Method.IsStatic)
                generator.LoadArgument(_parameter.Index);
            else
                generator.LoadArgument(_parameter.Index + 1);
        }

        public override void GenerateSet(MethodBodyGenerator generator)
        {
            if (generator.Method.IsStatic)
                generator.StoreArgument(_parameter.Index);
            else
                generator.StoreArgument(_parameter.Index + 1);
        }
    }

    public sealed class FieldBinding : Binding
    {
        private readonly System.Reflection.FieldInfo _field;

        public FieldBinding(System.Reflection.FieldInfo field)
        {
            _field = field;
            Type = _field.FieldType;
        }

        public override bool IsMember { get; } = true;

        public override Type Type { get; }

        public override void GenerateGet(MethodBodyGenerator generator)
        {
            var field = _field;
            if (field.FieldType == null)
                throw new Exception(string.Concat("Use of undeclared field ", field));
            if (field is FieldGenerator)
                field = ((FieldGenerator)field).FieldInfo;
            generator.LoadField(field);
        }

        public override void GenerateSet(MethodBodyGenerator generator)
        {
            var field = _field;
            if (field is FieldGenerator)
                field = ((FieldGenerator)field).FieldInfo;
            generator.StoreField(field);
        }
    }

    public sealed class PropertyBinding : Binding
    {
        private readonly System.Reflection.PropertyInfo _property;
        public PropertyBinding(System.Reflection.PropertyInfo property)
        {
            _property = property;
            Type = property.PropertyType;
        }

        public override bool IsMember { get; } = true;

        public override Type Type { get; }

        public override void GenerateGet(MethodBodyGenerator generator)
        {
            var get = _property.GetGetMethod(true);
            if (get is IMethodBaseGenerator)
                get = (System.Reflection.MethodInfo)((IMethodBaseGenerator)get).MethodBase;
            generator.Call(get);
        }

        public override void GenerateSet(MethodBodyGenerator generator)
        {
            var set = _property.GetSetMethod(true);
            if (set is IMethodBaseGenerator)
                set = (System.Reflection.MethodInfo)((IMethodBaseGenerator)set).MethodBase;
            generator.Call(set);
        }
    }
}
