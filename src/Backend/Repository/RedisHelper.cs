using StackExchange.Redis;
using System;

namespace Backend.Repository
{
	public class RedisRepository : IRepository
	{
		private ConnectionMultiplexer Connection =>
			ConnectionMultiplexer.Connect("localhost");

		private const int DATABASE_COUNT = 16;

		public string GetString(string key)
		{
			try
			{
				return GetDatabase(key, out int dbIndex)
					.StringGet(key);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public string GetString(string key, string defaultValue)
		{
			var result = "";

			try
			{
				result = Connection.GetDatabase()
					.StringGet(key);
			}
			catch (Exception)
			{
				result = defaultValue;
			}

			return result ?? defaultValue;
		}

		public void SetString(string key, string value)
		{
			GetDatabase(key, out int dbIndex)
				.StringSet(key, value);

			Console.WriteLine("Put: {0} to database {1}", value, dbIndex);
		}

		private IDatabase GetDatabase(string key, out int dbIndex)
		{
			int databaseId = 0;

			for (int i = 0; i < key.Length; ++i)
			{
				databaseId += key[i];
			}

			dbIndex = databaseId % DATABASE_COUNT;
			Console.WriteLine("Key: {0}, Redis database: {1}", key, databaseId);

			return Connection.GetDatabase(dbIndex);
		}
	}
}
