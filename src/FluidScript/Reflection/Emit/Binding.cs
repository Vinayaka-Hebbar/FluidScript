using System;

namespace FluidScript.Reflection.Emit
{
    public abstract class Binding
    {
        public abstract void GenerateGet(MethodBodyGenerator generator);

        public abstract void GenerateSet(MethodBodyGenerator generator);

        public abstract Type Type { get; }

        public abstract bool IsMember { get; } 

        public abstract bool IsStatic { get; }
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

        public override bool IsStatic { get; } = false;

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

        public override bool IsStatic { get; } = false;

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

        public override bool IsStatic => _field.IsStatic;

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

        private System.Reflection.MethodInfo _getter;
        public System.Reflection.MethodInfo Getter
        {
            get
            {
                if (_getter == null)
                    _getter = _property.GetGetMethod(true);
                return _getter;
            }
        }

        private System.Reflection.MethodInfo _setter;
        public System.Reflection.MethodInfo Setter
        {
            get
            {
                if (_setter == null)
                    _setter = _property.GetSetMethod(true);
                return _setter;
            }
        }

        public override bool IsStatic
        {
            get
            {
                if (Getter != null)
                    return Getter.IsStatic;
                if (Setter != null)
                    return Setter.IsStatic;
                return false;
            }
        }
        public override Type Type { get; }

        public override void GenerateGet(MethodBodyGenerator generator)
        {
            var get = Getter;
            if (get is IMethodBaseGenerator)
                get = (System.Reflection.MethodInfo)((IMethodBaseGenerator)get).MethodBase;
            generator.Call(get);
        }

        public override void GenerateSet(MethodBodyGenerator generator)
        {
            var set = Setter;
            if (set is IMethodBaseGenerator)
                set = (System.Reflection.MethodInfo)((IMethodBaseGenerator)set).MethodBase;
            generator.Call(set);
        }
    }
}
