using FluentValidation;

namespace Aether.Application.Features.Portfolio.Validators;

public class CreatePortfolioRequestValidator : AbstractValidator<CreatePortfolioRequest>
{
    public CreatePortfolioRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Portfolio name is required.")
            .MaximumLength(100).WithMessage("Portfolio name must not exceed 100 characters.");
    }
}
