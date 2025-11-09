using DTO.Auth;
using FluentValidation;

namespace API.Validation.Auth;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("Password must not be empty or whitespace.")
            .MinimumLength(6);
    }
}

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("Password must not be empty or whitespace.")
            .MinimumLength(6);

        RuleFor(x => x.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("Confirm password must not be empty or whitespace.")
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}

