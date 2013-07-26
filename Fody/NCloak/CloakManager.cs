using System;
using System.Collections.Generic;
using Anotar.Custom;
using TiviT.NCloak.CloakTasks;

namespace TiviT.NCloak
{
    public class CloakManager
    {
        private readonly List<ICloakTask> cloakingTasks;

        public CloakManager()
        {
            cloakingTasks = new List<ICloakTask>();
        }

        private void Configure(CloakContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            cloakingTasks.Add(new SimplifyTask(context));

            //Encrypt strings before anything else
            if (context.Settings.EncryptStrings != StringEncryptionMethod.None)
                cloakingTasks.Add(new StringEncryptionTask(context));

            //Build up a mapping of the assembly and obfuscate
            if (context.Settings.RenameMethod != NamingMethod.None)
            {
                cloakingTasks.Add(new MappingTask(context));
                cloakingTasks.Add(new ObfuscationTask(context));
            }

            if (context.Settings.RenameMethod == NamingMethod.Readable)
            {
                context.NameManager.SetCharacterSet(NameManager.ReadableCharacterSet);
            }

            //Supress ILDASM decompilation
            if (context.Settings.SupressIldasm)
                cloakingTasks.Add(new SupressIldasmTask(context));

            //Try to confuse reflection
            if (context.Settings.ConfuseDecompilationMethod != ConfusionMethod.None)
                cloakingTasks.Add(new ConfuseDecompilationTask(context, ConfusionMethod.InvalidIl));

            //Optimize the assembly (turn into short codes where poss)
            if (context.Settings.ConfuseDecompilationMethod == ConfusionMethod.None) //HACK: The new Mono.Cecil doesn't like bad IL codes
                cloakingTasks.Add(new OptimizeTask(context));

            ////Always last - output the assembly in the relevant format
            //if (String.IsNullOrEmpty(context.Settings.TamperProofAssemblyName))
            //    RegisterTask<OutputAssembliesTask>(); //Default
            //else
            //    RegisterTask<TamperProofTask>(); //Tamper proofing combines all assemblies into one
        }

        public void Run(CloakContext context)
        {
            //Back stop - allows for tests to include only the relevant tasks
            if (cloakingTasks.Count == 0)
                Configure(context);

            //Run through each of our tasks
            foreach (ICloakTask task in cloakingTasks)
            {
                Log.Information("Disguise Task: {0}", task.Name);
                task.RunTask();
            }
        }
    }
}