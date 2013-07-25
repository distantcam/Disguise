using System;
using System.Linq;
using System.Xml.Linq;
using Mono.Cecil;
using TiviT.NCloak;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }
    public IAssemblyResolver AssemblyResolver { get; set; }
    public string[] DefineConstants { get; set; }

    public XElement Config { get; set; }

    public ModuleWeaver()
    {
        LogInfo = s => { };
        LogWarning = s => { };
        LogError = s => { };
        DefineConstants = new string[0];
    }

    public void Execute()
    {
        LoggerFactory.LogInfo = LogInfo;
        LoggerFactory.LogWarn = LogWarning;
        LoggerFactory.LogError = LogError;

        var settings = new InitialisationSettings(Config);

        var manager = new CloakManager();

        var cloakContext = new CloakContext(settings, ModuleDefinition);

        manager.Run(cloakContext);
    }
}