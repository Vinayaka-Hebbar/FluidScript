using FluidScript.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript.Compiler.SyntaxTree
{
    public sealed class BlockStatement : Statement
    {
        //Todo Linq
        public readonly Statement[] Statements;

        public BlockStatement(Statement[] statements, string[] labels) : base(labels, StatementType.Block)
        {
            Statements = statements;
        }

        public override IEnumerable<Node> ChildNodes() => Statements;

        public override void GenerateCode(MethodBodyGenerator generator)
        {
            var statementLocals = new StatementLocals() { NonDefaultSourceSpanBehavior = true };
            GenerateStartOfStatement(generator, statementLocals);
            foreach (var statement in Statements)
            {
                statement.GenerateCode(generator);
            }
            GenerateEndOfStatement(generator, statementLocals);
        }

#if Runtime
        public override RuntimeObject Evaluate(RuntimeObject instance)
        {
            var proto = new Metadata.FunctionPrototype(instance.GetPrototype(), "Block", Metadata.ScopeContext.Block);
            instance = new Library.LocalInstance(proto, instance);
            return Evaluate(instance, proto);
        }

        internal override RuntimeObject Evaluate(RuntimeObject instance, Metadata.Prototype prototype)
        {
            foreach (var statement in Statements)
            {
                StatementType nodeType = statement.NodeType;
                var value = statement.Evaluate(instance);
                switch (nodeType)
                {
                    case StatementType.Return:
                        return value;
                    case StatementType.Declaration:
                    case StatementType.Expression:
                        break;
                    default:
                        if (value is object)
                            return value;
                        break;
                }
            }
            return null;
        }
#endif

        public override string ToString()
        {
            return string.Concat("{", string.Join(";", Statements.Select(statement => statement.ToString())), "}");
        }
    }
}
