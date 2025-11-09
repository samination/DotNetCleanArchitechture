using DTO.Categories;
using FluentValidation;

namespace API.Validation.Categories;

public class CategoryAddRequestDtoValidator : AbstractValidator<CategoryAddRequestDto>
{
    public CategoryAddRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}

public class CategoryUpdateRequestDtoValidator : AbstractValidator<CategoryUpdateRequestDto>
{
    public CategoryUpdateRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.RowVersion)
            .NotNull()
            .Must(rowVersion => rowVersion != null && rowVersion.Length > 0)
            .WithMessage("Concurrency token is required.");
    }
}

public class CategoryRequestByIdDtoValidator : AbstractValidator<CategoryRequestByIdDto>
{
    public CategoryRequestByIdDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

public class CategoryRequestByNameDtoValidator : AbstractValidator<CategoryRequestByNameDto>
{
    public CategoryRequestByNameDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be empty or whitespace.")
            .MaximumLength(100);
    }
}

