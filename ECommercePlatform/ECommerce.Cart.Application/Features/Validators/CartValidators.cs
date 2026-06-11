using ECommerce.Cart.Application.Features.Commands;
using FluentValidation;

namespace ECommerce.Cart.Application.Features.Validators;

public class AddToCartValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.Dto.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Dto.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Cannot add more than 100 of a single item.");
    }
}

public class UpdateQuantityValidator : AbstractValidator<UpdateQuantityCommand>
{
    public UpdateQuantityValidator()
    {
        RuleFor(x => x.Dto.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Cannot set quantity above 100.");
    }
}
