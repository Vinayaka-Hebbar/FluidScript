using FluidScript;
using FluidScript.Library;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        public void Run()
        {
            ScriptEngine engine = new ScriptEngine();
            var tree = engine.GetStatement("2+6");
            var instance = new MathObject();
           var value =  tree.Evaluate(instance);
            Console.WriteLine();

        }

        void Print()
        {
            Console.WriteLine();
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
