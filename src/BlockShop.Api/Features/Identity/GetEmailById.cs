using BlockShop.Api.Data;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Identity;

public static class GetEmailById
{
    public record Query(Guid Id) : IRequest<Result<string>>;

    internal sealed class Handler(ApplicationDbContext context)
        : IRequestHandler<Query, Result<string>>
    {
        public async Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await context
                .Users
                .FirstOrDefaultAsync(u => u.Id == request.Id.ToString(), cancellationToken);

            if (user is null)
                return Result.Failure<string>(new Error("GetEmailById.NoEmail", "No Email found"));

            var userEmail = user.Email;
            return userEmail;
        }
    }
}

public class GetEmailByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("user/{id:Guid}/email", async (Guid id, ISender sender) =>
            {
                var query = new GetEmailById.Query(id);
                var result = await sender.Send(query);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            })
            .WithTags("Identity");
    }
}