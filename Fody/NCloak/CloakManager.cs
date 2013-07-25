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

            cloakingTasks.Add(new SimplifyTask());

            //Encrypt strings before anything else
            if (context.Settings.EncryptStrings)
                cloakingTasks.Add(new StringEncryptionTask(StringEncryptionMethod.Xor));

            //Build up a mapping of the assembly and obfuscate
            if (!context.Settings.NoRename)
            {
                cloakingTasks.Add(new MappingTask());
                cloakingTasks.Add(new ObfuscationTask());
            }

            //Supress ILDASM decompilation
            if (context.Settings.SupressIldasm)
                cloakingTasks.Add(new SupressIldasmTask());

            //Try to confuse reflection
            if (context.Settings.ConfuseDecompilationMethod != ConfusionMethod.None)
                cloakingTasks.Add(new ConfuseDecompilationTask(ConfusionMethod.InvalidIl));

            //Optimize the assembly (turn into short codes where poss)
            if (context.Settings.ConfuseDecompilationMethod == ConfusionMethod.None) //HACK: The new Mono.Cecil doesn't like bad IL codes
                cloakingTasks.Add(new OptimizeTask());

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

            //Make sure we have a context
            if (context == null) throw new ArgumentNullException("context");

            //Run through each of our tasks
            foreach (ICloakTask task in cloakingTasks)
            {
                Log.Information(task.Name);
                task.RunTask(context);
            }
        }
    }
}