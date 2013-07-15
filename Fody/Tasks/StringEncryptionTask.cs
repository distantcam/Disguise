using System;
using System.Linq;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Disguise.Tasks
{
    public class StringEncryptionTask
    {
        public static void RunTask(ModuleDefinition moduleDefinition)
        {
            var decryptMethod = new Lazy<MethodReference>(() =>
            {
                var moduleInit = moduleDefinition.Types.First(type => type.Name == "<Module>");

                MethodDefinition md = new MethodDefinition("Decrypt", MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.CompilerControlled, moduleDefinition.TypeSystem.String);

                md.Parameters.Add(new ParameterDefinition("v", ParameterAttributes.None, moduleDefinition.TypeSystem.String));
                md.Parameters.Add(new ParameterDefinition("s", ParameterAttributes.None, moduleDefinition.TypeSystem.Int32));

                moduleInit.Methods.Add(md);
                md.Body = new MethodBody(md);

                GenerateDecryptionMethod(moduleDefinition, md.Body);

                return md.GetElementMethod();
            });

            foreach (TypeDefinition typeDefinition in moduleDefinition.Types)
                foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
                    if (methodDefinition.HasBody)
                        ProcessMethod(methodDefinition, decryptMethod);
        }

        private static void ProcessMethod(MethodDefinition methodDefinition, Lazy<MethodReference> decryptMethod)
        {
            var random = new Random();

            var stringInstructions = methodDefinition.Body.Instructions.Where(ins => ins.OpCode == OpCodes.Ldstr && ins.Operand is String).ToList();

            if (stringInstructions.Any())
                methodDefinition.Body.SimplifyMacros();

            var il = methodDefinition.Body.GetILProcessor();

            try
            {
                foreach (var instruction in stringInstructions)
                {
                    //First get the original value
                    string originalValue = instruction.Operand.ToString();

                    //Secondly generate a random integer as a salt
                    int salt = random.Next(5000, 10000);

                    //Now we need to work out what the encrypted value is and set the operand
                    string byteArray = EncryptString(originalValue, salt);
                    Instruction loadString = il.Create(OpCodes.Ldstr, byteArray);
                    il.Replace(instruction, loadString);

                    //Now load the salt
                    Instruction loadSalt = il.Create(OpCodes.Ldc_I4, salt);
                    il.InsertAfter(loadString, loadSalt);

                    //Process the decryption
                    Instruction call = il.Create(OpCodes.Call, decryptMethod.Value);
                    il.InsertAfter(loadSalt, call);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error encrypting strings in method '{0}'.", methodDefinition.FullName);
            }
            finally
            {
                methodDefinition.Body.OptimizeMacros();
            }
        }

        private static string EncryptString(string value, int salt)
        {
            char[] characters = value.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i] = (char)(characters[i] ^ salt);
            }
            return new String(characters);
        }

        private static void GenerateDecryptionMethod(ModuleDefinition moduleDefinition, MethodBody body)
        {
            var worker = body.GetILProcessor();

            //Declare a local to store the char array
            body.InitLocals = true;
            body.Variables.Add(new VariableDefinition(moduleDefinition.Import(typeof(char[]))));
            body.Variables.Add(new VariableDefinition(moduleDefinition.TypeSystem.Int32));

            var toCharArrayMethodRef = moduleDefinition.Import(typeof(string).GetMethod("ToCharArray", Type.EmptyTypes));
            var constructor = moduleDefinition.Import(typeof(string).GetConstructor(new[] { typeof(char[]) }));

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
            worker.Append(worker.Create(OpCodes.Ldelem_U2));
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Xor));
            worker.Append(worker.Create(OpCodes.Conv_U2));
            worker.Append(worker.Create(OpCodes.Stelem_I2));
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
    }
}