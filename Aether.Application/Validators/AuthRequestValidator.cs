using Aether.Application.DTOs;
using FluentValidation;

namespace Aether.Application.Validators;

public class AuthRequestValidator : AbstractValidator<AuthRequest>
{
    public AuthRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
