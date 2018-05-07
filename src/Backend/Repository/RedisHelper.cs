using StackExchange.Redis;
using System;

namespace Backend.Repository
{
	public class RedisRepository : IRepository
	{
		private ConnectionMultiplexer Connection => ConnectionMultiplexer.Connect("localhost");

		private const int DATABASE_COUNT = 16;

		public string GetString(string key)
		{
			try
			{
				return GetDatabase(key)
					.StringGet(key);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void SetString(string key, string value)
		{
			GetDatabase(key)
				.StringSet(key, value);
		}

		private IDatabase GetDatabase(string key)
		{
			int databaseId = 0;

			for (int i = 0; i < key.Length; ++i)
			{
				databaseId += key[i];
			}

			databaseId %= DATABASE_COUNT;
			Console.WriteLine("Key: {0}, Redis database: {1}", key, databaseId);

			return Connection.GetDatabase(databaseId);
		}
	}
}
