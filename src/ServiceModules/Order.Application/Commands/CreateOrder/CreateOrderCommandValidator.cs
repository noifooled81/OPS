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

		RuleFor(x => x.Street)
			.NotEmpty().WithMessage("Street is required.");

		RuleFor(x => x.City)
			.NotEmpty().WithMessage("City is required.");

		RuleFor(x => x.ZipCode)
			.NotEmpty().WithMessage("ZipCode is required.");

		RuleFor(x => x.Country)
			.NotEmpty().WithMessage("Country is required.");

		RuleFor(x => x.Items)
			.NotEmpty().WithMessage("At least one order item is required.");

		RuleForEach(x => x.Items).ChildRules(item =>
		{
			item.RuleFor(x => x.ProductSku)
				.NotEmpty().WithMessage("Sku ID is required.");

			item.RuleFor(x => x.Quantity)
				.GreaterThan(0).WithMessage("Quantity must be greater than zero.");

			item.RuleFor(x => x.UnitPrice)
				.GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to zero.");
		});
	}
}
