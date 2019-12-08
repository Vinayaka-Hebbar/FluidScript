using FluidScript;
using System;
using System.Linq.Expressions;
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
            class1.Print();
            Console.ReadKey();
        }

        private void Run()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.ParseFile(path + "source.fls");
            if (tree is FluidScript.Compiler.SyntaxTree.TypeDeclaration declration)
            {
                var dynamicAssembly =
               AppDomain.CurrentDomain.DefineDynamicAssembly(new System.Reflection.AssemblyName("DynamicClass.Utility.DynamicClasses, Version=1.0.0.0"), System.Reflection.Emit.AssemblyBuilderAccess.RunAndSave);
                var dynamicModule =
                    dynamicAssembly.DefineDynamicModule("DynamicClass.Utility.DynamicClasses.dll", true);
                var classType = declration.Generate(new FluidScript.Reflection.Emit.ReflectionModule(dynamicAssembly, dynamicModule));
                dynamic instance = Activator.CreateInstance(classType);
                var c = instance.Read(1);
                Console.WriteLine(c);
                dynamicAssembly.Save("DynamicClass.Utility.DynamicClasses.dll");
            }
        }

        void Print()
        {
            int[] array = new int[0];
           var proper =  array.GetType().GetProperties();
            var obj = new { a = 1 };
            System.Linq.Expressions.Expression<Func<int>> func = () => obj.a.GetHashCode();
            var body = func.Body;
            if (body is MethodCallExpression method)
            {
                var type = method.Object.GetType();
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        class ExV :
            System.Linq.Expressions.ExpressionVisitor
        {
            protected override Expression VisitUnary(UnaryExpression node)
            {
                return base.VisitUnary(node);
            }
        }

        public FluidScript.Double Add(int a, int b)
        {
            return a + b;
        }

        public override string ToString()
        {
            return base.ToString();
        }


        public object Get(object value) => value;
    }

    [DataContract]
    public struct Age
    {
        [DataMember]
        public int Number { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Age && ((Age)obj).Number == Number;
        }

        public override int GetHashCode()
        {
            return Number.GetHashCode();
        }

        public static bool operator ==(Age left, Age right)
        {
            return left.Number == right.Number;
        }

        public static bool operator !=(Age left, Age right)
        {
            return left.Number != right.Number;
        }
    }

}
