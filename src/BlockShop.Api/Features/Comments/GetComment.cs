using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlockShop.Api.Features.Comments;

// ReSharper disable UnusedType.Global

public static class GetComment
{
    public record Query(Guid Id) : IRequest<Result<CommentResponse>>;

    internal sealed class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<CommentResponse>>
    {
        public async Task<Result<CommentResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var commentResponse = await context
                .Comments
                .Where(c => c.Id == request.Id)
                .Select(c =>
                    new CommentResponse(c.Id, c.CreatorId, c.BlockId, c.Content, c.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            if (commentResponse is null)
                return Result.Failure<CommentResponse>(new Error("GetComment.Null",
                    "The comment with the specified ID was not found"));

            return commentResponse;
        }
    }
}

public class GetCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("comments/{id:Guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetComment.Query(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            })
            .WithTags(nameof(Comment));
    }
}