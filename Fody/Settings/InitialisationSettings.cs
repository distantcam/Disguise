using System;
using System.Xml;
using System.Xml.Linq;

namespace Disguise.Settings
{
    public class InitialisationSettings
    {
        private readonly XElement xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitialisationSettings"/> class.
        /// </summary>
        public InitialisationSettings()
        {
            xml = new XElement("Disguise");
        }

        public InitialisationSettings(XElement config)
        {
            xml = config ?? new XElement("Disguise");
        }

        /// <summary>
        /// Gets or sets a value indicating whether the obfuscator should obfuscate all access modifiers as opposed
        /// to just private access modifiers
        /// </summary>
        /// <value>
        /// 	<c>true</c> to obfuscate all modifiers; otherwise, <c>false</c>.
        /// </value>
        public bool ObfuscateAllModifiers
        {
            get { return GetBool("ObfuscateAllModifiers"); }
            set { Set("ObfuscateAllModifiers", value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include the SupressIldasmAttribute on the assembly
        /// </summary>
        /// <value><c>true</c> to include the attribute; otherwise, <c>false</c>.</value>
        public bool SupressIldasm
        {
            get { return GetBool("SupressIldasm", true); }
            set { Set("SupressIldasm", value, true); }
        }

        /// <summary>
        /// Gets or sets the method used to confuse decompilation tools.
        /// </summary>
        /// <value>The method used to confuse decompilation tools.</value>
        public ConfusionMethod ConfuseDecompilationMethod
        {
            get { return GetEnum<ConfusionMethod>("ConfuseDecompilationMethod"); }
            set { Set("ConfuseDecompilationMethod", value); }
        }

        public NamingMethod RenameMethod
        {
            get { return GetEnum<NamingMethod>("RenameMethod"); }
            set { Set("RenameMethod", value); }
        }

        public StringEncryptionMethod EncryptStrings
        {
            get { return GetEnum<StringEncryptionMethod>("EncryptStrings"); }
            set { Set("EncryptStrings", value); }
        }

        private bool GetBool(XName name, bool dfault = false)
        {
            var attribute = xml.Attribute(name);

            if (attribute == null)
                return dfault;

            return XmlConvert.ToBoolean(attribute.Value);
        }

        private string GetString(XName name, string dfault = "")
        {
            var attribute = xml.Attribute(name);

            if (attribute == null)
                return dfault;

            return attribute.Value;
        }

        private T GetEnum<T>(XName name, T dfault = default(T)) where T : struct
        {
            var attribute = xml.Attribute(name);

            if (attribute == null)
                return dfault;

            T result;
            if (Enum.TryParse<T>(attribute.Value, true, out result))
                return result;

            return dfault;
        }

        private void Set<T>(XName name, T value, T dfault = default(T))
        {
            var attribute = xml.Attribute(name);

            if (attribute == null)
            {
                if (!object.Equals(value, dfault))
                    xml.Add(new XAttribute(name, value));
            }
            else
            {
                if (object.Equals(value, dfault))
                    attribute.Remove();
                else
                    attribute.SetValue(value);
            }
        }
    }
}