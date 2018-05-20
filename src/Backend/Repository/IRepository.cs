namespace Backend.Repository
{
	public interface IRepository
	{
		string GetString(string key);
		string GetString(string key, string defaultValue);

		void SetString(string key, string value);
	}
}
