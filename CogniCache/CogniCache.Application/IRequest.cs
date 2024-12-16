namespace CogniCache.Application
{
    public interface IRequest<TRequest, TResponse>
    {
        TResponse Handle(TRequest request);
    }
}
