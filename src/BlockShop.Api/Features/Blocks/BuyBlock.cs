using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Blocks;

public static class BuyBlock
{
    public record Command(Guid Id) : IRequest<Result>;

    internal sealed class Handler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.GetLoggedInUserId<string>();
            if (userId is null || !Guid.TryParse(userId, out _))
                return Result.Failure<Guid>(new Error("BuyBlock.NoLoggedUser", "No logged user found"));

            var block = await context
                .Blocks
                .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (block is null)
                return Result.Failure<Guid>(new Error("BuyBlock.Null",
                    "The block with the specified ID was not found"));

            block.NumberOfBuys++;

            context.Update(block);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class BuyBlockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("blocks/buy/{id:Guid}", async (Guid id, ISender sender) =>
            {
                var query = new BuyBlock.Command(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok();
            })
            .RequireAuthorization()
            .WithTags(nameof(Block));
    }
}