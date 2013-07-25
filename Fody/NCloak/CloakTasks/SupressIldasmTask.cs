using System;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace TiviT.NCloak.CloakTasks
{
    public class SupressIldasmTask : ICloakTask
    {
        /// <summary>
        /// Gets the task name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Inserting anti-ILDASM code"; }
        }

        /// <summary>
        /// Runs the specified cloaking task.
        /// </summary>
        /// <param name="context">The running context of this cloak job.</param>
        public void RunTask(CloakContext context)
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