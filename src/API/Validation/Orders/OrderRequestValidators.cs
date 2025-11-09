using DTO.Orders;
using FluentValidation;

namespace API.Validation.Orders;

public class OrderCreateRequestDtoValidator : AbstractValidator<OrderCreateRequestDto>
{
    public OrderCreateRequestDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}

