using System.Collections.Generic;
using System.Linq;

namespace FluidScript.SyntaxTree.Statements
{
    public class FunctionDefinitionStatement : Statement, IExpression, IFunctionExpression
    {
        public readonly string Name;
        public Expression.Operation Kind => Expression.Operation.Function;

        public IExpression[] Arguments { get; }
        public readonly Scope Scope;

        public Statement Body { get; }

        public FunctionDefinitionStatement(FunctionDeclarationStatement declaration, BlockStatement body, Scope scope) : base(Operation.Function)
        {
            Name = declaration.Name;
            Arguments = declaration.Arguments;
            Body = body;
            Scope = scope;
        }

        public FunctionDefinitionStatement(string name, IExpression[] arguments, BlockStatement body) : base(Operation.Function)
        {
            Name = name;
            Arguments = arguments;
            Body = body;
        }

        public override bool Equals(object obj)
        {
            return Name.Equals(obj);
        }

        public override string ToString()
        {
            return Name;
        }

        public FunctionPartBuilder GetPartBuilder()
        {
            return new FunctionPartBuilder(Arguments.Length, Invoke, Scope);
        }

        private Object Invoke(NodeVisitor visitor, IExpression[] args)
        {
            visitor = new NodeVisitor(visitor);
            return visitor.VisitFunction(this, args.Select(arg => arg.Accept(visitor)).ToArray());
        }

        public override TReturn Accept<TReturn>(INodeVisitor<TReturn> visitor)
        {
            return visitor.VisitFunctionDefinition(this);
        }

        public override int GetHashCode()
        {
            var hashCode = 1062545247;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IExpression[]>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<Statement>.Default.GetHashCode(Body);
            hashCode = hashCode * -1521134295 + Kind.GetHashCode();
            return hashCode;
        }
    }
}
