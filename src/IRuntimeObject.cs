using System;

namespace FluidScript
{
    public interface IRuntimeObject
    {
        bool IsNull { get; }
        bool IsArray();
        bool IsBool();
        bool IsChar();
        bool IsInbuilt();
        bool IsNumber();
        bool IsString();
        bool IsTypeOf<TSource>();
        object[] ToArray();
        bool ToBool();
        char ToChar();
        double ToDouble();
        float ToFloat();
        int ToInt32();
        double ToNumber();
        string ToString();
        Type ToType();
    }
}