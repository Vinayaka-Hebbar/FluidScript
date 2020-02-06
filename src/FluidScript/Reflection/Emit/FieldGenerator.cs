using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    public class FieldGenerator : System.Reflection.FieldInfo, IMemberGenerator
    {
        private IList<AttributeGenerator> _CustomAttributes;

        private readonly TypeGenerator TypeGenerator;

        public FieldGenerator(TypeGenerator builder, System.Reflection.FieldAttributes attributes, Compiler.SyntaxTree.VariableDeclarationExpression expression)
        {
            TypeGenerator = builder;
            Attributes = attributes;
            Name = expression.Name;
            DeclarationExpression = expression;
            DefaultValue = expression.Value;
            MemberType = System.Reflection.MemberTypes.Field;
        }

        public override System.Reflection.FieldAttributes Attributes { get; }

        public Compiler.SyntaxTree.Expression DefaultValue { get; }

        public override string Name { get; }

        public Compiler.SyntaxTree.VariableDeclarationExpression DeclarationExpression { get; }

        public System.Reflection.MemberInfo MemberInfo => this;

        public System.Reflection.FieldInfo FieldInfo { get; private set; }

        public override System.Reflection.MemberTypes MemberType { get; }

        public override RuntimeFieldHandle FieldHandle => FieldInfo.FieldHandle;

        public override Type FieldType => FieldInfo.FieldType;

        public override Type DeclaringType => TypeGenerator;

        public override Type ReflectedType => TypeGenerator;

        internal MethodBodyGenerator MethodBody { get; set; }

        public bool BindingFlagsMatch(System.Reflection.BindingFlags flags)
        {
            return Utils.TypeUtils.BindingFlagsMatch(IsPublic, flags, System.Reflection.BindingFlags.Public, System.Reflection.BindingFlags.NonPublic)
                && Utils.TypeUtils.BindingFlagsMatch(IsStatic, flags, System.Reflection.BindingFlags.Static, System.Reflection.BindingFlags.Instance);
        }

        public virtual void SetCustomAttribute(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters)
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

        public Type GetType(string typeName)
        {
            if (Utils.TypeUtils.IsInbuiltType(typeName))
                return Utils.TypeUtils.GetInbuiltType(typeName);
            return TypeGenerator.Module.GetType(typeName);
        }

        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _CustomAttributes != null && _CustomAttributes.Any(attr => attr.Type == attributeType || (inherit && attr.Type.IsAssignableFrom(attributeType)));
        }

        public override void SetValue(object obj, object value, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void Build()
        {
            if (FieldInfo == null)
            {
                System.Type type;
                if (DeclarationExpression.VariableType == null)
                {
                    if (DefaultValue == null)
                        throw new System.ArgumentNullException(nameof(DefaultValue));
                    //literal ok
                    if (DefaultValue.NodeType != Compiler.SyntaxTree.ExpressionType.Literal && MethodBody == null)
                        return;
                    type = DefaultValue.Accept(MethodBody).Type;

                }
                else
                    type = DeclarationExpression.VariableType.GetType(TypeGenerator);
                var fieldBul = TypeGenerator.GetBuilder().DefineField(Name, type, Attributes);
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
