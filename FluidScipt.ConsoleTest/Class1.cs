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
            var tree = engine.GetStatement("sqrt(a)");
            var context = new FluidScript.Dynamic.RuntimeContext(tree, new FluidScript.Math());
            context["a"] = new FluidScript.Double(10);
            context["b"] = new Integer(20);
            var re = context.Invoke();
            Console.WriteLine(re);
        }
    }

}
