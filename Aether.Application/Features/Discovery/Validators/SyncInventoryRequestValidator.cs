using FluentValidation;

namespace Aether.Application.Features.Discovery.Validators;

public class SyncInventoryRequestValidator : AbstractValidator<SyncInventoryRequest>
{
    public SyncInventoryRequestValidator()
    {
        RuleFor(x => x.AppIds)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one AppId is required.");
    }
}
