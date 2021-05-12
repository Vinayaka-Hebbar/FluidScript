using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace FluidScript.Compiler.Generators
{
    public class FieldGenerator : FieldInfo, Emit.IMember
    {
        private List<AttributeGenerator> _customAttributes;

        private readonly TypeGenerator TypeGenerator;

        public FieldGenerator(TypeGenerator builder, FieldAttributes attributes, SyntaxTree.VariableDeclarationExpression expression)
        {
            TypeGenerator = builder;
            Attributes = attributes;
            Name = expression.Name;
            DeclarationExpression = expression;
            DefaultValue = expression.Value;
            MemberType = MemberTypes.Field;
        }

        public override FieldAttributes Attributes { get; }

        public Expression DefaultValue { get; set; }

        public override string Name { get; }

        public VariableDeclarationExpression DeclarationExpression { get; }

        public MemberInfo MemberInfo => this;

        public FieldInfo FieldInfo { get; private set; }

        public override MemberTypes MemberType { get; }

        public override RuntimeFieldHandle FieldHandle => FieldInfo.FieldHandle;

        private Type fieldType;
        public override Type FieldType
        {
            get
            {
                return fieldType;
            }
        }

        public override Type DeclaringType => TypeGenerator;

        public override Type ReflectedType => TypeGenerator;

        internal Emit.MethodBodyGenerator MethodBody { get; set; }

        public virtual void SetCustomAttribute(Type type, ConstructorInfo ctor, params object[] parameters)
        {
            if (_customAttributes == null)
                _customAttributes = new List<AttributeGenerator>();
            _customAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (_customAttributes != null)
                return _customAttributes.Map(att => att.Instance);
            return new Attribute[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_customAttributes != null)
            {
                return _customAttributes
                    .FindAll(att => att.Type == attributeType)
                    .Map(att => att.Instance);
            }
            return new Attribute[0];
        }

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _customAttributes != null && _customAttributes.Exists(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        void Emit.IMember.Compile()
        {
            if (FieldInfo == null)
            {
                Type type;
                if (DeclarationExpression.VariableType == null)
                {
                    if (DefaultValue == null)
                        throw new ArgumentNullException(nameof(DefaultValue));
                    // must have a Body gen
                    if (MethodBody == null)
                        return;
                    if (DefaultValue == Expression.Null)
                    {
                        type = TypeProvider.AnyType;
                    }
                    else
                    {
                        type = DefaultValue.Accept(MethodBody).Type;
                    }

                }
                else
                {
                    type = DeclarationExpression.VariableType.ResolveType(TypeGenerator.Context);
                }
                fieldType = type;
                var fieldBuilder = TypeGenerator.Builder.DefineField(Name, type.UnderlyingSystemType, Attributes);
                if (_customAttributes != null)
                {
                    foreach (var attr in _customAttributes)
                    {
                        var cuAttr = new System.Reflection.Emit.CustomAttributeBuilder(attr.Ctor, attr.Parameters);
                        fieldBuilder.SetCustomAttribute(cuAttr);
                    }
                }
                FieldInfo = fieldBuilder;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
