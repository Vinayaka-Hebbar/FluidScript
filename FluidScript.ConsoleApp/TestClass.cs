using System;

namespace FluidScipt.ConsoleTest
{
    public class TestClass
    {
        public static void Run()
        {
        }

        private int Test()
        {
            int a = 10;
            int b = 20;
            {
                int f = 10;
                a = 20 + f;
            }
            int c = a + b;
            int d = c * a;
            return d * b;
        }
    }

    public sealed class Closure
    {
        public object[] Values;

        public Closure(params object[] values)
        {
            Values = values;
        }

        public object Test(Closure closure)
        {
            return 2;
        }
    }
}
