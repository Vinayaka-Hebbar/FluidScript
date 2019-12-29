using FluidScript.Compiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Reflection.Emit
{
    /// <summary>
    /// Method body IL Generator
    /// </summary>
    public sealed class MethodBodyGenerator : ReflectionILGenerator, Compiler.IExpressionVisitor<Expression>
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

        internal readonly IMethodBaseGenerator Method;

        internal readonly TypeGenerator TypeGenerator;

        private IList<ILLocalVariable> LocalVariables;

        private readonly Stack<BreakOrContinueInfo> breakOrContinueStack = new Stack<BreakOrContinueInfo>();

        /// <summary>
        /// Initializes new instance of <see cref="MethodBodyGenerator"/>
        /// </summary>
        public MethodBodyGenerator(IMethodBaseGenerator method, System.Reflection.Emit.ILGenerator generator, bool emitInfo = false) : base(generator, emitInfo)
        {
            Method = method;
            TypeGenerator = method.TypeGenerator;
            SyntaxTree = method.SyntaxTree;
        }

#if NET40
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
        public void MarkSequencePoint(Compiler.TextSpan span)
        {
#if NET40
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
        public void PushBreakOrContinueInfo(string[] labels, Reflection.Emit.ILLabel breakTarget, Reflection.Emit.ILLabel continueTarget, bool labelledOnly)
        {
            if (breakTarget == null)
                throw new System.ArgumentNullException(nameof(breakTarget));
            if (labels != null && labels.Length > 0)
            {
                foreach (var label in labels)
                {
                    foreach (var info in breakOrContinueStack)
                    {
                        if (info.LabelNames != null && info.LabelNames.Any(ln => ln.Equals(label)))
                        {
                            throw new System.Exception(string.Format("label {0} already present", label));
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
        public override ILLocalVariable DeclareVariable(Type type, string name = null)
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
        public ILLocalVariable GetLocalVariable(string name)
        {
            if (LocalVariables != null)
            {
                return LocalVariables.FirstOrDefault(item => item.Name == name);
            }
            return null;
        }

        ///<summary>Creates IL Code</summary>
        public void Build()
        {
#if NET40
            if (DebugDoument != null)
                MarkSequencePoint(DebugDoument, new Compiler.TextSpan(1, 1, 1, 1));
#endif
            SyntaxTree.GenerateCode(this);
            if (ReturnTarget != null)
                DefineLabelPosition(ReturnTarget);
            if (ReturnVariable != null)
                LoadVariable(ReturnVariable);
            Complete();
        }

        #region Visitors

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitUnary(UnaryExpression node)
        {
            var operand = node.Operand.Accept(this);
            string name = null;
            switch (node.NodeType)
            {
                case ExpressionType.Parenthesized:
                    return operand;
                case ExpressionType.PostfixPlusPlus:
                    name = "op_Increment";
                    break;
                case ExpressionType.PrefixPlusPlus:
                    name = "op_Increment";
                    break;
                case ExpressionType.PostfixMinusMinus:
                    name = "op_Decrement";
                    break;
                case ExpressionType.PrefixMinusMinus:
                    name = "op_Decrement";
                    break;
                case ExpressionType.Bang:
                    name = "op_LogicalNot";
                    break;
                case ExpressionType.Plus:
                    name = "op_UnaryPlus";
                    break;
                case ExpressionType.Minus:
                    name = "op_UnaryNegation";
                    break;
                case ExpressionType.Circumflex:
                    name = "op_ExclusiveOr";
                    break;
                case ExpressionType.Or:
                    name = "op_BitwiseOr";
                    break;
                case ExpressionType.And:
                    name = "op_BitwiseAnd";
                    break;
            }
            var method = TypeUtils.GetOperatorOverload(name, out Conversion[] conversions, operand.Type);
            node.Conversions = conversions;
            node.Method = method;
            node.Type = method.ReturnType;
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitBinary(BinaryExpression node)
        {
            string opName = null;
            var nodeType = node.NodeType;
            switch (node.NodeType)
            {
                case ExpressionType.Plus:
                    opName = "op_Addition";
                    break;
                case ExpressionType.Minus:
                    opName = "op_Subtraction";
                    break;
                case ExpressionType.Multiply:
                    opName = "op_Multiply";
                    break;
                case ExpressionType.Divide:
                    opName = "op_Division";
                    break;
                case ExpressionType.Percent:
                    opName = "op_Modulus";
                    break;
                case ExpressionType.BangEqual:
                    opName = "op_Inequality";
                    break;
                case ExpressionType.EqualEqual:
                    opName = "op_Equality";
                    break;
                case ExpressionType.Greater:
                    opName = "op_GreaterThan";
                    break;
                case ExpressionType.GreaterEqual:
                    opName = "op_GreaterThanOrEqual";
                    break;
                case ExpressionType.Less:
                    opName = "op_LessThan";
                    break;
                case ExpressionType.LessEqual:
                    opName = "op_LessThanOrEqual";
                    break;
                case ExpressionType.And:
                    opName = "op_BitwiseAnd";
                    break;
                case ExpressionType.Or:
                    opName = "op_BitwiseOr";
                    break;
                case ExpressionType.Circumflex:
                    opName = "op_ExclusiveOr";
                    break;
            }
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);
            System.Reflection.MethodInfo method = null;
            Conversion[] conversions = null;
            if (opName != null)
            {
                method = TypeUtils.
                   GetOperatorOverload(opName, out conversions, left.Type, right.Type);
                
                node.Conversions = conversions;
            }
            else if (nodeType == ExpressionType.AndAnd || nodeType == ExpressionType.OrOr)
            {
                conversions = new Conversion[2];
                System.Reflection.MethodInfo convertLeft = TypeUtils.GetBooleanOveraload(left.Type);
                if (convertLeft != null)
                    conversions[0] = new Conversion(convertLeft);
                var convertRight = TypeUtils.GetBooleanOveraload(right.Type);
                if (convertRight != null)
                    conversions[1] = new Conversion(convertRight);
                method = nodeType == ExpressionType.AndAnd ? Helpers.LogicalAnd : Helpers.LogicalOr;
            }
            node.Conversions = conversions;
            node.Method = method ?? throw new OperationCanceledException(string.Concat("Invalid Operation ", node.ToString()));
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitArrayLiteral(ArrayLiteralExpression node)
        {
            if (node.Size != null)
                node.Size.Accept(this);
            node.Type = node.ArrayType != null ? node.ArrayType.GetType(TypeGenerator).MakeArrayType() : typeof(IFSObject).MakeArrayType();
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitAssignment(AssignmentExpression node)
        {
            node.Type = node.Right.Accept(this).Type;
            node.Left.Accept(this);
            return node;
        }

        bool HasMember(System.Reflection.MemberInfo m, object filter)
        {
            if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
            {
                var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                if (data != null)
                    return data.Match(filter);
            }
            return filter.Equals(m.Name);
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitMember(MemberExpression node)
        {
            var target = node.Target.Accept(this);
            var member = target.Type.FindMembers(System.Reflection.MemberTypes.Field | System.Reflection.MemberTypes.Property, TypeUtils.Any, HasMember, node.Name).FirstOrDefault();
            Binding binding = null;
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new Exception(string.Concat("Use of undeclared field ", field));
                    binding = new FieldBinding(field);
                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    binding = new PropertyBinding(property);
                }
            }
            node.Binding = binding;
            node.Type = binding.Type;
            return node;
        }

        /// <summary>
        /// Get Arguments types
        /// </summary>
        private Type[] GetTypes(Expression[] arguments)
        {
            return arguments.Select(arg => arg.Accept(this).Type).ToArray();
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitCall(InvocationExpression node)
        {
            var target = node.Target.Accept(this);
            var types = GetTypes(node.Arguments);
            Type resultType = null;
            string name = null;
            if (target.NodeType == ExpressionType.Identifier)
            {
                resultType = TypeGenerator;
                name = target.ToString();
            }
            else if (target.NodeType == ExpressionType.MemberAccess)
            {
                var exp = (MemberExpression)target;
                resultType = exp.Type;
                name = exp.Name;
            }
            bool HasMethod(System.Reflection.MethodInfo m)
            {
                if (m.IsDefined(typeof(Runtime.RegisterAttribute), false))
                {
                    var data = (Attribute)m.GetCustomAttributes(typeof(Runtime.RegisterAttribute), false).FirstOrDefault();
                    if (data != null)
                        return data.Match(name);
                }
                return m.Name == name;
            }
            var methods = resultType
                .GetMethods(TypeUtils.Any)
                .Where(HasMethod).ToArray();
            //todo ignore case
            if (methods.Length == 0)
                throw new Exception(string.Concat("method ", name, " not found"));
            //todo type conversion
            var method = (System.Reflection.MethodInfo)Type.DefaultBinder.SelectMethod(TypeUtils.Any, methods, types, null);
            if (method is IMethodBaseGenerator baseGenerator)
                method = (System.Reflection.MethodInfo)baseGenerator.MethodBase;

            node.Method = method;
            node.Type = method.ReturnType;
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitLiteral(LiteralExpression node)
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
        Expression Compiler.IExpressionVisitor<Expression>.VisitTernary(TernaryExpression node)
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
        Expression Compiler.IExpressionVisitor<Expression>.VisitMember(NameExpression node)
        {
            var name = node.Name;
            var variable = GetLocalVariable(name);
            if (variable != null)
            {
                if (variable.Type == null)
                    throw new Exception(string.Concat("Use of undeclared variable ", variable));
                node.Type = variable.Type;
                node.Binding = new VariableBinding(variable);
                return node;
            }
            ParameterInfo arg = Method.Parameters.FirstOrDefault(para => para.Name == name);
            if (arg.Name != null)
            {
                node.Type = arg.Type;
                node.Binding = new ArgumentBinding(arg);
                return node;
            }
            //find in the class level
            var member = TypeGenerator.FindMember(name).FirstOrDefault();
            if (member != null)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var field = (System.Reflection.FieldInfo)member;
                    if (field.FieldType == null)
                        throw new System.Exception(string.Concat("Use of undeclared field ", field));
                    node.Type = field.FieldType;
                    node.Binding = new FieldBinding(field);
                }
                if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    var property = (System.Reflection.PropertyInfo)member;
                    node.Type = property.PropertyType;
                    node.Binding = new PropertyBinding(property);
                }
            }
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitThis(ThisExpression node)
        {
            node.Type = TypeGenerator;
            return node;
        }

        private bool FindExactMethod(System.Reflection.MemberInfo m, object filterCriteria)
        {
            return filterCriteria.Equals(m.Name);
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitIndex(IndexExpression node)
        {
            var target = node.Target.Accept(this);
            var type = target.Type;
            if (type.IsArray == false)
            {
                var types = GetTypes(node.Arguments);
                var indexers = type
                    .FindMembers(System.Reflection.MemberTypes.Method, TypeUtils.Any, FindExactMethod, "get_Item");
                var indexer = TypeUtils.BindToMethod(indexers, types, out Conversion[] convers);
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
        Expression Compiler.IExpressionVisitor<Expression>.VisitDeclaration(VariableDeclarationExpression node)
        {
            if (node.VariableType == null)
            {
                if (node.Value == null)
                    throw new InvalidOperationException("Invalid declaration syntax");
                node.Type = node.Value.Accept(this).Type;
            }
            else
            {
                node.Type = node.VariableType.GetType(TypeGenerator);
            }
            return node;

        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitNull(NullExpression node)
        {
            return node;
        }

        /// <inheritdoc/>
        Expression Compiler.IExpressionVisitor<Expression>.VisitNullPropegator(NullPropegatorExpression node)
        {
            var left = node.Left.Accept(this);
            node.Type = left.Type;
            return node;
        }
        #endregion
    }
}
