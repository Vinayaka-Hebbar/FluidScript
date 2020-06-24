using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
using System;
using System.Reflection;

namespace FluidScript.ConsoleTest
{
    public class Class1
    {
        const string text = "Console.WriteLine";
        // todo import static class
        static void Main(string[] args)
        {
            new Class1().Test();
            Console.ReadKey();
        }

        private void Test()
        {
            try
            {
                var val = FuncTest(this, 0);
                var code = (TypeDeclaration)ScriptParser.ParseFile(AppDomain.CurrentDomain.BaseDirectory + "source.fls");
                AssemblyGen assembly = new AssemblyGen("FluidTest", "1.0");
                assembly.Context.Register("Console", typeof(Console));
                var type = code.Generate(assembly);
                var value = Activator.CreateInstance(type);
                
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        public void Compile()
        {
            Console.WriteLine("OK");
        }

        public int GetInt(int i) => ++i;
        Integer j = 1;

        static object FuncTest(object sender, Integer i)
        {
            return ++i;
        }

        public Action TestFun3()
        {
            return () => Console.WriteLine();
        }


    }
}
