using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BuildingBlocks.Domain.Common;

public abstract class SmartEnum<TEnum, TValue> :
	IEquatable<SmartEnum<TEnum, TValue>>,
	IComparable<SmartEnum<TEnum, TValue>>
	where TEnum : SmartEnum<TEnum, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue>
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Private Fields
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private static readonly Lazy<List<TEnum>> _list = new(CreateList, LazyThreadSafetyMode.ExecutionAndPublication);

	private static readonly Lazy<Dictionary<TValue, TEnum>> _fromValue = new(()
		=> _list.Value.ToDictionary(x => x.Value));

	// Case-insensitive
	private static readonly Lazy<Dictionary<string, TEnum>> _fromName = new(()
		=> _list.Value.ToDictionary(
			x => x.Name,
			x => x,
			StringComparer.OrdinalIgnoreCase));

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public static IReadOnlyList<TEnum> List => _list.Value;

	public string Name { get; }
	public TValue Value { get; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Protected constructor prevents creation of enums from external classes, only derived classes
	protected SmartEnum(string name, TValue value)
	{
		ArgumentNullException.ThrowIfNullOrWhiteSpace(name);
		ArgumentNullException.ThrowIfNull(value);

		Name = name;
		Value = value;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Lookups ---

	// Get enum from value, throw exception when not found
	public static TEnum FromValue(TValue value)
	{
		ArgumentNullException.ThrowIfNull(value);

		if (_fromValue.Value.TryGetValue(value, out var result))
			return result;

		throw new KeyNotFoundException(
			$"The value '{value}' is not a valid underlying value for {typeof(TEnum).Name}.");
	}

	// Try get enum from value, return false when not found
	public static bool TryFromValue(TValue value, [NotNullWhen(true)] out TEnum? result)
	{
		if (value is null)
		{
			result = default;
			return false;
		}

		return _fromValue.Value.TryGetValue(value, out result);
	}

	// Get enum from name, throw exception when not found
	public static TEnum FromName(string name)
	{
		ArgumentNullException.ThrowIfNullOrWhiteSpace(name);

		if (_fromName.Value.TryGetValue(name, out var result))
			return result;

		throw new KeyNotFoundException(
			$"The name '{name}' is not a valid underlying name for {typeof(TEnum).Name}.");
	}

	// Try get enum from name, return false when not found
	public static bool TryFromName(string name, [NotNullWhen(true)] out TEnum? result)
	{
		if (String.IsNullOrWhiteSpace(name))
		{
			result = default;
			return false;
		}

		return _fromName.Value.TryGetValue(name, out result);
	}

	// --- Feature: Equality Operator Overloads ---
	public static bool operator ==(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> left is null ? right is null : left.Equals(right);

	public static bool operator !=(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> !(left == right);

	// --- Feature: Comparison Operator Overloads ---
	public static bool operator <(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> left is null ? right is not null : left.CompareTo(right) < 0;

	public static bool operator >(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> left is not null && left.CompareTo(right) > 0;

	public static bool operator <=(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> left is null || left.CompareTo(right) <= 0;

	public static bool operator >=(SmartEnum<TEnum, TValue>? left, SmartEnum<TEnum, TValue>? right)
		=> left is null ? right is null : left.CompareTo(right) >= 0;

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Interface Implementations
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public virtual bool Equals(SmartEnum<TEnum, TValue>? other)
	{
		// Check if same instance
		if (Object.ReferenceEquals(this, other))
			return true;

		if (other is null)
			return false;

		return Value.Equals(other.Value);
	}

	public virtual int CompareTo(SmartEnum<TEnum, TValue>? other)
	{
		if (Object.ReferenceEquals(this, other))
			return 0;

		if (other is null)
			return 1; // Any instance is larger than null

		return Value.CompareTo(other.Value);
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Overridden Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public override string ToString() => Name;

	public override bool Equals(object? obj)
		=> (obj is SmartEnum<TEnum, TValue> other) && Equals(other);

	public override int GetHashCode() => Value.GetHashCode();

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Private Utilities
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Reflection Initialization ---
	private static List<TEnum> CreateList()
	{
		return [.. typeof(TEnum)
			// Find public, static fields declared directly on enum class
			.GetFields(
				BindingFlags.Public |
				BindingFlags.Static |
				BindingFlags.DeclaredOnly)
			// Ensure the field holds an actual instance of the enum type
			.Where(f => typeof(TEnum).IsAssignableFrom(f.FieldType))
			.Select(f => f.GetValue(null))
			.Cast<TEnum>()
			.OrderBy(x => x.Name)];
	}
}
