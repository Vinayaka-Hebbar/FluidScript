using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        const string text = @"Math.pow(2.2,2)";

        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        private void Test()
        {
            var compiler = new FluidScript.Compiler.RuntimeCompiler();
            compiler.Locals["pi"] = new FluidScript.Double(3.14);
            compiler.Locals["uLu"] = new FluidScript.Double(3.14);
            compiler.Locals["uppsetForgingId"] = new FluidScript.Double(47);
            compiler.Locals["dc"] = new FluidScript.Double(12);
            compiler.Locals["d"] = new FluidScript.Double(11);
            compiler.Locals["t"] = new FluidScript.Double(20);
            compiler.Locals["lnt"] = new FluidScript.Double(21);
            var statement = FluidScript.ScriptParser.GetStatement(text);
            var result = compiler.Invoke(statement);
            Console.WriteLine(result);

        }

        public void Test(FluidScript.Integer value)
        {
            FluidScript.Math.Pow(2, 2);
        }
    }

}
