using FluidScript.Collections;
using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Compiler.SyntaxTree;
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
                //Integer x = 0;
                var code = ScriptParser.ParseProgram("source.fls");
                var assembly = new AssemblyGen("FluidTest", "1.0");
                code.Compile(assembly);
                // assembly.Save("FluidTest.dll");
                var type = assembly.Context.GetType("Sample");
                if(type is IType)
                {
                    type = type.ReflectedType;
                }
                Any instance = Activator.CreateInstance(type);
               var res = (String)instance.Call("add");
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }
        static Any val;
        public void Compile()
        {
            val.Call("Equals",new Integer(0));
            return;
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

        struct TestClass
        {
            internal int x;
        }
    }
}
