using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BlockShop.Api.Features.Blocks;

// ReSharper disable UnusedType.Global

public static class DeleteBlock
{
    public record Command(Guid Id) : IRequest<Result>;

    internal sealed class Handler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.GetLoggedInUserId<string>();
            if (userId is null || !Guid.TryParse(userId, out _))
                return Result.Failure(new Error("DeleteBlock.NoCreator", "No creator found"));

            var block = await context
                .Blocks
                .Where(b => b.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (block is null)
                return Result.Failure(new Error("DeleteBlock.NotFound",
                    "The block with the specified ID was not found"));

            if (!block.CreatorId.Equals(Guid.Parse(userId)))
                return Result.Failure(new Error("DeleteBlock.Forbidden", "The logged user does not own this block"));

            context.Blocks.Remove(block);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class DeleteBlockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("blocks/{id:Guid}", async (Guid id, ISender sender) =>
            {
                var query = new DeleteBlock.Command(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags(nameof(Block));
    }
}