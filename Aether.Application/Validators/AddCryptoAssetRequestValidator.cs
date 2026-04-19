using Aether.Application.DTOs;
using FluentValidation;

namespace Aether.Application.Validators;

public class AddCryptoAssetRequestValidator : AbstractValidator<AddCryptoAssetRequest>
{
    public AddCryptoAssetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Asset name is required.");
        RuleFor(x => x.Symbol).NotEmpty().WithMessage("Symbol is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.AcquisitionPrice).GreaterThan(0).WithMessage("Acquisition price must be greater than 0.");
        RuleFor(x => x.Currency).NotEmpty().WithMessage("Currency is required.");
    }
}
