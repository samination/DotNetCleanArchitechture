using DTO.Products;
using FluentValidation;

namespace API.Validation.Products;

public class ProductCreateRequestDtoValidator : AbstractValidator<ProductCreateRequestDto>
{
    public ProductCreateRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}

public class ProductUpdateRequestDtoValidator : AbstractValidator<ProductUpdateRequestDto>
{
    public ProductUpdateRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.RowVersion)
            .NotNull()
            .Must(rowVersion => rowVersion != null && rowVersion.Length > 0)
            .WithMessage("Concurrency token is required.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(1_000_000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}

public class ProductRequestByIdDtoValidator : AbstractValidator<ProductRequestByIdDto>
{
    public ProductRequestByIdDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public class ProductRequestByNameDtoValidator : AbstractValidator<ProductRequestByNameDto>
{
    public ProductRequestByNameDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(150);
    }
}

