using BlockShop.Api.Contracts;
using BlockShop.Api.Data;
using BlockShop.Api.Entities;
using BlockShop.Api.Shared;
using Carter;
using FluentValidation;
using Mapster;
using MediatR;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace BlockShop.Api.Features.Blocks;

public static class CreateBlock
{
    public record Command(string Name, string Description, decimal Price) : IRequest<Result<Guid>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
        }
    }

    internal sealed class Handler(ApplicationDbContext context, IValidator<Command> validator)
        : IRequestHandler<Command, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return Result.Failure<Guid>(new Error("CreateBlock.Validation", validationResult.ToString()));

            var block = new Block
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price
            };

            context.Add(block);

            await context.SaveChangesAsync(cancellationToken);

            return block.Id;
        }
    }
}

public class CreateBlockEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("blocks", async (CreateBlockRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateBlock.Command>();

                var result = await sender.Send(command);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result.Value);
            })
            .WithTags(nameof(Block));
    }
}