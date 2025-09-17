namespace CogniCache.Shared.Services;

public interface IPubSubService
{
    event Action<IMessage>? Notify;
    void Publish(IMessage message);
}

public class PubSubService : IPubSubService
{
    public event Action<IMessage>? Notify;

    public void Publish(IMessage message)
    {
        Notify?.Invoke(message);
    }
}

public interface IMessage;

public class UpdateTagsMessage : IMessage;
