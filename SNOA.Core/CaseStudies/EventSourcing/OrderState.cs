using System.Collections.Generic;
using System.Linq;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// OrderState: V component for Event Sourcing case study
    /// - V: OrderState represents the main value component in SNOAObject<OrderState, EventLog>
    /// - Part of SNOA structure: X = (V, P, σ) where V = OrderState
    /// - V is the main value (core data)
    /// - OrderState represents the current projected state of an order aggregate
    /// - State is derived by replaying events from the event log
    /// - Immutable: Each event creates a new state
    /// </summary>
    public class OrderState
    {
        /// <summary>
        /// OrderId: Unique identifier for the order
        /// - Aggregate root identifier
        /// - Used to load events from event store
        /// - Immutable once created
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// TotalAmount: Total amount of the order
        /// - Calculated by replaying ItemAddedEvent and ItemRemovedEvent
        /// - Updated when order items change
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Status: Current status of the order
        /// - Updated by OrderCreatedEvent, OrderShippedEvent, OrderCancelledEvent
        /// - Represents current state of the order lifecycle
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Items: List of order items
        /// - Updated by ItemAddedEvent and ItemRemovedEvent
        /// - Represents current items in the order
        /// </summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Constructor for OrderState
        /// 1. Initialize OrderId with provided orderId
        /// 2. Initialize TotalAmount to 0
        /// 3. Initialize Status to Created
        /// 4. Initialize Items to empty list
        /// </summary>
        /// <param name="orderId">Unique identifier for the order</param>
        public OrderState(string orderId)
        {
            OrderId = orderId;
            TotalAmount = 0;
            Status = OrderStatus.Created;
            Items = new List<OrderItem>();
        }

        /// <summary>
        /// Object Equality: OrderState equality based on OrderId, TotalAmount, Status, and Items
        /// 1. Check if obj is OrderState type
        /// 2. Compare OrderId for equality
        /// 3. Compare TotalAmount for equality
        /// 4. Compare Status for equality
        /// 5. Compare Items for equality (sequence comparison)
        /// 6. Return true if all components equal
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if all components are equal</returns>
        public override bool Equals(object? obj)
        {
            return obj is OrderState other &&
                   OrderId == other.OrderId &&
                   TotalAmount == other.TotalAmount &&
                   Status == other.Status &&
                   Items.Count == other.Items.Count &&
                   Items.SequenceEqual(other.Items);
        }

        /// <summary>
        /// GetHashCode: Hash code based on OrderId, TotalAmount, Status, and Items
        /// 1. Combine OrderId, TotalAmount, Status using HashCode.Combine
        /// 2. Add Items hash code
        /// 3. Return combined hash code
        /// </summary>
        /// <returns>Hash code for OrderState</returns>
        public override int GetHashCode()
        {
            var hash = HashCode.Combine(OrderId, TotalAmount, Status);
            foreach (var item in Items)
            {
                hash = HashCode.Combine(hash, item);
            }
            return hash;
        }
    }

    /// <summary>
    /// OrderStatus: Enumeration of order statuses
    /// - Represents the lifecycle states of an order
    /// - Updated by domain events
    /// </summary>
    public enum OrderStatus
    {
        Created,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    /// <summary>
    /// OrderItem: Represents an item in an order
    /// - Added by ItemAddedEvent
    /// - Removed by ItemRemovedEvent
    /// - Part of OrderState.Items collection
    /// </summary>
    public class OrderItem
    {
        public string ItemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is OrderItem other &&
                   ItemId == other.ItemId &&
                   Name == other.Name &&
                   Price == other.Price &&
                   Quantity == other.Quantity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemId, Name, Price, Quantity);
        }
    }
}

