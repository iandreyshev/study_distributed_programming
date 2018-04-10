using StackExchange.Redis;
using System;

namespace Backend.Repository
{
	public class RedisRepository : IRepository
	{
		private ConnectionMultiplexer Connection => ConnectionMultiplexer.Connect("localhost");
		private IDatabase Database => Connection.GetDatabase();

		public string GetString(string key)
		{
			try
			{
				return Database.StringGet(key);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public void SetString(string key, string value)
		{
			Database.StringSet(key, value);
		}
	}
}
