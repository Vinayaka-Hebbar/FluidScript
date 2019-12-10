﻿namespace FluidScript.Reflection.Emit
{
    /// <summary>
    /// Emit Parameter
    /// </summary>
    public struct ParameterInfo
    {
        public readonly string Name;
        public readonly int Index;
        public readonly System.Type Type;
        public readonly bool IsVar;

        public ParameterInfo(string name, int index, System.Type type, bool isVar)
        {
            Name = name;
            Index = index;
            Type = type;
            IsVar = isVar;
        }
    }
}