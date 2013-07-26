#define VERBOSE

using System;
using System.Collections.Generic;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace TiviT.NCloak.CloakTasks
{
    public class StringEncryptionTask : ICloakTask
    {
        private readonly CloakContext context;
        private readonly Random random;

        public StringEncryptionTask(CloakContext context)
        {
            this.context = context;
            random = new Random();
        }

        public string Name { get { return "Encrypting strings"; } }

        public void RunTask()
        {
            //Go through each assembly and encrypt the strings
            //for each assembly inject a decryption routine - we'll let the obfuscator hide it properly
            //Loop through each assembly and obfuscate it

            //Add an encryption function
            MethodReference decryptionMethod = null;

            //Generate a new type for decryption
            Log.Information("Generating global decrypt method");
            foreach (TypeDefinition td in context.AssemblyDefinition.MainModule.GetAllTypes())
                if (td.Name == "<Module>")
                {
                    MethodDefinition md = new MethodDefinition("Decrypt", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.CompilerControlled, context.AssemblyDefinition.Import(typeof(string)));

                    //Generate the parameters
                    md.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, context.AssemblyDefinition.Import(typeof(string))));
                    md.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, context.AssemblyDefinition.Import(typeof(int))));

                    //Add it
                    td.Methods.Add(md);
                    //We now need to create a method body
                    md.Body = new MethodBody(md);

                    //Output the encryption method body
                    switch (context.Settings.EncryptStrings)
                    {
                        case StringEncryptionMethod.Xor:
                            GenerateXorDecryptionMethod(context.AssemblyDefinition, md.Body);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    //Finally get the reference
                    decryptionMethod = md.GetElementMethod();
                }

            //Loop through the modules
            Log.Information("Processing modules");
            foreach (ModuleDefinition moduleDefinition in context.AssemblyDefinition.Modules)
            {
                //Go through each type
                foreach (TypeDefinition typeDefinition in moduleDefinition.GetAllTypes())
                {
                    //Go through each method
                    foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                    {
                        if (methodDefinition.HasBody)
                        {
                            ProcessInstructions(context.AssemblyDefinition, methodDefinition.Body, decryptionMethod);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates the xor decryption method.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="body">The body.</param>
        private static void GenerateXorDecryptionMethod(AssemblyDefinition assembly, MethodBody body)
        {
            var worker = body.GetILProcessor();

            //Generate the decryption method
            //Since this is XOR it is the same as the encryption method
            //In reality its a bit of a joke calling this encryption as its really
            //just obfusaction
            /*
                char[] characters = value.ToCharArray();
                for (int i = 0; i < characters.Length; i++)
                {
                    characters[i] = (char)(characters[i] ^ salt);
                }
                return new String(characters);
             */

            //Declare a local to store the char array
            body.InitLocals = true;
            body.Method.AddLocal(typeof(char[]));
            body.Method.AddLocal(typeof(int));

            var toCharArrayMethodRef = assembly.Import(typeof(string).GetMethod("ToCharArray", Type.EmptyTypes));
            var constructor = assembly.Import(typeof(string).GetConstructor(new[] { typeof(char[]) }));

            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Append(worker.Create(OpCodes.Callvirt, toCharArrayMethodRef));
            worker.Append(worker.Create(OpCodes.Stloc_0));
            worker.Append(worker.Create(OpCodes.Ldc_I4_0));
            var justBeforeLoop = worker.Create(OpCodes.Stloc_1);
            worker.Append(justBeforeLoop);

            // Loop start
            var loopStart = worker.Create(OpCodes.Ldloc_0);
            worker.Append(loopStart);
            worker.Append(worker.Create(OpCodes.Ldloc_1));
            worker.Append(worker.Create(OpCodes.Ldloc_0));
            worker.Append(worker.Create(OpCodes.Ldloc_1));
            worker.Append(worker.Create(OpCodes.Ldelem_U2)); //Load the array
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Xor)); //Do the xor
            worker.Append(worker.Create(OpCodes.Conv_U2));
            worker.Append(worker.Create(OpCodes.Stelem_I2)); //Store back in the array
            worker.Append(worker.Create(OpCodes.Ldloc_1));
            worker.Append(worker.Create(OpCodes.Ldc_I4_1));
            worker.Append(worker.Create(OpCodes.Add));
            worker.Append(worker.Create(OpCodes.Stloc_1));

            var loop = worker.Create(OpCodes.Ldloc_1);
            worker.Append(loop);
            worker.InsertAfter(justBeforeLoop, worker.Create(OpCodes.Br_S, loop));
            worker.Append(worker.Create(OpCodes.Ldloc_0));
            worker.Append(worker.Create(OpCodes.Ldlen));
            worker.Append(worker.Create(OpCodes.Conv_I4));
            worker.Append(worker.Create(OpCodes.Blt_S, loopStart));

            worker.Append(worker.Create(OpCodes.Ldloc_0));
            worker.Append(worker.Create(OpCodes.Newobj, constructor));
            worker.Append(worker.Create(OpCodes.Ret));
        }

        /// <summary>
        /// Processes the instructions replacing all strings being loaded with an encrypted version.
        /// </summary>
        /// <param name="assemblyDef">The assembly definition.</param>
        /// <param name="body">The body.</param>
        /// <param name="decryptMethod">The decrypt method.</param>
        private void ProcessInstructions(AssemblyDefinition assemblyDef, MethodBody body, MethodReference decryptMethod)
        {
            var instructions = body.Instructions;
            var il = body.GetILProcessor();

            List<Instruction> instructionsToExpand = new List<Instruction>();
            List<int> offsets = new List<int>();
            foreach (Instruction instruction in instructions)
            {
                //Find the call statement
                switch (instruction.OpCode.Name)
                {
                    case "ldstr":
                        //We've found a string load message - we need to replace this instruction
                        if (instruction.Operand is string) //Only do the direct strings for now
                            instructionsToExpand.Add(instruction);
                        break;
                }
            }
            //Fix each ldstr instruction found
            foreach (Instruction instruction in instructionsToExpand)
            {
                //What we do is replace the ldstr "bla" with:
                //ldstr bytearray encrypted_array
                //ldc.i4 random_integer
                //call string class Decrypt(string, int32)

                //First get the original value
                string originalValue = instruction.Operand.ToString();
                offsets.Add(instruction.Offset);

                //Secondly generate a random integer as a salt
                int salt = random.Next(5000, 10000);

                //Now we need to work out what the encrypted value is and set the operand
                Log.Information("Encrypting string \"{0}\"", originalValue);
                string byteArray = EncryptString(originalValue, salt);
                Instruction loadString = il.Create(OpCodes.Ldstr, byteArray);
                il.Replace(instruction, loadString);

                //Now load the salt
                Instruction loadSalt = il.Create(OpCodes.Ldc_I4, salt);
                il.InsertAfter(loadString, loadSalt);

                //Process the decryption
                Instruction call = il.Create(OpCodes.Call, decryptMethod);
                il.InsertAfter(loadSalt, call);
            }

            //Unfortunately one thing Mono.Cecil doesn't do is adjust instruction offsets for branch statements
            //and exception handling start points. We need to fix these manually
            if (offsets.Count == 0)
                return;

            //Do the adjustments
            il.AdjustOffsets(body, offsets, 6 + assemblyDef.GetAddressSize());
        }

        /// <summary>
        /// Encrypts the string using the selected encryption method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private string EncryptString(string value, int salt)
        {
            switch (context.Settings.EncryptStrings)
            {
                case StringEncryptionMethod.Xor:
                    return EncryptWithXor(value, salt);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Encrypts the string with the xor method.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private static string EncryptWithXor(string value, int salt)
        {
            char[] characters = value.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = (char)(characters[i] ^ salt);
            }
            return new String(characters);
        }
    }
}