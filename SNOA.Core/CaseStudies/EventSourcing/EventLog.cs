using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// EventLog: σ component for Event Sourcing case study
    /// - σ: EventLog represents the state component in SNOAObject<OrderState, EventLog>
    /// - Part of SNOA structure: X = (V, P, σ) where σ = EventLog
    /// - EventLog stores the sequence of domain events
    /// - Events are immutable and append-only
    /// - CurrentVersion tracks the latest event version
    /// - IsReplaying indicates if events are being replayed (for state projection)
    /// </summary>
    public class EventLog
    {
        /// <summary>
        /// Events: Sequence of domain events
        /// - Immutable, append-only list of events
        /// - Events are ordered by version
        /// - Used to rebuild state by replaying events
        /// </summary>
        public List<DomainEvent> Events { get; set; } = new List<DomainEvent>();

        /// <summary>
        /// CurrentVersion: Current version of the event log
        /// - Incremented when new events are appended
        /// - Used for optimistic concurrency control
        /// - Represents the number of events in the log
        /// </summary>
        public int CurrentVersion { get; set; }

        /// <summary>
        /// IsReplaying: Indicates if events are being replayed
        /// - Set to true during state projection (replaying events)
        /// - Set to false during normal operation
        /// - Used to prevent side effects during replay
        /// </summary>
        public bool IsReplaying { get; set; }

        /// <summary>
        /// Constructor for EventLog
        /// 1. Initialize Events to empty list
        /// 2. Initialize CurrentVersion to 0
        /// 3. Initialize IsReplaying to false
        /// </summary>
        public EventLog()
        {
            Events = new List<DomainEvent>();
            CurrentVersion = 0;
            IsReplaying = false;
        }

        /// <summary>
        /// Object Equality: EventLog equality based on Events, CurrentVersion, and IsReplaying
        /// 1. Check if obj is EventLog type
        /// 2. Compare CurrentVersion for equality
        /// 3. Compare IsReplaying for equality
        /// 4. Compare Events for equality (sequence comparison)
        /// 5. Return true if all components equal
        /// <param name="obj">Object to compare</param>
        /// <returns>True if all components are equal</returns>
        public override bool Equals(object? obj)
        {
            return obj is EventLog other &&
                   CurrentVersion == other.CurrentVersion &&
                   IsReplaying == other.IsReplaying &&
                   Events.Count == other.Events.Count &&
                   Events.SequenceEqual(other.Events);
        }

        /// <summary>
        /// GetHashCode: Hash code based on Events, CurrentVersion, and IsReplaying
        /// 1. Combine CurrentVersion and IsReplaying using HashCode.Combine
        /// 2. Add Events hash code
        /// 3. Return combined hash code
        /// </summary>
        /// <returns>Hash code for EventLog</returns>
        public override int GetHashCode()
        {
            var hash = HashCode.Combine(CurrentVersion, IsReplaying);
            foreach (var evt in Events)
            {
                hash = HashCode.Combine(hash, evt);
            }
            return hash;
        }
    }

    /// <summary>
    /// DomainEvent: Base class for all domain events
    /// - All domain events inherit from this base class
    /// - Events are immutable and represent facts that happened
    /// - Events are stored in EventLog and used to rebuild state
    /// </summary>
    public abstract class DomainEvent
    {
        /// <summary>
        /// EventId: Unique identifier for the event
        /// </summary>
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Timestamp: When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Version: Version of the event (sequence number)
        /// </summary>
        public int Version { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is DomainEvent other &&
                   EventId == other.EventId &&
                   Timestamp == other.Timestamp &&
                   Version == other.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EventId, Timestamp, Version);
        }
    }

    /// <summary>
    /// OrderCreatedEvent: Event representing order creation
    /// </summary>
    public class OrderCreatedEvent : DomainEvent
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ItemAddedEvent: Event representing item addition to order
    /// </summary>
    public class ItemAddedEvent : DomainEvent
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// ItemRemovedEvent: Event representing item removal from order
    /// </summary>
    public class ItemRemovedEvent : DomainEvent
    {
        public string ItemId { get; set; } = string.Empty;
    }

    /// <summary>
    /// OrderShippedEvent: Event representing order shipment
    /// </summary>
    public class OrderShippedEvent : DomainEvent
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime ShippedAt { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// OrderCancelledEvent: Event representing order cancellation
    /// </summary>
    public class OrderCancelledEvent : DomainEvent
    {
        public string OrderId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime CancelledAt { get; set; }
    }
}

