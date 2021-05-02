using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using System;

namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Base Expression
    /// </summary>
    public class Expression : Node
    {
        internal const MethodCompileOption AssignOption = MethodCompileOption.Return | MethodCompileOption.Dupplicate;
        /// <summary>
        /// Empty Expression
        /// </summary>
        public static readonly Expression Empty = new EmptyExpression();

        static Expression m_null;
        public static Expression Null
        {
            get
            {
                if (m_null == null)
                    m_null = new NullExpression();
                return m_null;
            }
        }

        public static readonly Expression[] EmptyList = new Expression[0];

        static LiteralExpression m_true;
        public static LiteralExpression True
        {
            get
            {
                if (m_true == null)
                    m_true = new LiteralExpression(true);
                return m_true;
            }
        }

        static LiteralExpression m_false;
        public static LiteralExpression False
        {
            get
            {
                if (m_false == null)
                    m_false = new LiteralExpression(false);
                return m_false;
            }
        }

        static LiteralExpression m_NaN;
        public static LiteralExpression NaN
        {
            get
            {
                if (m_NaN == null)
                    m_NaN = new LiteralExpression(double.NaN);
                return m_NaN;
            }
        }

        public Expression(ExpressionType nodeType)
        {
            NodeType = nodeType;
        }

        public ExpressionType NodeType { get; }

        public Type Type { get; set; }

        /// <summary>
        /// Optimizes expression for emit or others
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public virtual TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.Default(this);
        }

        /// <summary>
        /// Generates compiled IL code for <see cref="Expression"/>
        /// </summary>
        /// <param name="generator">IL Generator for method body</param>
        /// <param name="option">Compiler option's for generating <see cref="Expression"/></param>
        public virtual void GenerateCode(MethodBodyGenerator generator, MethodCompileOption option = MethodCompileOption.None)
        {
            generator.NoOperation();
        }

        #region Static
        public static InvocationExpression Call(Expression instance, System.Reflection.MethodBase method, params Expression[] arguments)
        {
            NodeList<Expression> args = new NodeList<Expression>(arguments);
            InvocationExpression exp = new InvocationExpression(instance, args)
            {
                Method = method,
                Conversions = new Runtime.ArgumentConversions(args.Count)
            };
            if (!method.MatchesArgumentTypes(args.Map(ex => ex.Type), exp.Conversions))
                throw new InvalidOperationException("argument miss match");
            return exp;
        }

        public static InvocationExpression Call(Expression instance, System.Reflection.MethodInfo method, params Expression[] arguments)
        {
            NodeList<Expression> args = new NodeList<Expression>(arguments);
            InvocationExpression exp = new InvocationExpression(instance, args)
            {
                Method = method,
                Conversions = new Runtime.ArgumentConversions(args.Count),
                Type = method.ReturnType
            };
            if (!method.MatchesArgumentTypes(args.Map(ex => ex.Type), exp.Conversions))
                throw new InvalidOperationException("Arguments miss match");
            return exp;
        }

        public static ThisExpression This()
        {
            return new ThisExpression();
        }

        public static SuperExpression Super()
        {
            return new SuperExpression();
        }

        public static InstanceOfExpression IsInstanceOf(Expression target, Type type)
        {
            return new InstanceOfExpression(target, TypeSyntax.Create(type));
        }

        public static NameExpression Member(System.Reflection.MemberInfo member)
        {
            NameExpression exp = new NameExpression(member.Name, ExpressionType.Identifier);
            if (member.MemberType == System.Reflection.MemberTypes.Property)
            {
                exp.Binder = new Binders.PropertyBinder((System.Reflection.PropertyInfo)member);
            }
            else if (member.MemberType == System.Reflection.MemberTypes.Field)
            {
                exp.Binder = new Binders.FieldBinder((System.Reflection.FieldInfo)member);
            }
            else
            {
                throw new InvalidOperationException("Invalid member type " + member.MemberType);
            }
            return exp;
        }

        public static NameExpression Member(string name)
        {
            return new NameExpression(name, ExpressionType.Identifier);
        }

        public static AssignmentExpression Assign(Expression left, Expression right)
        {
            return new AssignmentExpression(left, right);
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
        {
            return new BinaryExpression(left, right, binaryType);
        }

        public static LiteralExpression Literal(object value)
        {
            return new LiteralExpression(value);
        }

        public static SystemLiternalExpression SystemLiteral(object value)
        {
            return new SystemLiternalExpression(value);
        }

        public static NewExpression New(Type type, params Expression[] args)
        {
            return new NewExpression(TypeSyntax.Create(type), new NodeList<Expression>(args));
        }

        public static ConvertExpression Convert(Expression target, Type type)
        {
            return new ConvertExpression(TypeSyntax.Create(type), target);
        }

        public static NameExpression Parameter(string name, Type type)
        {
            return new NameExpression(name, ExpressionType.Identifier) { Type = type };
        }

        public static NameExpression Parameter(ParameterInfo parameter)
        {
            return new NameExpression(parameter.Name, ExpressionType.Identifier) { Type = parameter.Type, Binder = new Binders.ParameterBinder(parameter) };
        }

        /// <summary>
        /// Custom IL Generation
        /// </summary>
        public static Expression Custom(Action<Expression, MethodBodyGenerator> custom)
        {
            return new CustomExpression(custom);
        }

        public static implicit operator ExpressionStatement(Expression expression)
        {
            return new ExpressionStatement(expression);
        }
        #endregion
    }

    internal sealed class EmptyExpression : Expression
    {
        public EmptyExpression() : base(ExpressionType.Unknown)
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
