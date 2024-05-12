using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using MediatR;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Identity;

public static class GetLoggedUserEmail
{
    public record Query : IRequest<Result<string>>;

    internal sealed class Handler(IHttpContextAccessor httpContextAccessor) : IRequestHandler<Query, Result<string>>
    {
        public Task<Result<string>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userEmail = httpContextAccessor.HttpContext?.User.GetLoggedInUserEmail();
            return Task.FromResult(string.IsNullOrEmpty(userEmail)
                ? Result.Failure<string>(new Error("GetEmail.NoEmail", "No Email found"))
                : userEmail);
        }
    }
}

public class GetLoggedUserEmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("user/email", async (ISender sender) =>
            {
                var query = new GetLoggedUserEmail.Query();
                var result = await sender.Send(query);

                return result.IsFailure ? Results.NotFound(result.Error) : Results.Ok(result.Value);
            })
            .WithTags("Identity");
    }
}