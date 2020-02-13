using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        // todo import static class
        static void Main(string[] args)
        {
            var engine = new FluidScript.ScriptEngine();
            Test instance = new Test();
            var context = new FluidScript.Compiler.ExpressionVisitor(instance);
            var expression = engine.GetExpression("Value = 10+4");
            object result = context.Visit(expression);
            Console.WriteLine(result);
            Console.ReadKey();
        }

        public static Func<int> Test()
        {
            var a = 1;
            return () => a;
        }
    }

    public class Test
    {
        public int Value { get; set; }

        [FluidScript.Runtime.Register("datetime")]
        public static FluidScript.String GetDataTime()
        {
            return System.DateTime.Now.ToString();
        }

        [FluidScript.Runtime.Register("datetime")]
        public static FluidScript.String GetDataTime(FluidScript.String format)
        {
            return System.DateTime.Now.ToString(format.ToString());
        }
    }

}
