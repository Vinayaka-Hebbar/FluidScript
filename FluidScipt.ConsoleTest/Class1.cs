using FluidScript;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FluidScipt.ConsoleTest
{
    public class Class1
    {
        static void Main(string[] args)
        {
            //Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax
            Class1 class1 = new Class1();
            class1.Run();
            //class1.Print();
            Console.ReadKey();
        }

        private void Print()
        {
            var value1 = FluidScript.Boolean.True;
            var value2 = FluidScript.Boolean.True;
            System.Linq.Expressions.Expression<Func<bool>> test = () => value1 && value2;
            var body = test.Body;

            Console.WriteLine(FluidScript.Boolean.True && FluidScript.Boolean.True);
        }

        private void Run()
        {

            var context = new FluidScript.Dynamic.DynamicContext(new FluidScript.Math());
            context["a"] = new Integer(4);
            FluidScript.Compiler.SyntaxTree.Statement tree = ScriptEngine.GetStatement("{a=}", FluidScript.Compiler.ParserSettings.Default);
            var re = context.Invoke(tree);
            Console.WriteLine(re);
        }

        void EmitRun()
        {
            var type = typeof(Dictionary<string, double>);
            Console.WriteLine(type.IsSerializable);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.ParseFile(path + "source.fls");
            if (tree is FluidScript.Compiler.SyntaxTree.TypeDeclaration declration)
            {
                System.Reflection.AssemblyName aName = new System.Reflection.AssemblyName("Runtime.DynamicClasses, Version=1.0.0.1");
                var dynamicAssembly =
               System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(aName, System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
                var dynamicModule =
                   dynamicAssembly.DefineDynamicModule(aName.Name, string.Concat(aName.Name, ".dll"), false);
                var classType = declration.Generate(new FluidScript.Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                dynamic instance = Activator.CreateInstance(classType);
                dynamicAssembly.Save("Runtime.DynamicClasses.dll");
                object result = instance.Add();
                Console.WriteLine(result);
            }
        }
    }

    [System.Serializable]
    public sealed class DictionaryItems : Dictionary<string, double>
    {
        public DictionaryItems()
        {
        }

        public DictionaryItems(IDictionary<string, double> items) : base(items)
        {

        }

        public DictionaryItems(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
