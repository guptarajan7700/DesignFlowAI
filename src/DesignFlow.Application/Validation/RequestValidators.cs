using DesignFlow.Application.Contracts;
using FluentValidation;

namespace DesignFlow.Application.Validation;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MaximumLength(200);
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password).NotEmpty().MinimumLength(8).MaximumLength(200);
        RuleFor(request => request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Phone).MaximumLength(30);
        RuleFor(request => request.Role).MaximumLength(50);
    }
}

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(request => request.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Phone).MaximumLength(30);
        RuleFor(request => request.Website).MaximumLength(300);
        RuleFor(request => request.AddressLine1).MaximumLength(200);
        RuleFor(request => request.City).MaximumLength(100);
        RuleFor(request => request.State).MaximumLength(100);
        RuleFor(request => request.PostalCode).MaximumLength(20);
        RuleFor(request => request.Country).MaximumLength(100);
        RuleFor(request => request.Notes).MaximumLength(4000);
    }
}

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(request => request.ClientId).NotEmpty();
        RuleFor(request => request.Code).NotEmpty().MaximumLength(50);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(8000);
        RuleFor(request => request.Budget).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Spent).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Currency).MaximumLength(3);
        RuleFor(request => request.SiteAddress).MaximumLength(300);
    }
}

public sealed class CreateRequirementRequestValidator : AbstractValidator<CreateRequirementRequest>
{
    public CreateRequirementRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(8000);
        RuleFor(request => request.Category).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Priority).NotEmpty().MaximumLength(20);
    }
}

public sealed class AssignIssueRequestValidator : AbstractValidator<AssignIssueRequest>
{
    public AssignIssueRequestValidator()
    {
        RuleFor(request => request.AssigneeId).NotEmpty();
    }
}

public sealed class SyncModelRequestValidator : AbstractValidator<SyncModelRequest>
{
    public SyncModelRequestValidator()
    {
        RuleFor(request => request.ProjectId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(260);
        RuleFor(request => request.VersionLabel).MaximumLength(50);
        RuleFor(request => request.AutodeskUrn).MaximumLength(500);
    }
}

public sealed class CreateValidationRunRequestValidator : AbstractValidator<CreateValidationRunRequest>
{
    public CreateValidationRunRequestValidator()
    {
        RuleFor(request => request.RevitModelId).NotEmpty();
        RuleFor(request => request.Results).NotEmpty();
        RuleForEach(request => request.Results).ChildRules(check =>
        {
            check.RuleFor(item => item.CheckCode).NotEmpty().MaximumLength(80);
            check.RuleFor(item => item.CheckName).NotEmpty().MaximumLength(200);
            check.RuleFor(item => item.Status).NotEmpty().MaximumLength(20);
            check.RuleFor(item => item.Message).NotEmpty().MaximumLength(1000);
            check.RuleFor(item => item.ElementId).MaximumLength(100);
        });
    }
}

public sealed class CreateAutomationJobRequestValidator : AbstractValidator<CreateAutomationJobRequest>
{
    public CreateAutomationJobRequestValidator()
    {
        RuleFor(request => request.JobType).NotEmpty().MaximumLength(80);
        RuleFor(request => request.RequestPayload).MaximumLength(8000);
    }
}

public sealed class AnalyzeBriefRequestValidator : AbstractValidator<AnalyzeBriefRequest>
{
    public AnalyzeBriefRequestValidator()
    {
        RuleFor(request => request.Brief).NotEmpty().MaximumLength(8000);
    }
}

public sealed class PlanProjectRequestValidator : AbstractValidator<PlanProjectRequest>
{
    public PlanProjectRequestValidator()
    {
        RuleFor(request => request.Brief).MaximumLength(8000);
    }
}

public sealed class PreviewCommandRequestValidator : AbstractValidator<PreviewCommandRequest>
{
    public PreviewCommandRequestValidator()
    {
        RuleFor(request => request.Prompt).NotEmpty().MaximumLength(4000);
    }
}

public sealed class ExecuteCommandRequestValidator : AbstractValidator<ExecuteCommandRequest>
{
    public ExecuteCommandRequestValidator()
    {
        RuleFor(request => request.PreviewId).NotEmpty();
    }
}

public sealed class AskAiRequestValidator : AbstractValidator<AskAiRequest>
{
    public AskAiRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty().MaximumLength(2000);
    }
}
