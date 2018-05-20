using System;

namespace TextStats
{
	static class Converter
	{
		public static int ToInt(string str, int defaultValue)
		{
			try
			{
				return int.Parse(str);
			}
			catch(Exception)
			{
				return defaultValue;
			}
		}

		public static float ToFloat(string str, float defaultValue)
		{
			try
			{
				return float.Parse(str);
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}
	}
}
