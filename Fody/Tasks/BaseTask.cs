using System;
using System.Linq;
using Disguise.Settings;
using Mono.Cecil;

namespace Disguise.Tasks
{
    internal abstract class BaseTask
    {
        public abstract string Name { get; }
        public abstract DisguiseLevel Level { get; }

        public virtual void AnalyseAssembly(AssemblyDefinition assembly)
        {
        }

        public virtual void AnalyseModule(ModuleDefinition module)
        {
        }

        public virtual void AnalyseType(TypeDefinition type)
        {
        }

        public virtual void AnalyseMethod(MethodDefinition method)
        {
        }

        public virtual void ProcessAssembly(AssemblyDefinition assembly)
        {
        }

        public virtual void ProcessModule(ModuleDefinition module)
        {
        }

        public virtual void ProcessType(TypeDefinition type)
        {
        }

        public virtual void ProcessMethod(MethodDefinition method)
        {
        }
    }
}