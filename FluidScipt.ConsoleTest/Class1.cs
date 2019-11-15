using FluidScript;
using FluidScript.Compiler.Metadata;
using FluidScript.Core;
using Scripting.Runtime;
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
            Class1 class1 = new Class1();
            class1.Run();
            Console.ReadKey();
        }

        public void Run()
        {
            var a = new int[] { 2, 1 }[1];
            ScriptEngine engine = new FluidScript.ScriptEngine();
            FunctionPrototype prototype = new FunctionPrototype();
            prototype.DefineVariable("a", 4);
            prototype.DefineVariable("b", 2);
            RuntimeObject instance = prototype.CreateInstance();
            var class2 = new Class2();
            instance["add"] = RuntimeObject.CreateReference(class2.Add);
            instance["x"] = 1;
            FluidScript.Library.MathObject mathObject = new FluidScript.Library.MathObject();
            var statement = engine.GetStatement("add()", prototype);
            var obj = statement.Evaluate(instance);
            Console.WriteLine(obj);
        }

        public void Add(RuntimeObject arg)
        {
            Console.WriteLine("Working", 1);
        }

    }

    public class Class2
    {
        public void Add()
        {

        }
    }
}
