using FluidScript;

namespace FluidScipt.ConsoleTest
{
    public class TestClass : FSObject
    {
        public Integer _x = 12;
        public static readonly String _name = "vinayaka";
        public TestClass Current { get => this; }
        public Integer X
        {
            get
            {
                return Current._x;
            }
        }

        public Integer Read(Integer arg1)
        {
            return X;
        }
    }
}
