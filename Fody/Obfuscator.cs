using System;
using System.Linq;
using Disguise.Tasks;
using Mono.Cecil;

public class Obfuscator
{
    private readonly Config config;

    public Obfuscator(Config config)
    {
        this.config = config;
    }

    public void Process(ModuleDefinition moduleDefinition)
    {
        if (config.SupressILdasm)
            SuppressIldasmTask.RunTask(moduleDefinition);

        if (config.EncryptStrings)
            StringEncryptionTask.RunTask(moduleDefinition);
    }
}