using Microsoft.AspNetCore.Authorization;

namespace WoofBnB.Api.Authorization;

/// <summary>
/// Reads the "is_active" claim stashed by the JWT bearer OnTokenValidated event
/// (see Program.cs) — no second database call here; OnTokenValidated already loaded
/// the user once to confirm it still exists.
/// </summary>
public sealed class ActiveUserAuthorizationHandler : AuthorizationHandler<ActiveUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveUserRequirement requirement)
    {
        var isActive = context.User.FindFirst("is_active")?.Value == "true";

        if (isActive)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
