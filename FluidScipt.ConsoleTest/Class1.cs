using FluidScript;
using System;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        private double x = 1.1;
        private int y = 1;
        private int a = 1;
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
            var meth = engine.CreateMethodGenerator();
            meth.DefineField("a", 1);
            meth.DefineField("b", 1);
            meth.DefineMethod("a", new PrimitiveType[0], (args) => { return 1; });
            var valie=  meth.GetExpression("{a()+b;}");
           var res =  valie.Evaluate();
            Console.WriteLine(res);
        }

        public void Test()
        {
            Test2();
            var type = System.Type.GetType("System.Int32[]");
            var memebers = type.GetMembers();
            Console.WriteLine();
        }

        public double Test2()
        {
            short m = 1;
            var a = m + 1.0;
            var value = '\x0001';
            var res = PrimitiveType.Int32 & PrimitiveType.UInt32;
            var n = 1;
            var x = n == 1 ? 1 : a;
            return x;
        }

        public double get(int x)
        {
            return x;
        }
        
    }
}
