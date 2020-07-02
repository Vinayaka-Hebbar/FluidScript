using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Extensions;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FluidScript.ConsoleApp
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

        public void Test()
        {
            try
            {
                var code = ScriptParser.ParseProgram("source.fls");
                var assembly = new AssemblyGen("FluidTest", "1.0");
                code.Compile(assembly);
                assembly.Save("FluidTest.dll");
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        public void Compile(object s)
        {
            dynamic value = 10;
            object x = value.Equals(2);
            object y = value.ToString();
            Console.WriteLine(x);
        }

        public int GetInt(int i) => ++i;
        Integer j = 1;

        static object FuncTest(object sender, Integer i)
        {
            return ++i;
        }

        public bool IsNull(object x)
        {
            return x is null;
        }

        public bool InstanceOf(object x)
        {
            return x is Integer;
        }

        public bool TestTry(out object res)
        {
            res = 10;
            return true;
        }

        public override string ToString()
        {
            object value = 10;
            TestTry(out value);
            return value.ToString();
        }

        public Action TestFun3()
        {
            return () => Console.WriteLine();
        }


    }
}
