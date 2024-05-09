using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Blocks;

public static class GetBlock
{
    public record Query(Guid Id) : IRequest<Result<BlockResponse>>;

    internal sealed class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<BlockResponse>>
    {
        public async Task<Result<BlockResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var blockResponse = await context
                .Blocks
                .Where(b => b.Id == request.Id)
                .Select(b => new BlockResponse(b.Id, b.Name, b.Description, b.Price, b.CreatedAt, b.LastUpdatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (blockResponse is null)
                return Result.Failure<BlockResponse>(new Error("GetBlock.Null",
                    "The block with the specified ID was not found"));

            return blockResponse;
        }
    }
}

public class GetBlockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("blocks/{id:Guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetBlock.Query(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            })
            .WithTags(nameof(Block));
    }
}