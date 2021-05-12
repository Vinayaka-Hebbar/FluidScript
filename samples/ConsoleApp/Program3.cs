using FluidScript.Compiler;
using FluidScript.Compiler.Emit;
using FluidScript.Runtime;
using System;

namespace ConsoleApp
{
    public class Program3
    {
        public interface ISample
        {
            void Start();
        }

        [Register("Console")]
        public class Console
        {
            [Register("print")]
            public static void Print(object value)
            {
                System.Console.WriteLine(value);
            }
        }

        static void Main(string[] args)
        {
            var code = ScriptParser.ParseProgram("script2.fls");
            var assembly = new AssemblyGen("FluidTest", "1.0");
            assembly.Context.Register("ISample", typeof(ISample));
            assembly.Context.Register("Console", typeof(Console));
            code.Compile(assembly);
#if NETFRAMEWORK
            assembly.Save("FluidTest.dll"); 
#endif
            if (assembly.Context.TryGetType("Sample", out Type type))
            {
                if (type is IType)
                {
                    type = type.UnderlyingSystemType;
                }
                var obj = (ISample)Activator.CreateInstance(type);
                obj.Start();
            }
            System.Console.ReadKey();
        }
    }
}
