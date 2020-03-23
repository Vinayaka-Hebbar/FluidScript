using System;

namespace Old.Test
{
    class Program
    {
        const string text = "2+2.0";
        static void Main(string[] args)
        {
            try
            {
                var compiler = new FluidScript.Compiler.DynamicCompiler(new FluidScript.Math());
                compiler["r"] = new FluidScript.Integer(2);
                compiler["s"] = new FluidScript.Double(1.3426);
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
                    var statement = FluidScript.ScriptParser.GetStatement(text);
                    var result = compiler.Invoke(statement);
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
            catch (FluidScript.Compiler.ExecutionException ex)
            {
                Console.WriteLine($"{ex.Message},\ntrace:\nat {ex.StackTrace}");
            }
            Console.ReadKey();
        }
    }
}
