using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace TiviT.NCloak.CloakTasks
{
    /// <summary>
    /// Simplifies the assemblies (i.e. turns short codes to long codes)
    /// </summary>
    public class SimplifyTask : ICloakTask
    {
        private readonly CloakContext context;

        public SimplifyTask(CloakContext context)
        {
            this.context = context;
        }

        public string Name { get { return "Simplifying methods"; } }

        public void RunTask()
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
                            //Do the method
                            methodDefinition.Body.SimplifyMacros();
                        }
                    }
                }
            }
        }
    }
}