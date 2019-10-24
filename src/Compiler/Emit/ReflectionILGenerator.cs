using System;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;

namespace FluidScript.Compiler.Emit
{
    internal class ReflectionILGenerator : ILGenerator
    {
        public readonly System.Reflection.Emit.ILGenerator Generator;
        public readonly bool EmitDebugInfo;

        public ReflectionILGenerator(System.Reflection.Emit.ILGenerator generator, bool emitDebugInfo)
        {
            Generator = generator;
            EmitDebugInfo = emitDebugInfo;
        }

        public override void Add()
        {
            Generator.Emit(OpCodes.Add);
        }

        public override void BegineCatchBlock(Type exceptionType)
        {
            Generator.BeginCatchBlock(exceptionType);
        }

        public override void BegineExceptionBlock()
        {
            Generator.BeginExceptionBlock();
        }

        public override void BeginFaultBlock()
        {
            Generator.BeginFaultBlock();
        }

        public override void BeginFilterBlock()
        {
            Generator.BeginExceptFilterBlock();
        }

        public override void BeginFinallyBlock()
        {
            Generator.BeginFinallyBlock();
        }

        public override void BitwiseAnd()
        {
            Generator.Emit(OpCodes.And);
        }

        public override void BitwiseNot()
        {
            Generator.Emit(OpCodes.Not);
        }

        public override void BitwiseOr()
        {
            Generator.Emit(OpCodes.Or);
        }

        public override void BitwiseXor()
        {
            Generator.Emit(OpCodes.Xor);
        }

        public override void Box(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Box, type);
        }

