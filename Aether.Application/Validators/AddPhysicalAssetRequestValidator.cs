using Aether.Application.DTOs;
using FluentValidation;

namespace Aether.Application.Validators;

public class AddPhysicalAssetRequestValidator : AbstractValidator<AddPhysicalAssetRequest>
{
    public AddPhysicalAssetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Asset name is required.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
        RuleFor(x => x.AcquisitionPrice).GreaterThan(0).WithMessage("Acquisition price must be greater than 0.");
        RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required.");
    }
}
