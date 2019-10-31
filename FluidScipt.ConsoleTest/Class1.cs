using System;
using System.Reflection;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            Class1 class1 = new Class1();
            class1.Run();
            Console.ReadKey();
        }
        public void Run()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "source.rs";
            FluidScript.ScriptEngine engine = new FluidScript.ScriptEngine();
            var meth = engine.CreateTypeGenerator(new System.IO.FileInfo(path));
            var type = meth.Generate().Create("Sample");
            var instance = Activator.CreateInstance(type);
            Console.WriteLine();
        }

        public object Test()
        {
            var x = 1;
            return x;

        }

        public void Test2()
        {
            var x = 1;
            var y = 1.1;
            var a = 1;
            var z = (x + y) + a + 1;
        }



    }
}
