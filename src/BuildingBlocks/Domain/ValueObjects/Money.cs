using BuildingBlocks.Domain.Enums;

namespace BuildingBlocks.Domain.ValueObjects;

public sealed record Money : IComparable<Money>
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public int Amount { get; init; } // Price in smallest unit to avoid floating-point errors
	public Currency Currency { get; init; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private Money(int amount, Currency currency)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(amount, nameof(amount));
		ArgumentNullException.ThrowIfNull(currency, nameof(currency));

		Amount = amount;
		Currency = currency;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Factory methods for clean object creation ---
	public static Money Create(int amount, Currency currency) => new(amount, currency);
	public static Money Zero(Currency currency) => new(0, currency);

	// --- Feature: Arithmetic ---
	public Money Add(Money other)
	{
		EnsureSameCurrency(other);
		return new Money(Amount + other.Amount, Currency);
	}

	public Money Subtract(Money other)
	{
		EnsureSameCurrency(other);
		// This will naturally trigger the ThrowIfNegative guard inside the constructor
		return new Money(Amount - other.Amount, Currency);
	}

	public Money Multiply(decimal factor)
	{
		// Rounding to nearest integer unit safely
		int newAmount = (int)Math.Round(Amount * factor, MidpointRounding.AwayFromZero);
		return new Money(newAmount, Currency);
	}

	// --- Feature: Operator Overloads ---
	public static Money operator +(Money left, Money right) => left.Add(right);
	public static Money operator -(Money left, Money right) => left.Subtract(right);
	public static Money operator *(Money left, decimal factor) => left.Multiply(factor);
	public static Money operator *(decimal factor, Money right) => right.Multiply(factor);

	public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
	public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
	public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
	public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Interface Implementations
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public int CompareTo(Money? other)
	{
		if (other is null)
			return 1;

		EnsureSameCurrency(other);
		return Amount.CompareTo(other.Amount);
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Private Ultilities
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private void EnsureSameCurrency(Money other)
	{
		ArgumentNullException.ThrowIfNull(other, nameof(other));

		if (Currency != other.Currency)
			throw new InvalidOperationException(
				$"Currency mismatch: Cannot operate between {Currency} and {other.Currency}.");
	}
}
