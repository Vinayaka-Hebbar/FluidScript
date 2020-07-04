using FluidScript.Compiler.Binders;
using FluidScript.Compiler.SyntaxTree;
using FluidScript.Extensions;
using FluidScript.Runtime;
using FluidScript.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.Emit
{
    /// <summary>
    /// Method body IL Generator
    /// </summary>
    public class MethodBodyGenerator : ReflectionILGenerator, IExpressionVisitor<Expression>
    {
        internal sealed class BreakOrContinueInfo
        {
            public readonly string[] LabelNames;
            public readonly bool LabelledOnly;
            public readonly ILLabel BreakTarget;
            public readonly ILLabel ContinueTarget;

            public BreakOrContinueInfo(string[] labelNames, bool labelledOnly, ILLabel breakTarget, ILLabel continueTarget)
            {
                LabelNames = labelNames;
                LabelledOnly = labelledOnly;
                BreakTarget = breakTarget;
                ContinueTarget = continueTarget;
            }
        }

        /// <summary>
        /// Syntax Tree of method
        /// </summary>
        public Statement SyntaxTree { get; internal set; }

        internal readonly IMethodBase Method;

        internal readonly ITypeContext Context;

        public Type ReturnType { get; }

        internal IList<ILLocalVariable> LocalVariables;

        private readonly Stack<BreakOrContinueInfo> breakOrContinueStack = new Stack<BreakOrContinueInfo>();

        /// <summary>
        /// Initializes new instance of <see cref="MethodBodyGenerator"/>
        /// </summary>
        public MethodBodyGenerator(IMethodBase method, System.Reflection.Emit.ILGenerator generator, bool emitInfo = false) : base(generator, emitInfo)
        {
            Method = method;
            ReturnType = method.ReturnType;
            Context = method.Context;
            SyntaxTree = method.SyntaxBody;
        }


#if NETFRAMEWORK
        /// <summary>
        /// Debug Document writter
        /// </summary>
        public System.Diagnostics.SymbolStore.ISymbolDocumentWriter DebugDoument { get; set; }
#endif

        /// <summary>
        /// Source code
        /// </summary>
        public string Source
        {
            get;
            set;
        }

        /// <summary>
        /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="span"> The start and end positions which define the sequence point. </param>
        public void MarkSequencePoint(Debugging.TextSpan span)
        {
#if NETFRAMEWORK
            MarkSequencePoint(DebugDoument, span);
#endif
        }


        /// <summary>
        /// Gets or sets the name of the function that is being generated.
        /// </summary>
        public string FunctionName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates current IL is inside try catch or nor
        /// </summary>
        public bool InsideTryCatchOrFinally { get; set; }

        /// <summary>
        /// Pushes information about break or continue targets to a stack.
        /// </summary>
        /// <param name="labels"> The label names associated with the break or continue target.
        /// Can be <c>null</c>. </param>
        /// <param name="breakTarget"> The IL label to jump to if a break statement is encountered. </param>
        /// <param name="continueTarget"> The IL label to jump to if a continue statement is
        /// encountered.  Can be <c>null</c>. </param>
        /// <param name="labelledOnly"> <c>true</c> if break or continue statements without a label
        /// should ignore this entry; <c>false</c> otherwise. </param>
        public void PushBreakOrContinueInfo(string[] labels, Emit.ILLabel breakTarget, Emit.ILLabel continueTarget, bool labelledOnly)
        {
            if (breakTarget == null)
                throw new ArgumentNullException(nameof(breakTarget));
            if (labels != null && labels.Length > 0)
            {
                foreach (var label in labels)
                {
                    foreach (var info in breakOrContinueStack)
                    {
                        if (info.LabelNames != null && info.LabelNames.Any(ln => ln.Equals(label)))
                        {
                            throw new Exception(string.Format("label {0} already present", label));
                        }
                    }
                }
            }
            breakOrContinueStack.Push(new BreakOrContinueInfo(labels, labelledOnly, breakTarget, continueTarget));
        }

        /// <summary>
        /// Removes the top-most break or continue information from the stack.
        /// </summary>
        public void PopBreakOrContinueInfo()
        {
            this.breakOrContinueStack.Pop();
        }

        /// <summary>
        /// Returns the break target for the statement with the given label, if one is provided, or
        /// the top-most break target otherwise.
        /// </summary>
        /// <param name="labelName"> The label associated with the break target.  Can be
        /// <c>null</c>. </param>
        /// <returns> The break target for the statement with the given label. </returns>
        public ILLabel GetBreakTarget(string labelName = null)
        {
            if (labelName == null)
            {
                foreach (var info in breakOrContinueStack)
                {
                    if (info.LabelledOnly == false)
                        return info.BreakTarget;
                }
                throw new System.InvalidOperationException(string.Format("illgal break statement"));
            }
            else
            {
                foreach (var info in breakOrContinueStack)
                {
                    if (info.LabelNames != null && info.LabelNames.Any(ln => ln.Equals(labelName)))
                        return info.BreakTarget;
                }
                throw new KeyNotFoundException(string.Format("break label {0} not found", labelName));
            }
        }

        /// <summary>
        /// Returns the continue target for the statement with the given label, if one is provided, or
        /// the top-most continue target otherwise.
        /// </summary>
        /// <param name="labelName"> The label associated with the continue target.  Can be
        /// <c>null</c>. </param>
        /// <returns> The continue target for the statement with the given label. </returns>
        public ILLabel GetContinueTarget(string labelName = null)
        {
            if (labelName == null)
            {
                foreach (var info in breakOrContinueStack)
                {
                    if (info.ContinueTarget != null && info.LabelledOnly == false)
                        return info.ContinueTarget;
                }
                throw new System.InvalidOperationException(string.Format("illgal continue statement"));
            }
            else
            {
                foreach (var info in breakOrContinueStack)
                {
                    if (info.LabelNames != null && info.LabelNames.Any(ln => ln.Equals(labelName)))
                        return info.ContinueTarget;
                }
                throw new KeyNotFoundException(string.Format("continue label {0} not found", labelName));
            }
        }

        /// <summary>
        /// Gets the number of available break or continue targets.  Used to support break or
        /// continue statements within finally blocks.
        /// </summary>
        public int BreakOrContinueStackSize
        {
            get { return this.breakOrContinueStack.Count; }
        }

        /// <summary>
        /// Gets or sets a delegate that is called when EmitLongJump() is called and the target
        /// label is outside the LongJumpStackSizeThreshold.
        /// </summary>
        public Action<ILGenerator, ILLabel> LongJumpCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the depth of the break/continue stack at the start of the finally
        /// statement.
        /// </summary>
        public int LongJumpStackSizeThreshold
        {
            get;
            set;
        }

        /// <summary>
        /// Searches for the given label in the break/continue stack.
        /// </summary>
        /// <param name="label"></param>
        /// <returns> The depth of the label in the stack.  Zero indicates the bottom of the stack.
        /// <c>-1</c> is returned if the label was not found. </returns>
        private int GetBreakOrContinueLabelDepth(ILLabel label)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            int depth = breakOrContinueStack.Count - 1;
            foreach (var info in breakOrContinueStack)
            {
                if (info.BreakTarget == label)
                    return depth;
                if (info.ContinueTarget == label)
                    return depth;
                depth--;
            }
            return -1;
        }

        /// <summary>
        /// Long jump like retur,break,goto
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="label"></param>
        public void EmitLongJump(ILGenerator generator, ILLabel label)
        {
            if (LongJumpCallback == null)
            {
                //code generation not inside finally block
                if (InsideTryCatchOrFinally)
                    generator.Leave(label);
                else
                    generator.Branch(label);
            }
            else
            {
                //jump occuring inside finally
                int depth = GetBreakOrContinueLabelDepth(label);
                if (depth < LongJumpStackSizeThreshold)
                {
                    LongJumpCallback(generator, label);
                }
                else
                {
                    //target label inside finally
                    if (InsideTryCatchOrFinally)
                        generator.Leave(label);
                    else
                        generator.Branch(label);
                }
            }
        }

        #region Emit Convert
        public void EmitConvert(Conversion c)
        {
            var n = c;
            do
            {
                n = n.Next;
                if (n is BoxConversion)
                    Box(n.Type);
                else if (n is ParamConversion p)
                    CallStatic(p.Method);
            } while (n != c);
        }

        public void EmitConvert(ParamArrayConversion c, Expression[] expressions)
        {
            // Remaining size
            var size = expressions.Length;
            LoadInt32(size);
            NewArray(c.Type);
            if (size > 0)
            {
                var conversions = c.ParamBinders;
                for (int i = 0; i < size; i++)
                {
                    Duplicate();
                    LoadInt32(i);
                    Expression exp = expressions[i];
                    exp.GenerateCode(this);
                    if (conversions != null)
                    {
                        var group = conversions[i];
                        if (group != null)
                            EmitConvert(group);
                    }
                    StoreArrayElement(c.Type);
                }
            }
        }
        #endregion

        #region Return
        /// <summary>
        /// Gets or sets the label the return statement should jump to (with the return value on
        /// top of the stack).  Will be <c>null</c> if code is being generated outside a function
        /// context.
        /// </summary>
        public ILLabel ReturnTarget
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the variable that holds the return value for the function.  Will be
        /// <c>null</c> if code is being generated outside a function context or if no return
        /// statements have been encountered.
        /// </summary>
        public ILLocalVariable ReturnVariable
        {
            get;
            set;
        }



        #endregion

        ///<inheritdoc/>
        public ILLocalVariable DeclareVariable(Type type, string name)
        {
            ILLocalVariable variable = base.DeclareVariable(type, name);
            if (LocalVariables == null)
                LocalVariables = new List<ILLocalVariable>();
            LocalVariables.Add(variable);
            return variable;
        }

        /// <summary>
        /// Get Declared local variable
        /// </summary>
        /// Problem when scope
        public ILLocalVariable GetLocalVariable(string name)
        {
            if (LocalVariables != null)
            {
                return LocalVariables.FirstOrDefault(item => item.Name == name);
            }
            return null;
        }

        ///<summary>Creates IL Code</summary>
        public void Compile()
        {
            SyntaxTree.GenerateCode(this);
            if (ReturnTarget != null)
                DefineLabelPosition(ReturnTarget);
            if (ReturnVariable != null)
                LoadVariable(ReturnVariable);
            Complete();
        }

        #region Visitors

        Expression IExpressionVisitor<Expression>.Default(Expression node)
        {
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitUnary(UnaryExpression node)
        {
            var operand = node.Operand.Accept(this);
            if (node.NodeType == ExpressionType.Parenthesized)
            {
                node.Type = operand.Type;
                return operand;
            }
            ArgumentConversions conversions = new ArgumentConversions(2);
            var method = Utils.ReflectionUtils.GetOperatorOverload(node.MethodName, conversions, operand.Type);
            node.Conversions = conversions;
            node.Method = method;
            node.Type = method.ReturnType;
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitBinary(BinaryExpression node)
        {
            //todo like rutime compiler
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            ArgumentConversions conversions = new ArgumentConversions(2);
            System.Reflection.MethodInfo method;
            switch (node.NodeType)
            {
                case ExpressionType.Plus:
                    method = VisitAddition(left, right, conversions);
                    break;
                case ExpressionType.AndAnd:
                case ExpressionType.OrOr:
                    System.Reflection.MethodInfo convertLeft = ReflectionUtils.GetBooleanOveraload(left.Type);
                    if (convertLeft != null)
                        conversions.Add(new ParamConversion(0, convertLeft));
                    var convertRight = ReflectionUtils.GetBooleanOveraload(right.Type);
                    if (convertRight != null)
                        conversions.Add(new ParamConversion(1, convertRight));
                    method = node.NodeType == ExpressionType.AndAnd ? ReflectionHelpers.LogicalAnd : ReflectionHelpers.LogicalOr;
                    break;
                case ExpressionType.StarStar:
                    method = VisitPow(node, left, right, conversions);
                    break;
                default:
                    method = VisitBinary(left, right, node.MethodName, conversions);
                    break;
            }
            node.Conversions = conversions;
            node.Method = method ?? throw new OperationCanceledException(string.Concat("Invalid Operation ", node.ToString()));
            node.Type = method.ReturnType;
            return node;
        }

        static System.Reflection.MethodInfo VisitAddition(Expression left, Expression right, ArgumentConversions conversions)
        {
            if(left.Type.Name.Equals("String"))
            {
                if (right.Type.Name.Equals("String"))
                {
                    return VisitBinary(left, right, "op_Addition", conversions);
                }
                if (right.Type.IsValueType)
                    conversions.Add(new BoxConversion(1, right.Type));
                conversions.Add(new ParamConversion(1, ReflectionHelpers.AnyToString));
                right.Type = TypeProvider.StringType;
                return VisitBinary(left, right, "op_Addition", conversions);
            }
            if (right.Type.Name.Equals("String"))
            {
                if (left.Type.Name.Equals("String"))
                {
                    return VisitBinary(left, right, "op_Addition", conversions);
                }
                if (right.Type.IsValueType)
                    conversions.Add(new BoxConversion(1, left.Type));
                conversions.Add(new ParamConversion(0, ReflectionHelpers.AnyToString));
                left.Type = TypeProvider.StringType;
                return VisitBinary(left, right, "op_Addition", conversions);
            }
            return VisitBinary(left, right, "op_Addition", conversions);
        }

        static System.Reflection.MethodInfo VisitPow(BinaryExpression node, Expression left, Expression right, ArgumentConversions conversions)
        {
            var types = new Type[2] { left.Type, right.Type };
            if (left.Type.IsPrimitive == true || right.Type.IsPrimitive == true)
            {
                ReflectionUtils.FromSystemType(conversions, ref types);
            }
            var method = ReflectionHelpers.MathPow;
            if (!ReflectionUtils.MatchesTypes(method, types, conversions))
                ExecutionException.ThrowArgumentMisMatch(node);
            return method;
        }

        static System.Reflection.MethodInfo VisitBinary(Expression left, Expression right, string opName, ArgumentConversions conversions)
        {
            var types = new Type[2] { left.Type, right.Type };
            if (left.Type.IsPrimitive == true || right.Type.IsPrimitive == true)
            {
                ReflectionUtils.FromSystemType(conversions, ref types);
            }
            return ReflectionUtils.
               GetOperatorOverload(opName, conversions, types);
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitArrayLiteral(ArrayListExpression node)
        {
            var type = node.ArrayType != null ? node.ArrayType.ResolveType(Context) : typeof(object);
            node.Type = typeof(Collections.List<>).MakeGenericType(type);
            node.ElementType = type;
            if (node.Arguments != null)
            {
                var types = node.Arguments.Map(arg => arg.Accept(this).Type);
                if (node.Constructor == null)
                {
                    var ctors = node.Type.GetConstructors(Utils.ReflectionUtils.PublicInstance);
                    var ctor = Utils.ReflectionUtils.BindToMethod(ctors, types, out ArgumentConversions conversions);
                    if (ctor == null)
                        ExecutionException.ThrowMissingMethod(node.Type, "ctor", node);
                    node.Constructor = ctor;
                    node.ArgumentConversions = conversions;
                }
            }
            else if (node.Constructor == null)
            {
                node.Constructor = node.Type.GetConstructor(Utils.ReflectionUtils.PublicInstance, null, new Type[0], null);
            }
            var items = node.Expressions;
            if (items.Count > 0)
            {
                var arrayConversions = new ArgumentConversions(items.Count);
                for (int index = 0; index < items.Count; index++)
                {
                    Expression expression = items[index];
                    var value = expression.Accept(this);
                    if (!Runtime.TypeUtils.AreReferenceAssignable(type, expression.Type) && Runtime.TypeUtils.TryImplicitConvert(expression.Type, type, out System.Reflection.MethodInfo implicitCall))
                    {
                        arrayConversions.Append(index, new ParamConversion(index, implicitCall));
                    }
                }
                node.ArrayConversions = arrayConversions;
            }
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitAssignment(AssignmentExpression node)
        {
            node.Type = node.Right.Accept(this).Type;
            node.Left.Accept(this);
            return node;
        }

        bool HasMember(System.Reflection.MemberInfo m, object filter)
        {
            if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
            {
                var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).First();
                return data.Match(filter);
            }
            return filter.Equals(m.Name);
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            var member = target.Type.FindMembers(System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property, ReflectionUtils.Any, HasMember, node.Name).FirstOrDefault();
            IBinder binder = null;
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new Exception(string.Concat("Use of undeclared field ", field));
                    binder = new FieldBinder(field);
                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    binder = new PropertyBinder(property);
                }
            }
            node.Binder = binder;
            node.Type = binder.Type;
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitCall(InvocationExpression node)
        {
            if (node.Method is null)
            {
                var types = node.Arguments.Map(arg => arg.Accept(this).Type);
                Type resultType = null;
                string name = null;
                System.Reflection.MethodInfo method;
                ArgumentConversions conversions;
                // we can't resolve method directly using target.Accept()
                var target = node.Target;
                if (target.NodeType == ExpressionType.Identifier)
                {
                    resultType = Method.DeclaringType;
                    name = target.ToString();
                }
                else if (target.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression member = (MemberExpression)target;
                    var exp = member.Target.Accept(this);
                    resultType = exp.Type;
                    name = member.Name;
                }
                else if (target.NodeType == ExpressionType.Super || target.NodeType == ExpressionType.This)
                {
                    // this will be call to super();
                    var expression = target.Accept(this);
                    node.Method = ReflectionUtils.BindToMethod(expression.Type.GetConstructors(ReflectionUtils.PublicInstanceDeclared), types, out conversions);
                    node.Conversions = conversions;
                    return node;
                }
                else if (typeof(Delegate).IsAssignableFrom(target.Type))
                {
                    method = target.Type.GetInstanceMethod("Invoke");
                    conversions = new ArgumentConversions(types.Length);
                    if (!ReflectionUtils.MatchesTypes(method, types, conversions))
                        ExecutionException.ThrowArgumentMisMatch(node.Target, node);
                    goto done;
                }
                if (resultType.IsValueType && resultType == TypeProvider.AnyType)
                {
                    types = types.AddFirst(typeof(string));
                    node.Arguments.Insert(0, Expression.SystemLiteral(name));
                    conversions = new ArgumentConversions(types.Length);
                    method = resultType.FindSystemMethod(nameof(Any.Call), types, conversions);
                    goto done;
                }
                method = resultType.FindMethod(name, types, out conversions);
            done:
                node.Conversions = conversions;
                node.Method = method ?? throw ExecutionException.ThrowMissingMethod(resultType, name, node);
                node.Type = method.ReturnType;
            }
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitLiteral(LiteralExpression node)
        {
            Type type = null;
            switch (node.Value)
            {
                case int _:
                    type = typeof(Integer);
                    break;
                case float _:
                    type = typeof(Float);
                    break;
                case double _:
                    type = typeof(Double);
                    break;
                case char _:
                    type = typeof(Char);
                    break;
                case string _:
                    type = typeof(String);
                    break;
                case bool _:
                    type = typeof(Boolean);
                    break;
                case null:
                    type = typeof(IFSObject);
                    break;
            }
            node.Type = type;
            return node;
        }

        ///<inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitTernary(TernaryExpression node)
        {
            var conditionType = node.First.Accept(this).Type;
            if (conditionType == typeof(Boolean) || conditionType == typeof(bool))
            {
                var first = node.Second.Accept(this);
                var second = node.Third.Accept(this);
                if (first.Type == second.Type)
                {
                    node.Type = second.Type;
                }
                else if (TypeUtils.TryImplicitConvert(first.Type, second.Type, out System.Reflection.MethodInfo method))
                {
                    node.Type = method.ReturnType;
                    node.ImplicitCall = method;
                }
                else if (TypeUtils.TryImplicitConvert(second.Type, first.Type, out method))
                {
                    node.Type = method.ReturnType;
                    node.ImplicitCall = method;
                }
            }
            else
            {
                throw new Exception("expected bool type");
            }
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitInstanceOf(InstanceOfExpression node)
        {
            node.Target.Accept(this);
            if (node.TypeSyntax != null)
                node.TypeSyntax.ResolveType(Context);
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitNew(NewExpression node)
        {
            node.Type = node.TypeSyntax.ResolveType(Context);
            var types = node.Arguments.Map(a => a.Accept(this).Type);
            node.Constructor = ReflectionUtils.BindToMethod(node.Type.GetConstructors(), types, out ArgumentConversions conversions);
            node.Conversions = conversions;
            return node;

        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitMember(NameExpression node)
        {
            var name = node.Name;
            var variable = GetLocalVariable(name);
            if (variable != null)
            {
                if (variable.Type == null)
                    throw new Exception(string.Concat("Use of undeclared variable ", variable));
                node.Type = variable.Type;
                node.Binder = new VariableBinder(variable);
                return node;
            }
            var arg = Method.Parameters.FirstOrDefault(para => para.Name == name);
            if (arg.Name != null)
            {
                node.Type = arg.Type;
                node.Binder = new ParameterBinder(arg);
                return node;
            }
            //find in the class level
            var member = Method.DeclaringType.FindMembers(System.Reflection.MemberTypes.Property | System.Reflection.MemberTypes.Field, ReflectionUtils.Any, HasMember, name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new Exception(string.Concat("Use of undeclared field ", field));
                    node.Type = field.FieldType;
                    node.Binder = new FieldBinder(field);
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    node.Type = property.PropertyType;
                    node.Binder = new PropertyBinder(property);
                }
            }
            else if (Context.TryGetType(name, out Type type))
            {
                // if static type name
                node.Type = type;
                node.Binder = new EmptyBinder(type);
            }
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitThis(ThisExpression node)
        {
            node.Type = Method.DeclaringType;
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitSuper(SuperExpression node)
        {
            node.Type = Method.DeclaringType.BaseType;
            return node;
        }

        private bool FindExactMethod(System.Reflection.MemberInfo m, object filterCriteria)
        {
            return filterCriteria.Equals(m.Name);
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitIndex(IndexExpression node)
        {
            var target = node.Target.Accept(this);
            var type = target.Type;
            if (type.IsArray == false)
            {
                var types = node.Arguments.Map(arg => arg.Accept(this).Type);
                var indexers = type
                    .FindMembers(System.Reflection.MemberTypes.Method, Utils.ReflectionUtils.Any, this.FindExactMethod, "get_Item");
                var indexer = Utils.ReflectionUtils.BindToMethod(indexers, types, out ArgumentConversions bindings);
                //todo binding in array
                node.Getter = indexer ?? throw new Exception("Indexer not found");
                type = indexer.ReturnType;
            }
            else
            {
                type = type.GetElementType();
            }
            node.Type = type;
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitDeclaration(VariableDeclarationExpression node)
        {
            if (node.VariableType == null)
            {
                if (node.Value == null)
                    throw new InvalidOperationException("Invalid declaration syntax");
                node.Type = node.Value.Accept(this).Type;
            }
            else
            {
                node.Type = node.VariableType.ResolveType(Context);
            }
            return node;

        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitNull(NullExpression node)
        {
            return node;
        }

        /// <inheritdoc/>
        Expression IExpressionVisitor<Expression>.VisitNullPropegator(NullPropegatorExpression node)
        {
            var left = node.Left.Accept(this);
            node.Type = left.Type;
            return node;
        }

        Expression IExpressionVisitor<Expression>.VisitAnonymousObject(AnonymousObjectExpression node)
        {
            //todo implementation
            throw new NotImplementedException();
        }

        Expression IExpressionVisitor<Expression>.VisitAnonymousFunction(AnonymousFunctionExpression node)
        {
            if (node.Type is null)
            {
                Type returnType;
                if (node.ReturnSyntax != null)
                    returnType = node.ReturnSyntax.ResolveType(Context);
                else
                    returnType = typeof(object);
                int length = node.Parameters.Count;
                Type[] types = new Type[length];
                var parameters = new ParameterInfo[length];
                for (int i = 0; i < length; i++)
                {
                    var para = node.Parameters[i];
                    Type paramerterType = para.Type == null ? TypeProvider.ObjectType : para.Type.ResolveType((ITypeContext)Context);
                    parameters[i] = new ParameterInfo(para.Name, i + 1, paramerterType);
                    types[i] = paramerterType;
                }
                var delgateType = DelegateGen.MakeNewDelegate(types, returnType);
                node.Type = delgateType;
                node.Types = types;
                node.ReturnType = returnType;
                node.ParameterInfos = parameters;
            }
            return node;
        }

        Expression IExpressionVisitor<Expression>.VisitSizeOf(SizeOfExpression node)
        {
            throw new NotImplementedException();
        }

        Expression IExpressionVisitor<Expression>.VisitConvert(ConvertExpression node)
        {
            var value = node.Target.Accept(this);
            if (node.Type == null)
            {
                var type = node.TypeName.ResolveType(Context);

                // todo explict convert
                if (!TypeUtils.AreReferenceAssignable(type, value.Type) &&
                    TypeUtils.TryImplicitConvert(value.Type, type, out System.Reflection.MethodInfo implicitConvert))
                {
                    node.Method = implicitConvert;
                }
                node.Type = type;
            }
            return node;
        }
        #endregion
    }
}
