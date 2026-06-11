using ECommerce.Product.Application.Features.Commands;
using FluentValidation;

namespace ECommerce.Product.Application.Features.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Dto.Description)
            .NotEmpty().WithMessage("Product description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.Dto.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Dto.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Dto.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.");
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        // Only validate fields that are actually provided
        When(x => x.Dto.Name is not null, () =>
            RuleFor(x => x.Dto.Name!)
                .NotEmpty().WithMessage("Product name cannot be empty.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters."));

        When(x => x.Dto.Price.HasValue, () =>
            RuleFor(x => x.Dto.Price!.Value)
                .GreaterThan(0).WithMessage("Price must be greater than zero."));

        When(x => x.Dto.Description is not null, () =>
            RuleFor(x => x.Dto.Description!)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters."));
    }
}

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
