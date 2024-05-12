using BlockShop.Api.Extensions;
using BlockShop.Api.Shared;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Authentication;

// ReSharper disable UnusedType.Global

namespace BlockShop.Api.Features.Identity;

public static class Logout
{
    public record Command : IRequest<Result<string>>;

    internal sealed class Handler(IHttpContextAccessor httpContextAccessor) : IRequestHandler<Command, Result<string>>
    {
        public Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext is null)
                return Task.FromResult(Result.Failure<string>(new Error("Logout.NoHttpContext", "No Http Context found")));

            var user = httpContext.User.GetLoggedInUserId<string>();
            if (user is null)
                return Task.FromResult(Result.Failure<string>(new Error("Logout.NoLoggedUser", "No logged user found")));

            foreach (var cookie in httpContext.Request.Cookies.Keys)
            {
                httpContext.Response.Cookies.Delete(cookie);
            }

            httpContextAccessor.HttpContext?.SignOutAsync();
            return Task.FromResult(Result.Success(string.Empty));
        }
    }
}

public class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("user/logout", async (ISender sender) =>
            {
                var query = new Logout.Command();
                var result = await sender.Send(query);

                return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok();
            })
            .WithTags("Identity");
    }
}