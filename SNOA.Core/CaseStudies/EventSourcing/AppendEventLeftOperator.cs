using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// AppendEventLeftOperator: L1 operator for Event Sourcing case study
    /// Left operator because it modifies event log structure (adds event, updates state)
    /// - This is a Left Operator (L ∈ 𝓛)
    /// - Left operators handle structural transformations
    /// - Follows formal semantics: L(X) = (V', P', σ') where:
    ///   - V' = L_V(V, P, σ) = ApplyEvent(V, event) (state updated by event)
    ///   - P' = L_P(V, P, σ) = P ∪ {version: σ.version + 1, lastEventTime: now, eventCount: P.eventCount + 1}
    ///   - σ' = L_σ(V, P, σ) = σ ∪ {event} (event appended to log)
    /// - Appends new domain event to event log
    /// - Updates state by applying event
    /// - Increments version and updates metadata
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (EventLog modified)
    /// - A4 (Property Mutability): Properties change (version, eventCount updated)
    /// - A5 (Noncommutativity): AppendEvent ∘ NormalizeEventLog ≠ NormalizeEventLog ∘ AppendEvent
    /// </summary>
    public class AppendEventLeftOperator : ILeftOperator<OrderState, EventLog>
    {
        private readonly DomainEvent _event;

        /// <summary>
        /// Constructor for AppendEventLeftOperator
        /// - domainEvent: The domain event to append to the event log
        /// </summary>
        /// <param name="domainEvent">Domain event to append</param>
        public AppendEventLeftOperator(DomainEvent domainEvent)
        {
            _event = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
        }

        /// <summary>
        /// Apply AppendEvent operator
        /// 1. Extract current order state (V), properties (P), and event log (σ)
        /// 2. Apply event to state (V' = ApplyEvent(V, event))
        /// 3. Append event to event log (σ' = σ ∪ {event})
        /// 4. Update properties: version = σ.version + 1, lastEventTime = now, eventCount = P.eventCount + 1
        /// 5. Return new SNOAObject with updated V', updated P', updated σ'
        /// L(X) = (V', P', σ') where:
        /// - V' = ApplyEvent(V, event) (state updated by event)
        /// - P' = P ∪ {version: σ.version + 1, lastEventTime: DateTime.Now, eventCount: P.eventCount + 1, lastEventType: event.GetType().Name}
        /// - σ' = (Events ∪ {event}, CurrentVersion + 1, IsReplaying)
        /// - OrderCreatedEvent: Initialize order state
        /// - ItemAddedEvent: Add item to order, update total amount
        /// - ItemRemovedEvent: Remove item from order, update total amount
        /// - OrderShippedEvent: Update order status to Shipped
        /// - OrderCancelledEvent: Update order status to Cancelled
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with event appended</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Apply event to state (V')
            var newState = ApplyEventToState(obj.Value, _event);

            // Update event log (σ')
            var newEventLog = new EventLog
            {
                Events = new List<DomainEvent>(obj.State.Events) { _event },
                CurrentVersion = obj.State.CurrentVersion + 1,
                IsReplaying = obj.State.IsReplaying
            };

            // Set event version
            _event.Version = newEventLog.CurrentVersion;

            // Update metadata (P')
            var eventCount = obj.Properties.ContainsKey("eventCount") 
                ? (int)obj.Properties["eventCount"] 
                : obj.State.Events.Count;

            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["version"] = newEventLog.CurrentVersion,
                ["lastEventTime"] = DateTime.UtcNow,
                ["eventCount"] = eventCount + 1,
                ["lastEventType"] = _event.GetType().Name
            };

            return new SNOAObject<OrderState, EventLog>(newState, newProperties, newEventLog);
        }

        /// <summary>
        /// ApplyEventToState: Apply domain event to order state
        /// 1. Switch on event type
        /// 2. For OrderCreatedEvent: Initialize order state
        /// 3. For ItemAddedEvent: Add item to order, update total amount
        /// 4. For ItemRemovedEvent: Remove item from order, update total amount
        /// 5. For OrderShippedEvent: Update order status to Shipped
        /// 6. For OrderCancelledEvent: Update order status to Cancelled
        /// 7. Return new state
        /// - Events are facts that happened
        /// - State is derived by replaying events
        /// - Each event creates a new state (immutable)
        /// </summary>
        /// <param name="state">Current order state</param>
        /// <param name="evt">Domain event to apply</param>
        /// <returns>New order state after applying event</returns>
        private OrderState ApplyEventToState(OrderState state, DomainEvent evt)
        {
            return evt switch
            {
                OrderCreatedEvent e => new OrderState(e.OrderId)
                {
                    Status = OrderStatus.Created
                },
                ItemAddedEvent e => new OrderState(state.OrderId)
                {
                    Status = state.Status,
                    Items = new List<OrderItem>(state.Items)
                    {
                        new OrderItem
                        {
                            ItemId = e.ItemId,
                            Name = e.Name,
                            Price = e.Price,
                            Quantity = e.Quantity
                        }
                    },
                    TotalAmount = state.TotalAmount + (e.Price * e.Quantity)
                },
                ItemRemovedEvent e => new OrderState(state.OrderId)
                {
                    Status = state.Status,
                    Items = state.Items.Where(item => item.ItemId != e.ItemId).ToList(),
                    TotalAmount = state.Items
                        .Where(item => item.ItemId != e.ItemId)
                        .Sum(item => item.Price * item.Quantity)
                },
                OrderShippedEvent e => new OrderState(state.OrderId)
                {
                    Status = OrderStatus.Shipped,
                    Items = new List<OrderItem>(state.Items),
                    TotalAmount = state.TotalAmount
                },
                OrderCancelledEvent e => new OrderState(state.OrderId)
                {
                    Status = OrderStatus.Cancelled,
                    Items = new List<OrderItem>(state.Items),
                    TotalAmount = state.TotalAmount
                },
                _ => state
            };
        }
    }
}

