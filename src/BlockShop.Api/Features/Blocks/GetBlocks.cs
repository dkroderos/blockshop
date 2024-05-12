using System.Linq.Expressions;
using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Shared;
using Carter;
using MediatR;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BlockShop.Api.Features.Blocks;

public static class GetBlocks
{
    public record Query(string? SearchTerm, string? SortColumn, string? SortOrder, int? Page, int? PageSize)
        : IRequest<Result<PagedList<BlockResponse>>>;

    internal sealed class Handler(ApplicationDbContext context)
        : IRequestHandler<Query, Result<PagedList<BlockResponse>>>
    {
        public async Task<Result<PagedList<BlockResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Block> blocksQuery = context.Blocks;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                blocksQuery = blocksQuery.Where(b =>
                    b.Name.Contains(request.SearchTerm) ||
                    b.Description.Contains(request.SearchTerm));

            Expression<Func<Block, object>> keySelector = request.SortColumn?.ToLower() switch
            {
                "description" => block => block.Description,
                "createdat" => block => (DateTime)(object)block.CreatedAt,
                "lastupdatedat" => block => (DateTime)(object)block.LastUpdatedAt,
                "price" => block => block.Price,
                "numberofbuys" => block => block.NumberOfBuys,
                _ => block => block.Name
            };

            blocksQuery = request.SortOrder?.ToLower() == "desc"
                ? blocksQuery.OrderByDescending(keySelector)
                : blocksQuery.OrderBy(keySelector);

            var blockResponsesQuery = blocksQuery
                .Select(b =>
                    new BlockResponse(b.Id, b.CreatorId, b.Name, b.Description, b.Image, b.Price, b.NumberOfBuys,
                        b.CreatedAt, b.LastUpdatedAt));

            var blocks = await PagedList<BlockResponse>.CreateAsync(
                blockResponsesQuery,
                request.Page ?? 1,
                request.PageSize ?? 10);

            return blocks;
        }
    }
}

public class GetBlocksEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("blocks",
                async (string? searchTerm, string? sortColumn, string? sortOrder, int? page, int? pageSize,
                    ISender sender) =>
                {
                    var query = new GetBlocks.Query(searchTerm, sortColumn, sortOrder, page, pageSize);
                    var result = await sender.Send(query);

                    return result.IsFailure ? Results.StatusCode(500) : Results.Ok(result.Value);
                })
            .WithTags(nameof(Block));
    }
}