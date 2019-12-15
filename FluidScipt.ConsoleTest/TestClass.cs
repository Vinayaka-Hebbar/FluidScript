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
            set
            {
                Current._x = value;
            }
        }

        public Integer Add()
        {
            Integer x = 0;
            for (var i=0; i < 10; i++)
            {
                x++;
            }
            return x;
        }
    }
}
