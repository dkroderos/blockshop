// ReSharper disable All
namespace BlockShop.Api.Contracts;

public record CreateBlockRequest(string Name, string Description, decimal Price);
public record BlockResponse(Guid Id, string Name, string Description, decimal Price, DateTimeOffset CreatedAt, DateTimeOffset LastUpdatedAt);
