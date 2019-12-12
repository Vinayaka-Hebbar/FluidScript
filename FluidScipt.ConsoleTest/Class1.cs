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
            var methods = typeof(Integer).GetMethods();
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.GetStatement("a<b");
            var context = new FluidScript.Reflection.Emit.RuntimeContext(tree, new FluidScript.Math());
            context["a"] = new Integer(10);
            context["b"] = new Integer(20);
            var value = new Integer(10);
            Integer up = value;
            var re = context.Invoke();
            Console.WriteLine(re);
        }
    }

}
