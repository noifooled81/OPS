using System;

namespace BuildingBlocks.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Guid Id { get; protected set; }

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Required for ORM / Parameterless initialization
	protected Entity() { }

	protected Entity(Guid id)
	{
		Id = id;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// --- Feature: Operator Overloading ---
	public static bool operator ==(Entity? left, Entity? right)
		=> Equals(left, right);

	public static bool operator !=(Entity? left, Entity? right)
		=> !Equals(left, right);

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Interface Implementations
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public bool Equals(Entity? other)
	{
		if (Object.ReferenceEquals(this, other))
			return true;

		if (other is null)
			return false;

		if (GetType() != other.GetType())
			return false;

		// Treat unpersisted/transient entities as distinct
		if (Id == Guid.Empty || other.Id == Guid.Empty)
			return false;

		return Id == other.Id;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Overridden Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public override bool Equals(object? obj)
		=> (obj is Entity other) && Equals(other);

	public override int GetHashCode() => Id.GetHashCode();
}
