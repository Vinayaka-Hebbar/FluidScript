using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace FluidScript.Compiler.Generators
{
    public class FieldGenerator : FieldInfo, Emit.IMemberGenerator
    {
        private IList<AttributeGenerator> _CustomAttributes;

        private readonly TypeGenerator TypeGenerator;

        public FieldGenerator(TypeGenerator builder, FieldAttributes attributes, Compiler.SyntaxTree.VariableDeclarationExpression expression)
        {
            TypeGenerator = builder;
            Attributes = attributes;
            Name = expression.Name;
            DeclarationExpression = expression;
            DefaultValue = expression.Value;
            MemberType = MemberTypes.Field;
        }

        public override FieldAttributes Attributes { get; }

        public SyntaxTree.Expression DefaultValue { get; }

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

        public virtual void SetCustomAttribute(Type type, ConstructorInfo ctor, object[] parameters)
        {
            if (_CustomAttributes == null)
                _CustomAttributes = new List<AttributeGenerator>();
            _CustomAttributes.Add(new AttributeGenerator(type, ctor, parameters, null, null));
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (_CustomAttributes != null)
                return _CustomAttributes.Select(att => att.Instance).ToArray();
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (_CustomAttributes != null)
            {
                var enumerable = _CustomAttributes.Where(att => att.Type == attributeType);
                return enumerable.Select(att => att.Instance).ToArray();
            }
            return new object[0];
        }

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _CustomAttributes != null && _CustomAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void Generate()
        {
            if (FieldInfo == null)
            {
                Type type;
                if (DeclarationExpression.VariableType == null)
                {
                    if (DefaultValue == null)
                        throw new ArgumentNullException(nameof(DefaultValue));
                    //literal ok
                    if (DefaultValue.NodeType != SyntaxTree.ExpressionType.Literal && MethodBody == null)
                        return;
                    type = DefaultValue.Accept(MethodBody).Type;

                }
                else
                    type = DeclarationExpression.VariableType.GetType(TypeGenerator.Context);
                var fieldBul = TypeGenerator.Builder.DefineField(Name, type, Attributes);
                if (_CustomAttributes != null)
                {
                    foreach (var attr in _CustomAttributes)
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
