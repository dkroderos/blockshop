using System.Linq.Expressions;
using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Shared;
using Carter;
using MediatR;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Comments;

public static class GetBlockComments
{
    public record Query(Guid BlockId, string? SearchTerm, string? SortColumn, string? SortOrder, int? Page, int? PageSize)
        : IRequest<Result<PagedList<CommentResponse>>>;

    internal sealed class Handler(ApplicationDbContext context)
        : IRequestHandler<Query, Result<PagedList<CommentResponse>>>
    {
        public async Task<Result<PagedList<CommentResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var commentsQuery = context
                .Comments
                .Where(c => c.BlockId == request.BlockId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                commentsQuery = commentsQuery.Where(c =>
                    c.Content.Contains(request.SearchTerm));

            Expression<Func<Comment, object>> keySelector = request.SortColumn?.ToLower() switch
            {
                "createdat" => comment => (DateTime)(object)comment.CreatedAt,
                "content" => comment => comment.Content,
                _ => comment => (DateTime)(object)comment.CreatedAt
            };

            commentsQuery = request.SortOrder?.ToLower() == "desc"
                ? commentsQuery.OrderByDescending(keySelector)
                : commentsQuery.OrderBy(keySelector);

            var commentResponsesQuery = commentsQuery
                .Select(c =>
                    new CommentResponse(c.Id, c.CreatorId, c.BlockId, c.Content, c.CreatedAt));

            var comments = await PagedList<CommentResponse>.CreateAsync(
                commentResponsesQuery,
                request.Page ?? 1,
                request.PageSize ?? 10);

            return comments;
        }
    }
}

public class GetBlockCommentsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("blocks/{blockId:Guid}/comments",
                async (Guid blockId, string? searchTerm, string? sortColumn, string? sortOrder, int? page, int? pageSize,
                    ISender sender) =>
                {
                    var query = new GetBlockComments.Query(blockId, searchTerm, sortColumn, sortOrder, page, pageSize);
                    var result = await sender.Send(query);

                    return result.IsFailure ? Results.StatusCode(500) : Results.Ok(result.Value);
                })
            .WithTags(nameof(Comment));
    }
}