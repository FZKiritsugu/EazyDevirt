﻿using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;
using AsmResolver.PE.DotNet.Cil;
using EazyDevirt.Abstractions;
using EazyDevirt.Core.IO;
using EazyDevirt.Devirtualization;

namespace EazyDevirt.PatternMatching.Patterns.OpCodes;

#region Ldloc
internal record PushLocalToStackPattern : IPattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,         // 0	0000	ldarg.0
        CilOpCodes.Ldarg_0,         // 1	0001	ldarg.0
        CilOpCodes.Ldfld,           // 2	0002	ldfld	class VMOperandType[] VM::Locals
        CilOpCodes.Ldarg_1,         // 3	0007	ldarg.1
        CilOpCodes.Ldelem,          // 4	0008	ldelem	VMOperandType
        CilOpCodes.Callvirt,        // 5	000D	callvirt	instance class VMOperandType VMOperandType::vmethod_3()
        CilOpCodes.Call,            // 6	0012	call	instance void VM::PushStack(class VMOperandType)
        CilOpCodes.Ret              // 7	0017	ret
    };

    public bool Verify(MethodDefinition method, int index = 0) =>
        method.CilMethodBody!.Instructions[2].Operand as SerializedFieldDefinition ==
        DevirtualizationContext.Instance.VMLocalsField;
}

internal record Ldloc : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldarg_1,     // 1	0001	ldarg.1
        CilOpCodes.Castclass,   // 2	0002	castclass	VMUShortOperand
        CilOpCodes.Callvirt,    // 3	0007	callvirt	instance uint16 VMUShortOperand::method_3()
        CilOpCodes.Callvirt,    // 4	000C	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 5	0011	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[3].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.UInt16") return false;
        
        var pushLocalToStackMethod = instructions[4].Operand as SerializedMethodDefinition;
        return PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), pushLocalToStackMethod!);
    }
}

internal record Ldloc_S : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldarg_1,     // 1	0001	ldarg.1
        CilOpCodes.Castclass,   // 2	0002	castclass	VMByteOperand
        CilOpCodes.Callvirt,    // 3	0007	callvirt	instance uint8 VMByteOperand::method_3()
        CilOpCodes.Callvirt,    // 4	000C	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 5	0011	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc_S;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[3].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.Byte") return false;
        
        var pushLocalToStackMethod = instructions[4].Operand as SerializedMethodDefinition;
        return PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), pushLocalToStackMethod!);
    }
}
#endregion Ldloc

#region Ldloca
internal record PushLocalVarAddressToStackPattern : IPattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Newobj,      // 1	0001	newobj	instance void LocalVarIndexOperand::.ctor()
        CilOpCodes.Dup,         // 2	0006	dup
        CilOpCodes.Ldarg_1,     // 3	0007	ldarg.1
        CilOpCodes.Callvirt,    // 4	0008	callvirt	instance void LocalVarIndexOperand::method_4(int32)
        CilOpCodes.Call,        // 5	000D	call	instance void VM::PushStack(class VMOperandType)
        CilOpCodes.Ret          // 6	0012	ret
    };

    public bool Verify(MethodDefinition method, int index = 0)
    {
        var instructions = method.CilMethodBody!.Instructions;
        var localVarIndexOperandCtor = instructions[1].Operand as SerializedMethodDefinition;
        if (localVarIndexOperandCtor?.Name != ".ctor") return false;

        var baseType = localVarIndexOperandCtor.DeclaringType!.BaseType?.Resolve();
        if (baseType == null || baseType.Methods.Count <= 0) return false;

        if (baseType.Methods.Any(m => m.Name != ".ctor" && (m.CilMethodBody?.Instructions.Count != 2 ||
                                                            m.CilMethodBody?.Instructions[1].OpCode !=
                                                            CilOpCodes.Throw)))
            return false;
        return true;
    }
}

internal record Ldloca : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldarg_1,     // 1	0001	ldarg.1
        CilOpCodes.Castclass,   // 2	0002	castclass	VMUShortOperand
        CilOpCodes.Callvirt,    // 3	0007	callvirt	instance uint16 VMUShortOperand::method_3()
        CilOpCodes.Callvirt,    // 4	000C	callvirt	instance void VM::PushLocalVarAddressToStack(int32)
        CilOpCodes.Ret          // 5	0011	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloca;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[3].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.UInt16") return false;

        var pushLocalVarAddressToStackMethod = instructions[4].Operand as SerializedMethodDefinition;
        return PatternMatcher.MatchesPattern(new PushLocalVarAddressToStackPattern(), pushLocalVarAddressToStackMethod!);
    }
}


internal record Ldloca_S : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldarg_1,     // 1	0001	ldarg.1
        CilOpCodes.Castclass,   // 2	0002	castclass	VMByteOperand
        CilOpCodes.Callvirt,    // 3	0007	callvirt	instance uint16 VMByteOperand::method_3()
        CilOpCodes.Callvirt,    // 4	000C	callvirt	instance void VM::PushLocalVarAddressToStack(int32)
        CilOpCodes.Ret          // 5	0011	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloca_S;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[3].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.Byte") return false;

        var pushLocalVarAddressToStackMethod = instructions[4].Operand as SerializedMethodDefinition;
        return PatternMatcher.MatchesPattern(new PushLocalVarAddressToStackPattern(), pushLocalVarAddressToStackMethod!);
    }
}
#endregion Ldloca

#region Ldloc_C
internal record Ldloc_0 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_0,    // 1	0001	ldc.i4.0
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc_0;

    public bool Verify(MethodDefinition method, int index) => 
        PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), 
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!, index);
}

