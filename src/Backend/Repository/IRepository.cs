namespace Backend.Repository
{
	public interface IRepository
	{
		string GetString(string key);

		void SetString(string key, string value);
	}
}
