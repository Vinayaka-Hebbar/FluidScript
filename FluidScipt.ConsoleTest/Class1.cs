using FluidScript;
using FluidScript.Compiler.Metadata;
using System;
using System.Collections.Generic;
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
            object x = (int)1;
            var a = (uint)1;
            var eq = a.Equals(x);
            var dict = new Dictionary<uint, int>();
            dict.Add(1, 1);
            dict.Add(2, 1);
            dict.Add(3, 1);
            var engine = new ScriptEngine();
            Prototype proto = new FunctionPrototype(typeof(FluidScript.Library.MathObject));
            RuntimeObject instance = proto.CreateInstance();
            instance.Append("name", "vinayaka", true);
            instance["rowCount"] = 10;
            RuntimeObject runtimeObject = RuntimeObject.From(dict);
           var res =  runtimeObject.Call("hasKey", 1);
            instance["names"] = runtimeObject;
            instance.Append("totalCoils", 2.70, true);
            instance["print"] = RuntimeObject.CreateReference(Print);
            
            var exp2 = engine.GetStatement("rowCount++");
            var contactStress = exp2.Evaluate(instance);
            Console.WriteLine(contactStress);
            var exp = engine.GetStatement("rowCount++");
            var contactStress1 = exp.Evaluate(instance);
            Console.WriteLine(contactStress1);
            var exp3 = engine.GetStatement("rowCount++");
            var contactStress2 = exp3.Evaluate(instance);
            Console.WriteLine(contactStress2);
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
