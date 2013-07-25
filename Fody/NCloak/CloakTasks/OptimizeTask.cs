using System;
using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace TiviT.NCloak.CloakTasks
{
    /// <summary>
    /// Optimizes the assemblies (i.e. turns long codes to short codes where necessary)
    /// </summary>
    public class OptimizeTask : ICloakTask
    {
        /// <summary>
        /// Gets the task name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Optimizing methods"; }
        }

        /// <summary>
        /// Runs the specified cloaking task.
        /// </summary>
        /// <param name="context">The running context of this cloak job.</param>
        public void RunTask(CloakContext context)
        {
            //We'll search methods only at this point
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
                            Log.Information("> {0}.{1}.{2}", typeDefinition.Namespace, typeDefinition.Name, methodDefinition.Name);
                            methodDefinition.Body.OptimizeMacros();
                        }
                    }
                }
            }
        }
    }
}