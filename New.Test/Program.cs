using System;

namespace New.Test
{
    class Program
    {
        const string text = "Math.pow(3,3)";
        static void Main(string[] args)
        {
            try
            {
                var compiler = new FluidScript.Compiler.DynamicCompiler();
                compiler["r"] = new FluidScript.Integer(2);
                compiler["s"] = new FluidScript.Double(1.3426);
                var statement = FluidScript.ScriptParser.GetStatement(text);
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
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

        public class Class1
        {
            public Class2 Class2 { get; set; } = new Class2();
        }

        public class Class2
        {
            public Type Type { get; set; } = typeof(Class1);
        }
    }
}
