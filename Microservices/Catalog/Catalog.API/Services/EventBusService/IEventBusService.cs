
namespace Catalog.API.Services.EventBusService
{
    public interface IEventBusService : IAsyncDisposable
    {
        Task InitializeAsync();
        Task PublishAsync<T>(T message, string routingKey);
    }
}