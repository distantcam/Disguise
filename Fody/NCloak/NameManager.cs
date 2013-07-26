using System;
using System.Collections.Generic;

namespace TiviT.NCloak
{
    public class NameManager
    {
        private readonly Dictionary<NamingTable, CharacterSet> namingTables;
        private readonly Dictionary<NamingTable, int> nameCount;

        public NameManager()
        {
            namingTables = new Dictionary<NamingTable, CharacterSet>();
            nameCount = new Dictionary<NamingTable, int>();
            foreach (NamingTable table in Enum.GetValues(typeof(NamingTable)))
                nameCount[table] = 0;
        }

        public void SetCharacterSet(CharacterSet characterSet)
        {
            if (characterSet == DefaultCharacterSet)
            {
                namingTables.Clear();
                return;
            }
            foreach (NamingTable table in Enum.GetValues(typeof(NamingTable)))
                SetCharacterSet(table, characterSet);
        }

        public void SetCharacterSet(NamingTable table, CharacterSet characterSet)
        {
            namingTables[table] = characterSet;
        }

        public string GenerateName(NamingTable table)
        {
            //Check the naming table exists
            if (!namingTables.ContainsKey(table))
                SetCharacterSet(table, DefaultCharacterSet);

            var count = nameCount[table]++;

            //Generate a new name
            if (table == NamingTable.Field) //For fields append an _ to make sure it differs from properties etc
                return "_" + namingTables[table].Generate(count);
            return namingTables[table].Generate(count);
        }

        public static CharacterSet DefaultCharacterSet
        {
            get { return new CharacterSet('\u0800', '\u08ff'); }
        }

        public static CharacterSet ReadableCharacterSet
        {
            get { return new CharacterSet('a', 'z'); }
        }
    }
}