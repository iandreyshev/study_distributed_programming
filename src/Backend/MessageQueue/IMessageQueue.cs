namespace Backend.MessageQueue
{
	public interface IMessageQueue
	{
		void Post(string tag, string message);
	}
}
