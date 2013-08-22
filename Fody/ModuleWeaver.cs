using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Disguise.Settings;
using Disguise.Tasks;
using Mono.Cecil;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }

    public XElement Config { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        LogError = s => { };
    }

    public void Execute()
    {
        LoggerFactory.LogInfo = LogInfo;
        LoggerFactory.LogWarn = LogWarning;
        LoggerFactory.LogError = LogError;

        var settings = new DisguiseConfig(Config);

        var tasks = CreateTasks(settings);

        var assembly = ModuleDefinition.Assembly;

        Analyse(tasks, assembly);

        Process(tasks, assembly);
    }

    private static IEnumerable<BaseTask> CreateTasks(DisguiseConfig config)
    {
        return new BaseTask[] { new EncryptionTask() };
    }

    private static void Analyse(IEnumerable<BaseTask> tasks, AssemblyDefinition assembly)
    {
        Parallel.ForEach(tasks, task => task.AnalyseAssembly(assembly));

        foreach (var module in assembly.Modules)
        {
            Parallel.ForEach(tasks, task => task.AnalyseModule(module));

            foreach (var type in module.Types)
            {
                Parallel.ForEach(tasks, task => task.AnalyseType(type));

                foreach (var method in type.Methods)
                {
                    Parallel.ForEach(tasks, task => task.AnalyseMethod(method));
                }
            }
        }
    }

    private void Process(IEnumerable<BaseTask> tasks, AssemblyDefinition assembly)
    {
        foreach (var task in tasks)
        {
            task.ProcessAssembly(assembly);

            foreach (var module in assembly.Modules)
            {
                task.ProcessModule(module);

                foreach (var type in module.Types)
                {
                    task.ProcessType(type);

                    foreach (var method in type.Methods)
                    {
                        task.ProcessMethod(method);
                    }
                }
            }
        }
    }
}