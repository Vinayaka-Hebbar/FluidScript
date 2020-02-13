using System;

namespace Old.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new FluidScript.ScriptEngine();
            var context = new FluidScript.Dynamic.DynamicContext(new FluidScript.Math());
            context["r"] = new FluidScript.Double(715.77);
            context["r"] = new FluidScript.Double(71.77);
            context["s"] = new FluidScript.Double(1.3426);
            var statement = engine.GetStatement("{var value={a:1};value.a=29;return value.count;}");
            object result = context.Invoke(statement);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
