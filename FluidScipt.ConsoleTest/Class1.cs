using FluidScript;
using System;
using System.Reflection;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax

            Class1 class1 = new Class1();
            class1.Run();
            //class1.Print();
            Console.ReadKey();
        }

        private void Run()
        {
            ScriptEngine engine = new ScriptEngine();
            var context = new FluidScript.Dynamic.RuntimeContext(new FluidScript.Math());
            context["a"] = new FluidScript.Double(10);
            context["b"] = new Integer(20);
            FluidScript.Compiler.SyntaxTree.Statement tree = engine.GetStatement("{a=[10,20];a[2]=`a value`;return a[2];}");
            var re = context.Invoke(tree);
            Console.WriteLine(re);
        }
    }

}
