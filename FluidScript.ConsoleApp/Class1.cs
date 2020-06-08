using FluidScript.Compiler;
using FluidScript.Compiler.SyntaxTree;
using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        const string text = "Console.WriteLine";
        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        private void Test()
        {
            Console.WriteLine(default(DateTime));
            Console.WriteLine(default(DateTime));
            NodeList<TypeParameter> types = new NodeList<TypeParameter>();
            NodeList<Statement> statements = new NodeList<Statement>();
            BlockStatement body = new BlockStatement(statements);
            FunctionDeclaration declaration = new FunctionDeclaration("Test", types, TypeSyntax.Create(typeof(bool)), body);
            statements.Add(Statement.Return(Expression.False));
            var del = declaration.Compile();
            Console.WriteLine(del.DynamicInvoke());
        }

        public void Compile()
        {
            Console.WriteLine("OK");
        }

    }
}
