using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationMvc.Utils
{
	public static class StringUtils
	{
		private static Random random = new Random((int)DateTime.Now.Ticks);
		private const string random_chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

		public static string RandomString(int size)
		{
			char[] buffer = new char[size];
			int numChars = random_chars.Length;

			for (int i = 0; i < size; i++)
			{
				buffer[i] = random_chars[random.Next(numChars)];
			}

			return new string(buffer);
		}
	}
}