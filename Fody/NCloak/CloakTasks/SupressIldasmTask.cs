using System;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace TiviT.NCloak.CloakTasks
{
    public class SupressIldasmTask : ICloakTask
    {
        private readonly CloakContext context;

        public SupressIldasmTask(CloakContext context)
        {
            this.context = context;
        }

        public string Name { get { return "Inserting anti-ILDASM code"; } }

        public void RunTask()
        {
            Type si = typeof(SuppressIldasmAttribute);
            CustomAttribute found = null;
            foreach (CustomAttribute attr in context.AssemblyDefinition.CustomAttributes)
            {
                if (attr.Constructor.DeclaringType.FullName == si.FullName)
                {
                    found = attr;
                    break;
                }
            }

            //Only add if it's not there already
            if (found == null)
            {
                //Add one
                MethodReference constructor = context.AssemblyDefinition.MainModule.Import(typeof(SuppressIldasmAttribute).GetConstructor(Type.EmptyTypes));
                CustomAttribute attr = new CustomAttribute(constructor);
                context.AssemblyDefinition.CustomAttributes.Add(attr);
            }
        }
    }
}