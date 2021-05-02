using FluidScript;
using System;

namespace FluidScript.ConsoleTest
{

    public class Sample2 
    {
        public Sample2()
        {

        }
    }




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
        Closure Current => this;

        public Closure(params object[] values)
        {
            Values = values;
        }

        public object Test(Integer i)
        {
            return -i;
        }

        public object Test2()
        {
            return i << 1;
        }

        Integer i;
        public object Value
        {
            get
            {
                var x = 0;
                for (i = 0; i < 10; i++)
                {
                    x = i;
                }
                return x;
            }
        }

        public void Read()
        {
            Print(new Integer(10));
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public string BaseName
        {
            get
            {
                return ToString();
            }
        }

        public static void Print(object value)
        {
            Console.Write(new String("Hello"));
        }
    }
}
