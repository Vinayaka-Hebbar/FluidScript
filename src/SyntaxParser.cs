using FluidScript.SyntaxTree;
using FluidScript.SyntaxTree.Statements;
using System.Collections.Generic;
using System.Linq;

namespace FluidScript
{
    public class SyntaxParser
    {
        public readonly IOperationContext Context;

        public SyntaxParser()
        {
            Context = new OperationContextWrapper();
        }

        public SyntaxParser(IEqualityComparer<string> comparer)
        {
            Context = new OperationContextWrapper(comparer);
        }

        public SyntaxParser(IOperationContext context)
        {
            Context = context;
        }

        public void AddConstants(IEnumerable<KeyValuePair<string, double>> values)
        {
            Context.Concat(values);
        }

        public void AddVariables(IEnumerable<KeyValuePair<string, double>> values)
        {
            foreach (var item in values)
            {
                Context[item.Key] = new Object(item.Value);
            }
        }

        public void AddVariables(IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var item in values)
            {
                Context[item.Key] = new Object(item.Value);
            }
        }

        public void SetVariable(string name, Object value)
        {
            Context[name] = value;
        }

        public void AddVariables(double[] values)
        {
            int index = 1;
            foreach (var value in values)
            {
                Context[$"arg{index++}"] = new Object(value);
            }
        }

        public void AddVariable(string name, Object value)
        {
            Context.Variables.Add(name, value);
        }

        public void AddFunction(IFunction function)
        {
            Context.Functions.Add(function.Name, function);
        }

        public ParsedExpression ParseExpression(string text)
        {
            var iterator = new SyntaxVisitor(text, Context);
            if (iterator.MoveNext())
            {
                var expression = iterator.VisitExpression();
                return new ParsedExpression(Context, expression);
            }
            return new ParsedExpression(Context, Expression.Empty);
        }

        public ParsedStatement ParseStatement(string text)
        {
            var iterator = new SyntaxVisitor(text, Context);
            if (iterator.MoveNext())
                return new ParsedStatement(Context, iterator.VisitStatement(Scope.Local));
            return new ParsedStatement(Context, Statement.Empty);
        }

        public ParsedStatement ParseBlock(string text)
        {
            var iterator = new SyntaxVisitor(text, Context);
            if (iterator.MoveNext())
                return new ParsedStatement(Context, new BlockStatement(iterator.VisitListStatement().ToArray()));
            return new ParsedStatement(Context, Statement.Empty);
        }

        public ParsedProgram ParseProgram(string text)
        {
            var iterator = new SyntaxVisitor(text, Context);
            if (iterator.MoveNext())
                return new ParsedProgram(Context, iterator.VisitProgram());
            return new ParsedProgram(Context, Enumerable.Empty<Statement>());
        }
    }
}
