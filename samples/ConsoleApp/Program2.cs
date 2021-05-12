using FluidScript;
using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using System;

namespace ConsoleApp
{
    public class Program2
    {
        static void Main(string[] args)
        {
            var code = ScriptParser.ParseProgram("script.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            code.Compile(assembly);
#if NETFRAMEWORK
            assembly.Save("FluidTest.dll"); 
#endif
            if (assembly.Context.TryGetType("Sample", out Type type))
            {
                if (type is IType)
                {
                    type = type.ReflectedType;
                }
                object obj = Activator.CreateInstance(type);
                Any value = new Any(obj);
                var res = value.Invoke("test");
                Console.WriteLine(res);
            }
            Console.ReadKey();
        }
    }
}
