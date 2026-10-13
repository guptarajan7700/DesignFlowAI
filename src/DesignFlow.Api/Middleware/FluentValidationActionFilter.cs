using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DesignFlow.Api.Middleware;

public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = (IValidationContext)Activator.CreateInstance(
                typeof(ValidationContext<>).MakeGenericType(argument.GetType()),
                argument)!;
            var result = await validator.ValidateAsync(validationContext);
            if (result.IsValid)
            {
                continue;
            }

            context.Result = new BadRequestObjectResult(ToProblem(result));
            return;
        }

        await next();
    }

    private static ValidationProblemDetails ToProblem(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
        return new ValidationProblemDetails(errors)
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest
        };
    }
}
