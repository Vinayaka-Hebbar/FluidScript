using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public class FunctionDefinitionStatement : Statement, IFunctionExpression
    {
        public readonly string Name;

        public Node[] Arguments { get; }
        public readonly CodeScope Scope;

        public Statement Body { get; }

        public FunctionDefinitionStatement(FunctionDeclarationStatement declaration, BlockStatement body) : base(NodeType.Function)
        {
            Name = declaration.Name;
            Arguments = declaration.Arguments;
            Body = body;
            Scope = scope;
        }

        public FunctionDefinitionStatement(string name, Node[] arguments, BlockStatement body) : base(NodeType.Function)
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

        private Object Invoke(NodeVisitor visitor, Node[] args)
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
            hashCode = hashCode * -1521134295 + EqualityComparer<Node[]>.Default.GetHashCode(Arguments);
            hashCode = hashCode * -1521134295 + EqualityComparer<Statement>.Default.GetHashCode(Body);
            hashCode = hashCode * -1521134295 + NodeType.GetHashCode();
            return hashCode;
        }
    }
}
