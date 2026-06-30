using FluentValidation;

namespace Order.Application.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-character ISO code.");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("IdempotencyKey is required.");

        RuleFor(x => x.BillingStreet)
            .NotEmpty().WithMessage("Billing street is required.");

        RuleFor(x => x.BillingCity)
            .NotEmpty().WithMessage("Billing city is required.");

        RuleFor(x => x.BillingZipCode)
            .NotEmpty().WithMessage("Billing zip code is required.");

        RuleFor(x => x.BillingCountry)
            .NotEmpty().WithMessage("Billing country is required.");

        RuleFor(x => x.ShippingStreet)
            .NotEmpty().WithMessage("Shipping street is required.");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("Shipping city is required.");

        RuleFor(x => x.ShippingZipCode)
            .NotEmpty().WithMessage("Shipping zip code is required.");

        RuleFor(x => x.ShippingCountry)
            .NotEmpty().WithMessage("Shipping country is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one order item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductSku)
                .NotEmpty().WithMessage("Product SKU is required.");
                
            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        });
    }
}
