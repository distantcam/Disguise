using System;

namespace TiviT.NCloak
{
    public class CharacterSet
    {
        private readonly char startCharacter;
        private readonly char endCharacter;
        private readonly char[] baseChars;

        public CharacterSet(char startCharacter, char endCharacter)
        {
            this.startCharacter = startCharacter;
            this.endCharacter = endCharacter;

            baseChars = new char[endCharacter - startCharacter + 1];
            for (int i = 0; i < baseChars.Length; i++)
                baseChars[i] = (char)(startCharacter + i);
        }

        public char StartCharacter { get { return startCharacter; } }

        public char EndCharacter { get { return endCharacter; } }

        public string Generate(int value)
        {
            // 32 is the worst cast buffer size for base 2 and int.MaxValue
            int i = 32;
            char[] buffer = new char[i];
            int targetBase = baseChars.Length;

            do
            {
                buffer[--i] = baseChars[value % targetBase];
                value = value / targetBase;
            }
            while (value > 0);

            char[] result = new char[32 - i];
            Array.Copy(buffer, i, result, 0, 32 - i);

            return new string(result);
        }
    }
}