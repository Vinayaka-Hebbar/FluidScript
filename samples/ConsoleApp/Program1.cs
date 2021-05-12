using FluidScript.Compiler;
using System;

namespace ConsoleApp
{
    public class Program1
    {
        static void Main(string[] args)
        {
            var compiler = new RuntimeCompiler();
            compiler["x"] = 10;
            compiler["y"] = 20;
            var statement = Parser.GetStatement("x+y");
            var res = compiler.Invoke(statement);
            Console.WriteLine("res = " + res);
            Console.ReadKey();
        }
    }
}
