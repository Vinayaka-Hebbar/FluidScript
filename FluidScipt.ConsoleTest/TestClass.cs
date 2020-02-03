using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace FluidScipt.ConsoleTest
{
    public class TestClass
    {
        public static void Run()
        {
            JsonDictionary<string, string> values = new JsonDictionary<string, string>();
           var type = values.GetType();
            var se = type.IsSerializable;
            values.Add("name", "vinayaka");
            var json = Json.Serialization.JsonConvert.Serialize(values);
            Console.WriteLine(json);
        }

        public static string Test<T>(Expression<Func<T, bool>> expression)
        {
            return Get(expression.Body);
        }

        private static string Get(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.AndAlso:
                    return GetBinaryLogical((BinaryExpression)expression, "and");
                case ExpressionType.OrAssign:
                    return GetBinaryLogical((BinaryExpression)expression, "or");
                case ExpressionType.Equal:
                    return GetBinary((BinaryExpression)expression, "=");
                case ExpressionType.NotEqual:
                    return GetBinary((BinaryExpression)expression, "!=");
                case ExpressionType.MemberAccess:
                    return GetMemberName((MemberExpression)expression);
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value.ToString();
            }
            throw new FormatException("Condition format invalid");
        }

        private static object GetValue(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    return GetMemberValue((MemberExpression)expression);
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value;
            }
            throw new FormatException("Condition format invalid");
        }

        private static string GetMemberName(MemberExpression expression)
        {
            var member = expression.Member;
            var attr = (DataMemberAttribute)Attribute.GetCustomAttribute(member, typeof(DataMemberAttribute));
            if (attr != null && attr.Name != null)
            {
                return attr.Name;
            }
            return member.Name;
        }

        private static object GetMemberValue(MemberExpression memberAcess)
        {
            var expression = memberAcess.Expression;
            object instance = GetValue(expression);
            var member = memberAcess.Member;
            switch (member)
            {
                case System.Reflection.FieldInfo field:
                    return field.GetValue(instance);
                case System.Reflection.PropertyInfo property:
                    return property.GetValue(instance, new object[0]);
            }
            return string.Empty;
        }

        private static string GetBinaryLogical(BinaryExpression binary, string relation)
        {
            var left = binary.Left;
            var right = binary.Right;
            return string.Concat(Get(left), " ", relation, " ", Get(right));
        }

        private static string GetBinary(BinaryExpression binary, string operation)
        {
            var left = binary.Left;
            var right = binary.Right;
            return string.Concat(Get(left), " ", operation, " ", GetValue(right));
        }

        public class User
        {
            [DataMember(Name = "name")]
            internal string Name;
            [DataMember(Name = "isActive")]
            internal bool IsActive;
            [DataMember(Name = "age")]
            public int Age { get; set; }
        }
    }
}
