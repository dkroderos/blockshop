using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlockShop.Api.Features.Comments;

// ReSharper disable UnusedType.Global

public static class DeleteComment
{
    public record Command(Guid Id) : IRequest<Result>;

    internal sealed class Handler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.GetLoggedInUserId<string>();
            if (userId is null || !Guid.TryParse(userId, out _))
                return Result.Failure(new Error("DeleteComment.NoCreator", "No creator found"));

            var comment = await context
                .Comments
                .Where(b => b.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (comment is null)
                return Result.Failure(new Error("DeleteComment.NotFound",
                    "The comment with the specified ID was not found"));

            if (!comment.CreatorId.Equals(Guid.Parse(userId)))
                return Result.Failure(new Error("DeleteComment.Forbidden", "The logged user does not own this block"));

            context.Comments.Remove(comment);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("comments/{id:Guid}", async (Guid id, ISender sender) =>
            {
                var query = new DeleteComment.Command(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags(nameof(Comment));
    }
}