internal record Ldloc_1 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_1,    // 1	0001	ldc.i4.1
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc_1;

    public bool Verify(MethodDefinition method, int index) => 
        PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), 
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!, index);
}

internal record Ldloc_2 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_2,    // 1	0001	ldc.i4.2
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc_2;

    public bool Verify(MethodDefinition method, int index) => 
        PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), 
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!, index);
}

internal record Ldloc_3 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_3,    // 1	0001	ldc.i4.3
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::PushLocalToStack(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Ldloc_3;

    public bool Verify(MethodDefinition method, int index) => 
        PatternMatcher.MatchesPattern(new PushLocalToStackPattern(), 
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!, index);
}
#endregion Ldloc_C

internal record StoreLocalPattern : IPattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 12	001D	ldarg.0
        CilOpCodes.Ldfld,       // 13	001E	ldfld	class VMOperandType[] VM::Locals
        CilOpCodes.Ldarg_1,     // 14	0023	ldarg.1
        CilOpCodes.Ldelem,      // 15	0024	ldelem	VMOperandType
        CilOpCodes.Ldloc_0,     // 16	0029	ldloc.0
        CilOpCodes.Callvirt,    // 17	002A	callvirt	instance class VMOperandType VMOperandType::vmethod_2(class VMOperandType)
        CilOpCodes.Pop,         // 18	002F	pop
        CilOpCodes.Ret          // 19	0030	ret
    };

    public bool MatchEntireBody => false;

    public bool Verify(MethodDefinition method, int index = 0) =>
        method.CilMethodBody!.Instructions[index + 1].Operand as SerializedFieldDefinition ==
        DevirtualizationContext.Instance.VMLocalsField;
}

#region Stloc
internal record Stloc : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_1,     // 0	0000	ldarg.1
        CilOpCodes.Castclass,   // 1	0001	castclass	VMUShortOperand
        CilOpCodes.Stloc_0,     // 2	0006	stloc.0
        CilOpCodes.Ldarg_0,     // 3	0007	ldarg.0
        CilOpCodes.Ldloc_0,     // 4	0008	ldloc.0
        CilOpCodes.Callvirt,    // 5	0009	callvirt	instance uint16 VMUShortOperand::method_3()
        CilOpCodes.Callvirt,    // 6	000E	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 7	0013	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[5].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.UInt16") return false;
        
        var storeLocalMethod = instructions[6].Operand as SerializedMethodDefinition;
        return PatternMatcher.GetAllMatchingInstructions(new StoreLocalPattern(), storeLocalMethod!).Count > 0;
    }
}

internal record Stloc_S : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_1,     // 0	0000	ldarg.1
        CilOpCodes.Castclass,   // 1	0001	castclass	VMByteOperand
        CilOpCodes.Stloc_0,     // 2	0006	stloc.0
        CilOpCodes.Ldarg_0,     // 3	0007	ldarg.0
        CilOpCodes.Ldloc_0,     // 4	0008	ldloc.0
        CilOpCodes.Callvirt,    // 5	0009	callvirt	instance uint16 VMByteOperand::method_3()
        CilOpCodes.Callvirt,    // 6	000E	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 7	0013	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc_S;
    
    public bool Verify(VMOpCode vmOpCode, int index)
    {
        var instructions = vmOpCode.SerializedDelegateMethod.CilMethodBody!.Instructions;
        var getOperandMethod = instructions[5].Operand as SerializedMethodDefinition;
        if (getOperandMethod?.Signature!.ReturnType.FullName != "System.Byte") return false;
        
        var storeLocalMethod = instructions[6].Operand as SerializedMethodDefinition;
        return PatternMatcher.GetAllMatchingInstructions(new StoreLocalPattern(), storeLocalMethod!).Count > 0;
    }
}
#endregion Stloc

#region Stloc_C
internal record Stloc_0 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_0,    // 1	0001	ldc.i4.0
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc_0;

    public bool Verify(MethodDefinition method, int index) => PatternMatcher
        .GetAllMatchingInstructions(new StoreLocalPattern(),
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!).Count > 0;
}

internal record Stloc_1 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_1,    // 1	0001	ldc.i4.1
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc_1;

    public bool Verify(MethodDefinition method, int index) => PatternMatcher
        .GetAllMatchingInstructions(new StoreLocalPattern(),
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!).Count > 0;
}

internal record Stloc_2 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_2,    // 1	0001	ldc.i4.2
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc_2;

    public bool Verify(MethodDefinition method, int index) => PatternMatcher
        .GetAllMatchingInstructions(new StoreLocalPattern(),
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!).Count > 0;
}

internal record Stloc_3 : IOpCodePattern
{
    public IList<CilOpCode> Pattern => new List<CilOpCode>
    {
        CilOpCodes.Ldarg_0,     // 0	0000	ldarg.0
        CilOpCodes.Ldc_I4_3,    // 1	0001	ldc.i4.3
        CilOpCodes.Callvirt,    // 2	0002	callvirt	instance void VM::StoreLocal(int32)
        CilOpCodes.Ret          // 3	0007	ret
    };

    public CilOpCode CilOpCode => CilOpCodes.Stloc_3;

    public bool Verify(MethodDefinition method, int index) => PatternMatcher
        .GetAllMatchingInstructions(new StoreLocalPattern(),
            (method.CilMethodBody!.Instructions[2].Operand as SerializedMethodDefinition)!).Count > 0;
}
#endregion Stloc_C