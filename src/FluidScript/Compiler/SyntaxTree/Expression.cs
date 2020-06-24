namespace FluidScript.Compiler.SyntaxTree
{
    /// <summary>
    /// Base Expression
    /// </summary>
    public class Expression : Node
    {
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

        public System.Type Type { get; protected internal set; }

        /// <summary>
        /// Optimizes expression for emit or others
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public virtual TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
#if LATEST_VS 
            return default;
#else
            return default(TResult);
#endif
        }

        /// <summary>
        /// Generates IL code for <see cref="Expression"/>
        /// </summary>
        /// <param name="generator"></param>
        public virtual void GenerateCode(Emit.MethodBodyGenerator generator, Emit.MethodGenerateOption option = Emit.MethodGenerateOption.None)
        {
            generator.NoOperation();
        }

        #region Static
        public static InvocationExpression Call(Expression instance, System.Reflection.MethodInfo method, params Expression[] arguments)
        {
            NodeList<Expression> args = new NodeList<Expression>(arguments);
            InvocationExpression exp = new InvocationExpression(instance, args)
            {
                Method = method,
                Convertions = new Binders.ArgumentConversions(args.Count)
            };
            if (!Utils.TypeUtils.MatchesTypes(method, args.Map(ex => ex.Type), exp.Convertions))
                throw new System.InvalidOperationException("argument miss match");
            return exp;
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
                throw new System.InvalidOperationException("Invalid member type " + member.MemberType);
            }
            return exp;
        }

        public static BinaryExpression MakeBinary(ExpressionType binaryType, Expression left, Expression right)
        {
            return new BinaryExpression(left, right, binaryType);
        }

        public static LiteralExpression Literal(object value)
        {
            return new LiteralExpression(value);
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
