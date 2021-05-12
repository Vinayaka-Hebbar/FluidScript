using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Runtime;
using System;
using System.Reflection;

namespace FluidScript.ConsoleApp
{
    public class Program
    {
        Program()
        {
        }

        // todo import static class
        static void Main(string[] args)
        {
            new Program().Test();
            Console.ReadKey();
        }

        public void Test()
        {
            try
            {
                RunCodeGen();
                Console.WriteLine();
            }
            catch (TargetInvocationException ex)
            {
                throw ex;
            }
        }

        static void RunCodeGen()
        {
            var code = ScriptParser.ParseProgram("source.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            code.Compile(assembly);
            assembly.Save("FluidTest.dll");
            if (assembly.Context.TryGetType("Sample", out Type type))
            {
                if (type is IType)
                {
                    type = type.ReflectedType;
                }
                object obj = Activator.CreateInstance(type);
                Any value = new Any(obj);
                var res = (value.Invoke("test"));
                Console.WriteLine(res);
            }
        }

        private static void CodeGen()
        {
            //Integer x = 0;
            var code = ScriptParser.ParseProgram("source.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            code.Compile(assembly);
            assembly.Save("FluidTest.dll");
        }

        DynamicObject x = new DynamicObject()
        {
            ["a"] = 1,
            ["b"] = 2,
        };

        protected bool IsNull(object x)
        {
            return x is null;
        }

        public void InstanceOf()
        {
            var x = this.x;
            x["a"] = 10;
        }

        public override string ToString()
        {
            object value = 10;
            return value.ToString();
        }

        int i;
        public Action TestFun3()
        {
            i = Console.Read();
            void x() => Console.WriteLine(i);
            i = 10;
            return x;
        }
    }
}
