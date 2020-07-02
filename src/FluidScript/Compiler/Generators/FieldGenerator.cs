using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Generators
{
    public class FieldGenerator : FieldInfo, Emit.IMember
    {
        private IList<AttributeGenerator> _customAttributes;

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

        public SyntaxTree.Expression DefaultValue { get; set; }

        public override string Name { get; }

        public SyntaxTree.VariableDeclarationExpression DeclarationExpression { get; }

        public MemberInfo MemberInfo => this;

        public FieldInfo FieldInfo { get; private set; }

        public override MemberTypes MemberType { get; }

        public override RuntimeFieldHandle FieldHandle => FieldInfo.FieldHandle;

        public override Type FieldType => FieldInfo.FieldType;

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
                return _customAttributes.Select(att => att.Instance).ToArray();
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_customAttributes != null)
            {
                return _customAttributes.Where(att => att.Type == attributeType).Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _customAttributes != null && _customAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
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
                    // literal ok
                    if (DefaultValue.NodeType != ExpressionType.Literal && MethodBody == null)
                        return;
                    if (MethodBody is null && DefaultValue == Expression.Null)
                    {
                        type = TypeProvider.ObjectType;
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

                var fieldBul = TypeGenerator.Builder.DefineField(Name, type, Attributes);
                if (_customAttributes != null)
                {
                    foreach (var attr in _customAttributes)
                    {
                        var cuAttr = new System.Reflection.Emit.CustomAttributeBuilder(attr.Ctor, attr.Parameters);
                        fieldBul.SetCustomAttribute(cuAttr);
                    }
                }
                FieldInfo = fieldBul;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
