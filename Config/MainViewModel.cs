using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Disguise.Settings;

namespace Config
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            ConfusionMethods = Enum.GetValues(typeof(ConfusionMethod)).Cast<ConfusionMethod>();
            NamingMethods = Enum.GetValues(typeof(NamingMethod)).Cast<NamingMethod>();
            StringEncryptionMethods = Enum.GetValues(typeof(StringEncryptionMethod)).Cast<StringEncryptionMethod>();

            var disguiseNode = App.FodyWeaveFile.Descendants("Disguise").FirstOrDefault();
            if (disguiseNode == null)
            {
                disguiseNode = new System.Xml.Linq.XElement("Disguise");
                App.FodyWeaveFile.Element("Weavers").Add(disguiseNode);
            }

            var settings = new DisguiseConfig(disguiseNode);
            ObfuscateAllModifiers = settings.ObfuscateAllModifiers;
            SupressIldasm = settings.SupressIldasm;
            ConfuseDecompilationMethod = settings.ConfuseDecompilationMethod;
            RenameMethod = settings.RenameMethod;
            EncryptStrings = settings.EncryptStrings;
        }

        public bool ObfuscateAllModifiers { get; set; }
        public bool SupressIldasm { get; set; }
        public ConfusionMethod ConfuseDecompilationMethod { get; set; }
        public NamingMethod RenameMethod { get; set; }
        public StringEncryptionMethod EncryptStrings { get; set; }

        public IEnumerable<ConfusionMethod> ConfusionMethods { get; set; }
        public IEnumerable<NamingMethod> NamingMethods { get; set; }
        public IEnumerable<StringEncryptionMethod> StringEncryptionMethods { get; set; }

        public void Save()
        {
            var settings = new DisguiseConfig(App.FodyWeaveFile.Descendants("Disguise").First());
            settings.ObfuscateAllModifiers = ObfuscateAllModifiers;
            settings.SupressIldasm = SupressIldasm;
            settings.ConfuseDecompilationMethod = ConfuseDecompilationMethod;
            settings.RenameMethod = RenameMethod;
            settings.EncryptStrings = EncryptStrings;
        }

#pragma warning disable 67

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning restore 67
    }
}