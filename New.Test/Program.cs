using System;

namespace New.Test
{
    class Program
    {
        const string text = "vinayakahebbar*vinayakahebbar";
        static void Main(string[] args)
        {
            try
            {
                var compiler = new FluidScript.Compiler.RuntimeCompiler();
                compiler.Locals["vinayakahebbar"] = 1;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
                    var statement = FluidScript.ScriptParser.GetStatement(text);
                    _ = compiler.Invoke(statement);
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
