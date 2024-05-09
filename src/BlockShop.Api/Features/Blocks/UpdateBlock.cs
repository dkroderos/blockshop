using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Blocks;

public static class UpdateBlock
{
    public record Command(Guid Id, string Name, string Description) : IRequest<Result>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
        }
    }

    internal sealed class Handler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?.User.GetLoggedInUserId<string>();
            if (userId is null || !Guid.TryParse(userId, out _))
                return Result.Failure(new Error("UpdateBlock.NoCreator", "No creator found"));

            var block = await context
                .Blocks
                .Where(b => b.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (block is null)
                return Result.Failure(new Error("UpdateBlock.NotFound",
                    "The block with the specified ID was not found"));

            if (!block.CreatorId.Equals(Guid.Parse(userId)))
                return Result.Failure(new Error("UpdateBlock.Forbidden", "The logged user does not own this block"));

            block.Name = request.Name;
            block.Description = request.Description;
            block.LastUpdatedAt = DateTimeOffset.Now;

            context.Update(block);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

public class UpdateBlockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("blocks", async (UpdateBlockRequest request, ISender sender) =>
            {
                var command = request.Adapt<UpdateBlock.Command>();

                var result = await sender.Send(command);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.NoContent();
            })
            .RequireAuthorization()
            .WithTags(nameof(Block));
    }
}