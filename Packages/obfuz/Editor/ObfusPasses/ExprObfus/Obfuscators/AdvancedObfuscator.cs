﻿using dnlib.DotNet.Emit;
using Obfuz.Data;
using Obfuz.Emit;
using Obfuz.Utils;
using System.Collections.Generic;

namespace Obfuz.ObfusPasses.ExprObfus.Obfuscators
{
    class AdvancedObfuscator : BasicObfuscator
    {
        protected bool GenerateIdentityTransformForArgument(Instruction inst, EvalDataType op, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            IRandom random = ctx.localRandom;
            ConstFieldAllocator constFieldAllocator = ctx.constFieldAllocator;
            switch (op)
            {
                case EvalDataType.Int32:
                {
                    //  = x + y = x + (y * a + b) * ra + (-b * ra)
                    int a = random.NextInt() | 0x1;
                    int ra = MathUtil.ModInverse32(a);
                    int b = random.NextInt();
                    int b_ra = -b * ra;
                    float constProbability = 0.5f;
                    ConstObfusUtil.LoadConstInt(a, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    ConstObfusUtil.LoadConstInt(b, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    ConstObfusUtil.LoadConstInt(ra, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    ConstObfusUtil.LoadConstInt(b_ra, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    outputInsts.Add(inst.Clone());
                    return true;
                }
                case EvalDataType.Int64:
                {
                    //  = x + y = x + (y * a + b) * ra + (-b * ra)
                    long a = random.NextLong() | 0x1L;
                    long ra = MathUtil.ModInverse64(a);
                    long b = random.NextLong();
                    long b_ra = -b * ra;
                    float constProbability = 0.5f;
                    ConstObfusUtil.LoadConstLong(a, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    ConstObfusUtil.LoadConstLong(b, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    ConstObfusUtil.LoadConstLong(ra, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    ConstObfusUtil.LoadConstLong(b_ra, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    outputInsts.Add(inst.Clone());
                    return true;
                }
                case EvalDataType.Float:
                {
                    //  = x + y = x + (y + a) * b; a = 0.0f, b = 1.0f
                    float a = 0.0f;
                    float b = 1.0f;
                    float constProbability = 0f;
                    ConstObfusUtil.LoadConstFloat(a, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    ConstObfusUtil.LoadConstFloat(b, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    outputInsts.Add(inst.Clone());
                    return true;
                }
                case EvalDataType.Double:
                {
                    //  = x + y = x + (y + a) * b; a = 0.0, b = 1.0
                    double a = 0.0;
                    double b = 1.0;
                    float constProbability = 0f;
                    ConstObfusUtil.LoadConstDouble(a, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Add));
                    ConstObfusUtil.LoadConstDouble(b, random, constProbability, constFieldAllocator, outputInsts);
                    outputInsts.Add(Instruction.Create(OpCodes.Mul));
                    outputInsts.Add(inst.Clone());
                    return true;
                }
                default: return false;
            }
        }

        public override bool ObfuscateBasicUnaryOp(Instruction inst, EvalDataType op, EvalDataType ret, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            return GenerateIdentityTransformForArgument(inst, op, outputInsts, ctx) || base.ObfuscateBasicUnaryOp(inst, op, ret, outputInsts, ctx);
        }

        public override bool ObfuscateBasicBinOp(Instruction inst, EvalDataType op1, EvalDataType op2, EvalDataType ret, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            return GenerateIdentityTransformForArgument(inst, op2, outputInsts, ctx) || base.ObfuscateBasicBinOp(inst, op1, op2, ret, outputInsts, ctx);
        }

        public override bool ObfuscateUnaryBitwiseOp(Instruction inst, EvalDataType op, EvalDataType ret, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            return GenerateIdentityTransformForArgument(inst, op, outputInsts, ctx) || base.ObfuscateUnaryBitwiseOp(inst, op, ret, outputInsts, ctx);
        }

        public override bool ObfuscateBinBitwiseOp(Instruction inst, EvalDataType op1, EvalDataType op2, EvalDataType ret, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            return GenerateIdentityTransformForArgument(inst, op2, outputInsts, ctx) || base.ObfuscateBinBitwiseOp(inst, op1, op2, ret, outputInsts, ctx);
        }

        public override bool ObfuscateBitShiftOp(Instruction inst, EvalDataType op1, EvalDataType op2, EvalDataType ret, List<Instruction> outputInsts, ObfusMethodContext ctx)
        {
            return GenerateIdentityTransformForArgument(inst, op2, outputInsts, ctx) || base.ObfuscateBitShiftOp(inst, op1, op2, ret, outputInsts, ctx);
        }
    }
}
