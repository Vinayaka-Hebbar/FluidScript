namespace FluidScript.Compiler.Emit
{
    public static class EmitConvertion
    {
        public static void ToAny(ILGenerator generator, System.Type type)
        {
            if (type.IsValueType)
                generator.Box(type);
        }

        internal static void Convert(MethodBodyGenerator generator, System.Type from, System.Type to)
        {
            var method = to.GetMethod("op_Implicit", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                generator.Call(method);
            method = from.GetMethod("op_Implicit", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, new System.Type[1] { from }, null);
            if (method != null && method.ReturnType == to)
                generator.Call(method);
        }
    }
}
