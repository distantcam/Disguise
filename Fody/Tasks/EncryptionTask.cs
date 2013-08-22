using System;
using System.Linq;
using Disguise.Settings;
using Mono.Cecil;

namespace Disguise.Tasks
{
    internal class EncryptionTask : BaseTask
    {
        public override string Name
        {
            get { return "Encryption"; }
        }

        public override DisguiseLevel Level
        {
            get { return DisguiseLevel.Basic; }
        }

        public override void ProcessType(TypeDefinition type)
        {
            if (type.Name == "<Module>")
            {
                var templateAssembly = AssemblyDefinition.ReadAssembly(typeof(EncryptionTask).Assembly.Location);
                var encryptMethod = templateAssembly.MainModule.GetType("Disguise.Tasks.EncryptionTask").Methods.First(method => method.Name == "Encrypt");
                var cloneEncryptMethod = CecilCloningChamber.Clone(type.Module, encryptMethod);

                cloneEncryptMethod.Attributes &= ~Mono.Cecil.MethodAttributes.Private;

                type.Methods.Add(cloneEncryptMethod);
            }
        }

        private static string Encrypt(string value, int salt)
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