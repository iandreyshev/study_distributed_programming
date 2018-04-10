using System;
using System.Threading;

namespace Backend.Utils
{
	public static class LambdaUtils
	{
		public static void Repeat(int repeatsCount, int pauseMs, Predicate<int> action)
		{
			if (repeatsCount < 1)
			{
				return;
			}

			if (pauseMs < 0)
			{
				pauseMs = 0;
			}

			while (repeatsCount != 0)
			{
				if (action(repeatsCount))
				{
					break;
				}

				Thread.Sleep(pauseMs);
				--repeatsCount;
			}
		}
	}
}
