﻿using FluidScript;
using FluidScript.Compiler.Metadata;
using System;

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

            var path = System.AppDomain.CurrentDomain.BaseDirectory + "source.rs";
            ScriptEngine engine = new FluidScript.ScriptEngine();
            var scope = new ObjectPrototype();
            scope.DefineConstant("pi", 3.14);
            var prototype = new FunctionPrototype(scope);
            prototype.DefineVariable("a", 4);
            prototype.DefineVariable("b", 2);
            prototype.DefineVariable("c", 1);
            scope.DefineMethod("pow", new PrimitiveType[2] { PrimitiveType.Double, PrimitiveType.Double }, (args) =>
              {
                  return Math.Pow(args[0].ToDouble(), args[1].ToDouble());
              });
            var valie = engine.GetStatement("1", prototype);
            RuntimeObject output = valie.Evaluate();
            Console.WriteLine(output);
        }

    }
}
