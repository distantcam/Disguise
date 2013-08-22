using System;
using System.Linq;
using Mono.Cecil;

namespace Disguise
{
    internal static class CecilCloningChamber
    {
        public static MethodDefinition Clone(ModuleDefinition module, MethodDefinition method)
        {
            MethodDefinition newMethod = new MethodDefinition(method.Name, method.Attributes, module.TypeSystem.Void);
            newMethod.Attributes = method.Attributes;
            newMethod.ImplAttributes = method.ImplAttributes;

            if (method.IsPInvokeImpl)
            {
                newMethod.PInvokeInfo = method.PInvokeInfo;
                bool has = false;
                foreach (ModuleReference modRef in module.ModuleReferences)
                    if (modRef.Name == newMethod.PInvokeInfo.Module.Name)
                    {
                        has = true;
                        newMethod.PInvokeInfo.Module = modRef;
                        break;
                    }
                if (!has)
                    module.ModuleReferences.Add(newMethod.PInvokeInfo.Module);
            }

            return newMethod;
        }
    }
}