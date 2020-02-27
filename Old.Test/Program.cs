using System;

namespace Old.Test
{
    class Program
    {
        const string text = "sum(<int>[1,2,3,4,5,6])";
        static void Main(string[] args)
        {
            var engine = new FluidScript.ScriptEngine();
            try
            {
                var context = new FluidScript.Dynamic.DynamicContext(new FluidScript.Math());
                context["r"] = new FluidScript.Double(71.77);
                context["s"] = new FluidScript.Double(1.3426);
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                for (int i = 0; i < 100000; i++)
                {
                    var statement = engine.GetStatement(text);
                    object result = context.Invoke(statement);
                }
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"{ex.Message},\ntrace:\nat {ex.StackTrace}");
            }
            Console.ReadKey();
        }
    }
}
