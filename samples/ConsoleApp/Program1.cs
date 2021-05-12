using FluidScript.Compiler;
using System;

namespace ConsoleApp
{
    public class Program1
    {
        static void Main(string[] args)
        {
            var compiler = new RuntimeCompiler();
            compiler["x"] = 10.0;
            compiler["y"] = 20;
            var statement = Parser.GetStatement("x+y+5.0");
            var res = compiler.Invoke(statement);
            Console.WriteLine("res = " + res);
            Console.ReadKey();
        }
    }
}
