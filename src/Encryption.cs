using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaign
{
    internal static class Encryption
    {
		private const int Key = 7;
		private static char Cipher(char ch, int key)
		{
			if (!char.IsLetter(ch))
				return ch;

			char offset = char.IsUpper(ch) ? 'A' : 'a';
			return (char)((((ch + key) - offset) % 26) + offset);
		}

		internal static string Encipher(string input, int key)
		{
			string output = string.Empty;

			foreach (char ch in input)
				output += Cipher(ch, key);

			return output;
		}

		internal static string Decipher(string input)
		{
			return Encipher(input, 26 - Key);
		}
	}
}
