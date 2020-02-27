using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        const string text = "datetime(`dd-mm-yyyy`)";
        // todo import static class
        static void Main(string[] args)
        {
            Test();
            Console.ReadKey();
        }

        private static void Test()
        {
            try
            {
                var compiler = new FluidScript.Compiler.DynamicCompiler(new Test());
                compiler["r"] = FluidScript.Boolean.True;
                compiler["s"] = new FluidScript.Double(1.3426);
                var statement = FluidScript.ScriptParser.GetStatement(text);
                object result = compiler.Invoke(statement);
                Console.WriteLine(result);
            }
            catch (FluidScript.Compiler.ExecutionException ex)
            {
                Console.WriteLine($"{ex.Message},\ntrace:\nat {ex.StackTrace}");
            }
        }
    }


    public class Test : FluidScript.Runtime.DynamicObject
    {
        public Test()
        {
            Values = new JsonDictionary<string, object>
                {
                    {"name", "Vinayaka" }
                };
        }

        public FluidScript.Math Math { get; }

        public JsonDictionary<string, object> Values { get; }

        public string Name { get; } = "Vinayaka";

        [FluidScript.Runtime.Register("datetime")]
        public static FluidScript.String GetDataTime(FluidScript.String format)
        {
            return System.DateTime.Now.ToString(format.ToString());
        }

        [FluidScript.Runtime.Register("datetime")]
        public static FluidScript.String GetDataTime()
        {
            return System.DateTime.Now.ToString();
        }

        [FluidScript.Runtime.Register("add")]
        public int Add(params FluidScript.Integer[] values)
        {
            return System.Linq.Enumerable.Sum(values, value => value);
        }

        [FluidScript.Runtime.Register("add")]
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

}