        public override void Branch(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Br, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfEqual(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Beq, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfGreaterThan(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Bgt, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfGreaterThanOrEqual(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Bge, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfGreaterThanOrEqualUnsigned(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Bge_Un, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfGreaterThanUnsigned(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Bgt_Un, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfLessThan(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Blt, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfLessThanOrEqual(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Ble, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfLessThanOrEqualUnsigned(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Ble_Un, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfLessThanUnsigned(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Blt_Un, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfNotEqual(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Bne_Un, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfNotZero(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Brtrue, reflectionLabel.UnderlyingLabel);
        }

        public override void BranchIfZero(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Brfalse, reflectionLabel.UnderlyingLabel);
        }

        public override void Breakpoint()
        {
            Generator.Emit(OpCodes.Break);
        }

        public override void CallStatic(MethodBase method)
        {
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Call, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Call, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of MethodBase");
        }

        public override void CallVirtual(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (method.IsStatic)
                throw new ArgumentNullException(nameof(method));
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Callvirt, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Callvirt, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of MethodBase");
        }

        public override void CastClass(Type type)
        {
            Generator.Emit(OpCodes.Castclass, type);
        }

        public override void CompareEqual()
        {
            Generator.Emit(OpCodes.Ceq);
        }

        public override void CompareGreaterThan()
        {
            Generator.Emit(OpCodes.Cgt);
        }

        public override void CompareGreaterThanUnsigned()
        {
            Generator.Emit(OpCodes.Cgt_Un);
        }

        public override void CompareLessThan()
        {
            Generator.Emit(OpCodes.Clt);
        }

        public override void CompareLessThanUnsigned()
        {
            Generator.Emit(OpCodes.Clt_Un);
        }

        public override void Complete()
        {
            Return();
        }

        public override void ConvertToByte()
        {
            Generator.Emit(OpCodes.Conv_I1);
        }

        public override void ConvertToChar()
        {
            Generator.Emit(OpCodes.Conv_U2);
        }

        public override void ConvertToDouble()
        {
            Generator.Emit(OpCodes.Conv_R8);
        }

        public override void ConvertToInt16()
        {
            Generator.Emit(OpCodes.Conv_I2);
        }

        public override void ConvertToInt32()
        {
            Generator.Emit(OpCodes.Conv_I4);
        }

        public override void ConvertToInt64()
        {
            Generator.Emit(OpCodes.Conv_I8);
        }

        public override void ConvertToSingle()
        {
            Generator.Emit(OpCodes.Conv_R4);
        }

        public override void ConvertToUnsignedByte()
        {
            Generator.Emit(OpCodes.Conv_U1);
        }

        public override void ConvertToUnsignedInt16()
        {
            Generator.Emit(OpCodes.Conv_U2);
        }

        public override void ConvertToUnsignedInt32()
        {
            Generator.Emit(OpCodes.Conv_U4);
        }

        public override void ConvertToUnsignedInt64()
        {
            Generator.Emit(OpCodes.Conv_U8);
        }

        public override ILLabel CreateLabel()
        {
            return new ReflectionILLabel(Generator.DefineLabel());
        }

        public override ILLocalVariable DeclareVariable(Type type, string name = null)
        {
            var localBuilder = Generator.DeclareLocal(type);
            if (EmitDebugInfo && name != null)
                localBuilder.SetLocalSymInfo(name);
            return new ReflectionILLocalVariable(localBuilder, name);
        }

        public override void DefineLabelPosition(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.MarkLabel(reflectionLabel.UnderlyingLabel);
        }

        public override void Divide()
        {
            Generator.Emit(OpCodes.Div);
        }

        public override void Duplicate()
        {
            Generator.Emit(OpCodes.Dup);
        }

        public override void EndExceptionBlock()
        {
            Generator.EndExceptionBlock();
        }

        public override void EndFilter()
        {
            Generator.Emit(OpCodes.Endfilter);
        }

        public override void EndFinally()
        {
            Generator.Emit(OpCodes.Endfinally);
        }

        public override void InitObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Initobj, type);
        }

        public override void IsInstance(Type type)
        {
            Generator.Emit(OpCodes.Isinst, type);
        }

        public override void Leave(ILLabel label)
        {
            if (!(label is ReflectionILLabel reflectionLabel))
                throw new ArgumentNullException(nameof(label));
            Generator.Emit(OpCodes.Leave, reflectionLabel.UnderlyingLabel);
        }

        public override void LoadAddressOfVariable(ILLocalVariable variable)
        {
            if (!(variable is ReflectionILLocalVariable reflectionVariable))
                throw new ArgumentNullException(nameof(variable));
            Generator.Emit(OpCodes.Ldloca, reflectionVariable.UnderlyingLocal);
        }

        public override void LoadArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(argumentIndex));
            switch (argumentIndex)
            {
                case 0:
                    Generator.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    Generator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    Generator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    Generator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (argumentIndex < 256)
                        Generator.Emit(OpCodes.Ldarg_S, (byte)argumentIndex);
                    else
                        Generator.Emit(OpCodes.Ldarg, (short)argumentIndex);
                    break;
            }
        }

        public override void LoadArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    Generator.Emit(OpCodes.Ldelem_I1);
                    break;
                case TypeCode.SByte:
                    Generator.Emit(OpCodes.Ldelem_U1);
                    break;
                case TypeCode.Int16:
                    Generator.Emit(OpCodes.Ldelem_I2);
                    break;
                case TypeCode.Char:
                case TypeCode.UInt16:
                    Generator.Emit(OpCodes.Ldelem_U2);
                    break;
                case TypeCode.Int32:
                    Generator.Emit(OpCodes.Ldelem_I4);
                    break;
                case TypeCode.UInt32:
                    Generator.Emit(OpCodes.Ldelem_U4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    Generator.Emit(OpCodes.Ldelem_I8);
                    break;
                case TypeCode.Single:
                    Generator.Emit(OpCodes.Ldelem_R4);
                    break;
                case TypeCode.Double:
                    Generator.Emit(OpCodes.Ldelem_R8);
                    break;
                default:
                    if (type.IsClass)
                        Generator.Emit(OpCodes.Ldelem_Ref);
                    else
                        Generator.Emit(OpCodes.Ldelem, type);
                    break;

            }
        }

        public override void LoadArrayLength()
        {
            Generator.Emit(OpCodes.Ldlen);
        }

        public override void LoadBool(bool value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value ? 1 : 0);
        }

        public override void LoadByte(byte value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        public override void LoadByte(sbyte value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        public override void LoadChar(char value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        public override void LoadDouble(double value)
        {
            Generator.Emit(OpCodes.Ldc_R8, value);
        }

        public override void LoadField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.IsStatic)
                Generator.Emit(OpCodes.Ldsfld, field);
            else
                Generator.Emit(OpCodes.Ldfld, field);
        }

        public override void LoadInt16(short value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        public override void LoadInt16(ushort value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        public override void LoadInt32(int value)
        {
            Generator.Emit(OpCodes.Ldc_I4, value);
        }

        //private void LoadBit(sbyte value)
        //{
        //    switch (value)
        //    {
        //        case -1:
        //            Generator.Emit(OpCodes.Ldc_I4_M1);
        //            break;
        //        case 0:
        //            Generator.Emit(OpCodes.Ldc_I4_0);
        //            break;
        //        case 1:
        //            Generator.Emit(OpCodes.Ldc_I4_1);
        //            break;
        //        case 2:
        //            Generator.Emit(OpCodes.Ldc_I4_2);
        //            break;
        //        case 3:
        //            Generator.Emit(OpCodes.Ldc_I4_3);
        //            break;
        //        case 4:
        //            Generator.Emit(OpCodes.Ldc_I4_4);
        //            break;
        //        case 5:
        //            Generator.Emit(OpCodes.Ldc_I4_5);
        //            break;
        //        case 6:
        //            Generator.Emit(OpCodes.Ldc_I4_6);
        //            break;
        //        case 7:
        //            Generator.Emit(OpCodes.Ldc_I4_7);
        //            break;
        //        case 8:
        //            Generator.Emit(OpCodes.Ldc_I4_8);
        //            break;
        //    }
        //}

        public override void LoadInt32(uint value)
        {
            Generator.Emit(OpCodes.Ldc_I4_S, value);
        }

        public override void LoadInt64(long value)
        {
            Generator.Emit(OpCodes.Ldc_I8, value);
        }

        public override void LoadInt64(ulong value)
        {
            Generator.Emit(OpCodes.Ldc_I8, value);
        }

        public override void LoadNull()
        {
            Generator.Emit(OpCodes.Ldnull);
        }

        public override void LoadSingle(float value)
        {
            Generator.Emit(OpCodes.Ldc_R4, value);
        }

        public override void LoadStaticMethodPointer(MethodBase method)
        {
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldftn, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldftn, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        public override void LoadString(string value)
        {
            Generator.Emit(OpCodes.Ldstr, value);
        }

        public override void LoadToken(Type type)
        {
            Generator.Emit(OpCodes.Ldtoken, type);
        }

        public override void LoadToken(MethodBase method)
        {
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldtoken, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldtoken, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        public override void LoadToken(FieldInfo field)
        {
            Generator.Emit(OpCodes.Ldtoken, field);
        }

        public override void LoadVariable(ILLocalVariable variable)
        {
            if (!(variable is ReflectionILLocalVariable reflectionVariable))
                throw new ArgumentNullException(nameof(variable));
            Generator.Emit(OpCodes.Ldloc, reflectionVariable.UnderlyingLocal);
        }

        public override void LoadVirtualMethodPointer(MethodBase method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (method.IsStatic)
                throw new ArgumentException(nameof(method));
            if (method is ConstructorInfo)
                Generator.Emit(OpCodes.Ldvirtftn, (ConstructorInfo)method);
            else if (method is MethodInfo)
                Generator.Emit(OpCodes.Ldvirtftn, (MethodInfo)method);
            else
                throw new InvalidOperationException("unsupported subtype of Methodbase");
        }

        public override void MakeSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            Generator.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        public override void Multiply()
        {
            Generator.Emit(OpCodes.Mul);
        }

        public override void Negate()
        {
            Generator.Emit(OpCodes.Neg);
        }

        public override void NewArray(Type type)
        {
            Generator.Emit(OpCodes.Newarr, type);
        }

        public override void NewObject(ConstructorInfo constructor)
        {
            Generator.Emit(OpCodes.Newobj, constructor);
        }

        public override void NoOperation()
        {
            Generator.Emit(OpCodes.Nop);
        }

        public override void Pop()
        {
            Generator.Emit(OpCodes.Pop);
        }

        public override void Remainder()
        {
            Generator.Emit(OpCodes.Rem);
        }

        public override void ReThrow()
        {
            Generator.Emit(OpCodes.Rethrow);
        }

        public override void Return()
        {
            Generator.Emit(OpCodes.Ret);
        }

        public override void ShiftLeft()
        {
            Generator.Emit(OpCodes.Shl);
        }

        public override void ShiftRight()
        {
            Generator.Emit(OpCodes.Shr);
        }

        public override void ShiftRightUnsigned()
        {
            Generator.Emit(OpCodes.Shr_Un);
        }

        public override void StoreArgument(int argumentIndex)
        {
            if (argumentIndex < 0)
                throw new ArgumentNullException(nameof(argumentIndex));
            if (argumentIndex < 256)
                Generator.Emit(OpCodes.Starg_S, (byte)argumentIndex);
            else
                Generator.Emit(OpCodes.Starg, (short)argumentIndex);
        }

        public override void StoreArrayElement(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                    Generator.Emit(OpCodes.Stelem_I1);
                    break;
                case TypeCode.UInt16:
                case TypeCode.Int16:
                    Generator.Emit(OpCodes.Stelem_I2);
                    break;
                case TypeCode.Char:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    Generator.Emit(OpCodes.Stelem_I4);
                    break;
                case TypeCode.UInt64:
                case TypeCode.Int64:
                    Generator.Emit(OpCodes.Stelem_I8);
                    break;
                case TypeCode.Single:
                    Generator.Emit(OpCodes.Stelem_R4);
                    break;
                case TypeCode.Double:
                    Generator.Emit(OpCodes.Stelem_R8);
                    break;
                default:
                    if (type.IsClass)
                        Generator.Emit(OpCodes.Stelem_Ref);
                    else
                        Generator.Emit(OpCodes.Stelem, type);
                    break;

            }
        }

        public override void StoreField(FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.IsStatic)
                Generator.Emit(OpCodes.Stsfld, field);
            else
                Generator.Emit(OpCodes.Stfld, field);
        }

        public override void StoreVariable(ILLocalVariable variable)
        {
            if (!(variable is ReflectionILLocalVariable reflectionVariable))
                throw new ArgumentNullException(nameof(variable));
            Generator.Emit(OpCodes.Stloc, reflectionVariable.UnderlyingLocal);
        }

        public override void Subtract()
        {
            Generator.Emit(OpCodes.Sub);
        }

        public override void Switch(ILLabel[] labels)
        {
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));
            var reflectionLabels = new Label[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                reflectionLabels[i] = ((ReflectionILLabel)labels[i]).UnderlyingLabel;
            }
            Generator.Emit(OpCodes.Switch, reflectionLabels);
        }

        public override void Throw()
        {
            Generator.Emit(OpCodes.Throw);
        }

        public override void Unbox(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Unbox, type);
        }

        public override void UnboxObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            Generator.Emit(OpCodes.Unbox_Any, type);
        }
    }
}
