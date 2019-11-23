using FluidScript;
using FluidScript.Compiler.Metadata;
using Scripting.Runtime;
using System;
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
            var engine = new ScriptEngine();
            Prototype proto = typeof(FluidScript.Library.MathObject);
            RuntimeObject instance = proto.CreateInstance();
            instance.Append("name", "vinayaka", true);
            instance["density"] = 0.00786;
            instance["totalCoils"] = 0.0;
            var exp2 = engine.GetStatement("{var get=lamda()=>2;return get();}", proto);
            dynamic contactStress = exp2.Evaluate(instance);

            Console.WriteLine((object)contactStress);
        }

    }

    public class Class2
    {
        public void Add()
        {

        }
    }
}
