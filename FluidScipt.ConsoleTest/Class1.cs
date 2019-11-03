using FluidScript;
using System;
using System.Reflection;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        private double x = 1.1;
        private int y = 1;

        static void Main(string[] args)
        {
            Class1 class1 = new Class1();
            class1.Test();
            class1.Run();
            Console.ReadKey();
        }
        public void Run()
        {
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "source.rs";
            ScriptEngine engine = new FluidScript.ScriptEngine();
            var meth = engine.CreateTypeGenerator(new System.IO.FileInfo(path));
            var type = meth.Generate().Create("Sample");
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("read");
            var resilt = method.Invoke(instance, new object[] { 1 });
            Console.WriteLine(resilt);
        }

        public void Test()
        {
            var type = System.Type.GetType("System.Int32[]");
            var memebers = type.GetMembers();
            Console.WriteLine();
        }

        public int Test2()
        {
            var get = new int[] { 1 ,2};
            return 0;
        }

        public double get(int x)
        {
            return x;
        }



    }
}
