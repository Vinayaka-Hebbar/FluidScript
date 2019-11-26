using FluidScript;
using FluidScript.Compiler.Metadata;
using System;
using System.Diagnostics;
using System.Linq;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Microsoft.CodeAnalysis.CSharp.Syntax.AnonymousFunctionExpressionSyntax
            Class1 class1 = new Class1();
            var x = 59.453261567682823 - 0.0431;
            class1.Run();
            Console.ReadKey();
        }

        public void Run()
        {
            var engine = new ScriptEngine();
            Prototype proto = new FunctionPrototype(typeof(FluidScript.Library.MathObject));
            RuntimeObject instance = proto.CreateInstance();
            instance.Append("name", "vinayaka", true);
            instance["density"] = 0.00786;
            instance.Append("totalCoils", 2.70, true);
            instance["print"] = RuntimeObject.CreateReference(Print);
            var exp2 = engine.GetStatement("totalCoils = 360");
            var contactStress = exp2.Evaluate(instance);
            Console.WriteLine(contactStress);
        }

        static void Print(RuntimeObject value)
        {
            Console.WriteLine(value);
        }
    }

    public class Class2
    {
        public void Add()
        {

        }
    }
}
