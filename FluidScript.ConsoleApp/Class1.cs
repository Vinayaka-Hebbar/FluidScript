using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        const string text = @"{global.log = func (message):void=>this.print(message);global.log(`working`);}";

        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        private void Test()
        {
            var compiler = new FluidScript.Compiler.DynamicCompiler();
            compiler["r"] = new FluidScript.Integer(2);
            compiler["s"] = new FluidScript.Double(2);
            var statement = FluidScript.ScriptParser.GetStatement(text);
            var result = compiler.Invoke(statement);
            Console.WriteLine(result);
        }

        public int Test(FluidScript.Integer value)
        {
            return value;
        }
    }


    public class Test : FluidScript.Runtime.DynamicObject
    {
        public Test()
        {
        }

        public FluidScript.Math Math { get; }

        [FluidScript.Runtime.Register("name")]
        public string Name { get; set; } = "Vinayaka";

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
        public int Add(int a, int b)
        {
            return a + b;
        }
    }

}
