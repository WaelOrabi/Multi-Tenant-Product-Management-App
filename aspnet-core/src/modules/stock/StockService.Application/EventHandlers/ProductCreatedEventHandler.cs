using System.Threading.Tasks;
using ProductService.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace StockService.EventHandlers;

public class ProductCreatedEventHandler : IDistributedEventHandler<ProductCreatedEto>, ITransientDependency
{
    public Task HandleEventAsync(ProductCreatedEto eventData)
    {
        // TODO: react to product creation (e.g., initialize stock projections)
        return Task.CompletedTask;
    }
}
