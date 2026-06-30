namespace BuildingBlocks.Domain.Common;

public abstract class Entity : IEquatable<Entity>
{
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Private Fields
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	private int? _requestedHashCode;

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Public Properties
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public Guid Id { get; protected init; }

	private readonly List<Interfaces.IDomainEvent> _domainEvents = [];
	public IReadOnlyCollection<Interfaces.IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	public void AddDomainEvent(Interfaces.IDomainEvent eventItem)
	{
		_domainEvents.Add(eventItem);
	}

	public void RemoveDomainEvent(Interfaces.IDomainEvent eventItem)
	{
		_domainEvents.Remove(eventItem);
	}

	// Remove all events of a specific type
	public void RemoveDomainEventsOfType<TEvent>() where TEvent : Interfaces.IDomainEvent
	{
		_domainEvents.RemoveAll(e => e is TEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Constructors
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

	// Required for ORM / Parameterless initialization
	protected Entity()
	{
		Id = Guid.NewGuid();
	}

	protected Entity(Guid id)
	{
		Id = id == Guid.Empty ? Guid.NewGuid() : id;
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
		if (ReferenceEquals(this, other))
			return true;

		if (other is null)
			return false;

		// EF Core Proxy-safe type checking
		if (GetType().IsAssignableFrom(other.GetType()) == false && other.GetType().IsAssignableFrom(GetType()) == false)
			return false;

		return Id == other.Id;
	}

	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	//   Overridden Methods
	// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
	public override bool Equals(object? obj)
		=> (obj is Entity other) && Equals(other);

	public override int GetHashCode()
	{
		_requestedHashCode ??= Id.GetHashCode();
		return _requestedHashCode.Value;
	}
}
