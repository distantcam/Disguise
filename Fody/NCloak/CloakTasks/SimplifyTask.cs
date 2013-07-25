using Anotar.Custom;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace TiviT.NCloak.CloakTasks
{
    /// <summary>
    /// Simplifies the assemblies (i.e. turns short codes to long codes)
    /// </summary>
    public class SimplifyTask : ICloakTask
    {
        public string Name
        {
            get { return "Simplifying methods"; }
        }

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
                            //Do the method
                            Log.Information("> {0}.{1}.{2}", typeDefinition.Namespace, typeDefinition.Name, methodDefinition.Name);
                            methodDefinition.Body.SimplifyMacros();
                        }
                    }
                }
            }
        }
    }
}