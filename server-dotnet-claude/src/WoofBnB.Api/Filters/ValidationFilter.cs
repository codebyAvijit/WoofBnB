using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using WoofBnB.Application.Common.Exceptions;
using WoofBnB.Application.Common.Responses;

namespace WoofBnB.Api.Filters;

/// <summary>
/// Runs the FluentValidation validator (if any) registered for each action-method argument
/// before the action executes, mirroring server/src/middlewares/validate.middleware.js:
/// a validation failure short-circuits the request with the exact "Validation Failed" message
/// (server/src/middlewares/validate.middleware.js:17) and a { field, message } list, via
/// AppException so ExceptionHandlingMiddleware renders it as a 400 in the standard error
/// envelope. Model-binding auto-validation is disabled
/// (ApiBehaviorOptions.SuppressModelStateInvalidFilter = true in Program.cs) so this filter
/// is the only source of 400s for invalid request bodies/queries.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var failures = await ValidateAsync(context, argument);

            if (failures.Count > 0)
            {
                throw AppException.Validation(
                    "Validation Failed",
                    failures
                        .Select(failure => new ValidationErrorItem
                        {
                            Field = ToFieldPath(failure.PropertyName),
                            Message = failure.ErrorMessage,
                        })
                        .ToList());
            }
        }

        await next();
    }

    private static async Task<List<ValidationFailure>> ValidateAsync(ActionExecutingContext context, object argument)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
        var validator = context.HttpContext.RequestServices.GetService(validatorType);

        if (validator is not IValidator nonGenericValidator)
        {
            return [];
        }

        var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
        var validationContext = (IValidationContext)Activator.CreateInstance(validationContextType, argument)!;

        var result = await nonGenericValidator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

        return result.Errors;
    }

    /// <summary>
    /// FluentValidation reports PascalCase paths (e.g. "Location.Coordinates"); Zod's
    /// issue.path.join(".") produces camelCase (e.g. "location.coordinates"). Lowercasing
    /// the first character of each dot-separated segment reproduces that for plain nested
    /// properties. RuleForEach's indexed syntax ("Amenities[0]") is converted to Zod's
    /// dot-index equivalent ("amenities.0") by turning "[" into "." and dropping "]"
    /// before the per-segment lowercasing runs (added in the PetSitter phase, the first
    /// array validator in the codebase).
    /// </summary>
    internal static string ToFieldPath(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return string.Empty;
        }

        propertyName = propertyName.Replace("[", ".").Replace("]", "");

        var segments = propertyName.Split('.');

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            if (segment.Length > 0)
            {
                segments[i] = char.ToLowerInvariant(segment[0]) + segment[1..];
            }
        }

        return string.Join('.', segments);
    }
}
