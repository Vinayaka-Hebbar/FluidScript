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
                    type = type.UnderlyingSystemType;
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

    }
}
