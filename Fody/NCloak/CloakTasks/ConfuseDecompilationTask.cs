﻿using System;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace TiviT.NCloak.CloakTasks
{
    public class ConfuseDecompilationTask : ICloakTask
    {
        private readonly CloakContext context;
        private readonly ConfusionMethod method;
        private static readonly Random random = new Random();

        public ConfuseDecompilationTask(CloakContext context)
            : this(context, ConfusionMethod.InvalidIl)
        {
        }

        public ConfuseDecompilationTask(CloakContext context, ConfusionMethod method)
        {
            this.context = context;
            this.method = method;
        }

        public string Name { get { return "Confusing Reflector"; } }

        public void RunTask()
        {
            //We don't need to do this
            if (method == ConfusionMethod.None)
                return;

            switch (method)
            {
                case ConfusionMethod.InvalidIl:
                    ConfuseDecompilationWithInvalidIl(context.AssemblyDefinition);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Confuses the decompilation.
        /// </summary>
        /// <param name="definition">The definition.</param>
        private static void ConfuseDecompilationWithInvalidIl(AssemblyDefinition definition)
        {
            //Go through each type in the assembly
            foreach (TypeDefinition td in definition.MainModule.GetAllTypes())
            {
                //Go through each method and insert invalid il at the beginning
                foreach (MethodDefinition md in td.Methods)
                {
                    if (md.HasBody)
                    {
                        InsertInvalidIl(md.Body);
                    }
                }
                /*
                //Also do constructors
                foreach (MethodDefinition ci in td.Constructors)
                {
                    if (ci.HasBody)
                    {
                        OutputHelper.WriteMethod(td, ci);
                        InsertInvalidIl(ci.Body);
                    }
                }
                 */
            }
        }

        private static void InsertInvalidIl(MethodBody methodBody)
        {
            //Get the instructions and cil worker
            var instructions = methodBody.Instructions;
            if (instructions.Count <= 0)
                return; //We can only do this if we have instructions to work with
            ILProcessor il = methodBody.GetILProcessor();

            //First create an invalid il instruction
            OpCode fakeOpCode = CreateInvalidOpCode();
            Instruction invalidIlInstr1 = il.Create(fakeOpCode);
            Instruction invalidIlInstr2 = il.Create(fakeOpCode);
            Instruction originalFirst = instructions[0];

            //Insert invalid il at the start
            il.InsertBefore(originalFirst, invalidIlInstr1);
            il.InsertBefore(invalidIlInstr1, invalidIlInstr2);

            //Create the branch statement
            Instruction branchStatement = il.Create(OpCodes.Br_S, originalFirst);

            //Add the branch to the start
            il.InsertBefore(invalidIlInstr2, branchStatement);

            //Readjust the offsets
            il.AdjustOffsets(methodBody, 4);
        }

        internal static OpCode CreateInvalidOpCode()
        {
            //We create an opcode using a pretty dodgy method...
            byte op2;
            switch (random.Next(0, 8))
            {
                default:
                    op2 = 0xc1;
                    break;

                case 1:
                    op2 = 0xae;
                    break;

                case 2:
                    op2 = 0xc9;
                    break;

                case 3:
                    op2 = 0xca;
                    break;

                case 4:
                    op2 = 0xaf;
                    break;

                case 5:
                    op2 = 0xa7;
                    break;

                case 6:
                    op2 = 0xc0;
                    break;

                case 7:
                    op2 = 0xbe;
                    break;
            }
            InvalidOpCode invalidOpCode = new InvalidOpCode(0xff, op2);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(invalidOpCode));
            OpCode opCode;
            try
            {
                Marshal.StructureToPtr(invalidOpCode, ptr, false);
                opCode = (OpCode)Marshal.PtrToStructure(ptr, typeof(OpCode));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return opCode;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InvalidOpCode
        {
            public InvalidOpCode(byte op1, byte op2)
            {
                Value = (short)((op1 << 8) | op2);
                Code = op2;
                FlowControl = (byte)Mono.Cecil.Cil.FlowControl.Next;
                OpCodeType = (byte)Mono.Cecil.Cil.OpCodeType.Primitive;
                OperandType = (byte)Mono.Cecil.Cil.OperandType.InlineNone;
                StackBehaviourPop = (byte)StackBehaviour.Pop0;
                StackBehaviourPush = (byte)StackBehaviour.Push0;
            }

            public short Value;
            public byte Code;
            public byte FlowControl;
            public byte OpCodeType;
            public byte OperandType;
            public byte StackBehaviourPop;
            public byte StackBehaviourPush;
        }
    }
}