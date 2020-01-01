namespace FluidScript.Reflection.Emit
{
    public class AttributeGenerator
    {
        public readonly System.Type Type;
        public readonly System.Reflection.ConstructorInfo Ctor;
        public readonly object[] Parameters;
        public readonly System.Reflection.PropertyInfo[] Properties;
        public readonly object[] PropertiesData;

        public AttributeGenerator(System.Type type, System.Reflection.ConstructorInfo ctor, object[] parameters, System.Reflection.PropertyInfo[] properties, object[] propertiesData)
        {
            Type = type;
            Ctor = ctor;
            Parameters = parameters;
            Properties = properties;
            PropertiesData = propertiesData;
        }

        private object instance;
        public object Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = System.Activator.CreateInstance(Type, Parameters);
                    //todo property
                }
                return instance;
            }
        }
    }
}
