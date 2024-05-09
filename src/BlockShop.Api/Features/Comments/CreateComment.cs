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

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BlockShop.Api.Features.Comments;

public static class CreateComment
{
    public record Command(Guid BlockId, string Content) : IRequest<Result<Guid>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.BlockId).NotEmpty();
            RuleFor(c => c.Content).NotEmpty();
        }
    }

    internal sealed class Handler(
        ApplicationDbContext context,
        IValidator<Command> validator,
        IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return Result.Failure<Guid>(new Error("CreateComment.Validation", validationResult.ToString()));

            var userId = httpContextAccessor.HttpContext?.User.GetLoggedInUserId<string>();
            if (userId is null || !Guid.TryParse(userId, out _))
                return Result.Failure<Guid>(new Error("CreateComment.NoCreator", "No creator found"));

            var block = await context
                .Blocks
                .Where(b => b.Id == request.BlockId)
                .FirstOrDefaultAsync(cancellationToken);

            if (block is null)
                return Result.Failure<Guid>(new Error("CreateComment.NoBlock", "No block found"));

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                CreatorId = Guid.Parse(userId),
                BlockId = request.BlockId,
                Content = request.Content,
                Block = block,
                CreatedAt = DateTimeOffset.Now
            };

            context.Add(comment);

            await context.SaveChangesAsync(cancellationToken);

            return block.Id;
        }
    }
}

public class CreateCommentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("comments", async (CreateCommentRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateComment.Command>();

                var result = await sender.Send(command);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result.Value);
            })
            .RequireAuthorization()
            .WithTags(nameof(Comment));
    }
}