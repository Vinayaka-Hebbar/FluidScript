using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        const string text = @"func (item:int)=>item";

        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        private void Test()
        {
            var compiler = new FluidScript.Compiler.DynamicCompiler();
            compiler.Locals["r"] = 1;
            using (compiler.Locals.EnterScope())
            {
                compiler.Locals["r"] = 10;
                compiler.Locals["s"] = 10;
                var statement = FluidScript.ScriptParser.GetExpression(text);
                var result = compiler.Invoke(statement);
                result = compiler.Invoke(statement);
                Console.WriteLine(result);
            }
            Console.WriteLine();
        }

        public int Test(FluidScript.Integer value)
        {
            return value;
        }
    }

}
