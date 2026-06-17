using ECommerce.Order.Application.Features.Commands;
using FluentValidation;

namespace ECommerce.Order.Application.Features.Validators;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Dto.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required.")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters.");

        RuleFor(x => x.Dto.UserEmail)
            .NotEmpty().WithMessage("User email is required.")
            .EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Dto.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be at least 1.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("Item price must be greater than zero.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name is required.");
        });
    }
}
