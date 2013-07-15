using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;

namespace Disguise.Tasks
{
    public static class SuppressIldasmTask
    {
        public static void RunTask(ModuleDefinition moduleDefinition)
        {
            var assemblyDefinition = moduleDefinition.Assembly;

            var suppressIldasmAttribute = typeof(SuppressIldasmAttribute);
            if (!assemblyDefinition.CustomAttributes.Any(attr => attr.AttributeType.FullName == suppressIldasmAttribute.FullName))
            {
                var constructor = moduleDefinition.Import(suppressIldasmAttribute.GetConstructor(Type.EmptyTypes));
                assemblyDefinition.CustomAttributes.Add(new CustomAttribute(constructor));
            }
        }
    }
}