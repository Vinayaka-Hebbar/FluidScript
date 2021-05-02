using System;

namespace FluidScript.Compiler.Generators
{
    public class AttributeGenerator
    {
        public readonly Type Type;
        public readonly System.Reflection.ConstructorInfo Ctor;
        public readonly object[] Parameters;
        public readonly System.Reflection.PropertyInfo[] Properties;
        public readonly object[] PropertiesData;

        public AttributeGenerator(Type type, System.Reflection.ConstructorInfo ctor, object[] parameters, System.Reflection.PropertyInfo[] properties, object[] propertiesData)
        {
            Type = type;
            Ctor = ctor;
            Parameters = parameters;
            Properties = properties;
            PropertiesData = propertiesData;
        }

        private Attribute instance;
        public Attribute Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (Attribute)Activator.CreateInstance(Type, Parameters);
                    //todo property
                }
                return instance;
            }
        }
    }
}
