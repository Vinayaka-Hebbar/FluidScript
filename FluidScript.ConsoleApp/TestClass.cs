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

    [System.Serializable]
    public struct User
    {
        [NonSerialized]
        private string Value;

        public User(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
