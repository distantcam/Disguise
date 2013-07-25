using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TiviT.NCloak
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
        /// Gets or sets a value indicating whether the obfuscator should encrypt strings.
        /// </summary>
        /// <value><c>true</c> to encrypt strings; otherwise, <c>false</c>.</value>
        public bool EncryptStrings
        {
            get { return GetBool("EncryptStrings"); }
            set { Set("EncryptStrings", value); }
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

        /// <summary>
        /// Gets or sets the name of the tamper proof assembly. Please note, this
        /// is the .NET friendly name
        /// </summary>
        /// <value>The name of the tamper proof assembly.</value>
        public string TamperProofAssemblyName
        {
            get { return GetString("TamperProofAssemblyName"); }
            set
            {
                //Validate it
                if (String.IsNullOrEmpty(value))
                    Set("TamperProofAssemblyName", "");
                else if (Regex.IsMatch(value, "[A-Za-z][_A-Za-z0-9]*"))
                    Set("TamperProofAssemblyName", value);
                else
                    throw new FormatException("Assembly name must be a valid .NET friendly type name ([A-Za-z][_A-Za-z0-9]*)");
            }
        }

        /// <summary>
        /// Gets or sets the type of the tamper proof assembly.
        /// </summary>
        /// <value>The type of the tamper proof assembly.</value>
        public AssemblyType TamperProofAssemblyType
        {
            get { return GetEnum<AssemblyType>("TamperProofAssemblyType"); }
            set { Set("TamperProofAssemblyType", value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether rename is switched OFF.
        /// </summary>
        /// <value><c>true</c> if rename switched OFF; otherwise, <c>false</c>.</value>
        public bool NoRename
        {
            get { return GetBool("NoRename"); }
            set { Set("NoRename", value); }
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