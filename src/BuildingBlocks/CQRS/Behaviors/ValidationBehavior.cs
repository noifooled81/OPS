using BuildingBlocks.CQRS.Interfaces;
using BuildingBlocks.Exceptions;
using FluentValidation;
using MediatR;

namespace BuildingBlocks.CQRS.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
	IEnumerable<IValidator<TRequest>> validators)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : ICqrsRequest<TResponse>
{
	private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
		{
			return await next(cancellationToken);
		}

		var context = new ValidationContext<TRequest>(request);

		// Run all validators in parallel for better performance
		var validationResults = await Task.WhenAll(
			_validators.Select(v => v.ValidateAsync(context, cancellationToken))
		);

		// Group errors by property name and extract distinct messages
		var failures = validationResults
			.SelectMany(r => r.Errors)
			.Where(f => f != null)
			.GroupBy(
				f => f.PropertyName,
				f => f.ErrorMessage,
				(propertyName, errorMessages) => new
				{
					Key = propertyName,
					Values = errorMessages.Distinct().ToArray()
				})
			.ToDictionary(x => x.Key, x => x.Values);

		if (failures.Count > 0)
		{
			throw new CustomValidationException(failures);
		}

		return await next(cancellationToken);
	}
}
