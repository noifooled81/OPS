using Order.Application.Clients.Dtos;

namespace Order.Application.Clients.Interfaces;

public interface IInventoryServiceClient
{
    Task<List<ProductPriceDto>> GetProductPricesAsync(
        IEnumerable<string> productSkus,
        string currency,
        CancellationToken cancellationToken);
}
