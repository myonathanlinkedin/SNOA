using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// ReplayEventsLeftOperator: L2 operator for Event Sourcing case study
    /// Left operator because it modifies state structure (rebuilds state from events)
    /// - This is a Left Operator (L ∈ 𝓛)
    /// - Left operators handle structural transformations
    /// - Follows formal semantics: L(X) = (V', P', σ') where:
    ///   - V' = L_V(V, P, σ) = ReplayAllEvents(σ.events) (state rebuilt from events)
    ///   - P' = L_P(V, P, σ) = P ∪ {replayed: true, replayTime: now}
    ///   - σ' = L_σ(V, P, σ) = σ with IsReplaying = true
    /// - Replays all events from event log to rebuild state
    /// - Used for state projection and recovery
    /// - Sets IsReplaying flag to prevent side effects during replay
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (OrderState rebuilt)
    /// - A4 (Property Mutability): Properties change (replayed flag added)
    /// </summary>
    public class ReplayEventsLeftOperator : ILeftOperator<OrderState, EventLog>
    {
        /// <summary>
        /// Apply ReplayEvents operator
        /// 1. Extract current event log (σ)
        /// 2. Set IsReplaying flag to true
        /// 3. Replay all events to rebuild state (V' = ReplayAllEvents(σ.events))
        /// 4. Update properties: replayed = true, replayTime = now
        /// 5. Return new SNOAObject with rebuilt V', updated P', updated σ'
        /// L(X) = (V', P', σ') where:
        /// - V' = ReplayAllEvents(σ.events) (state rebuilt from events)
        /// - P' = P ∪ {replayed: true, replayTime: DateTime.UtcNow}
        /// - σ' = (Events, CurrentVersion, IsReplaying = true)
        /// - Start with empty order state
        /// - Apply each event in sequence
        /// - Final state is the result of replaying all events
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with state rebuilt from events</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Set IsReplaying flag
            var newEventLog = new EventLog
            {
                Events = new List<DomainEvent>(obj.State.Events),
                CurrentVersion = obj.State.CurrentVersion,
                IsReplaying = true
            };

            // Replay all events to rebuild state (V')
            var replayedState = ReplayAllEvents(obj.State.Events);

            // Update properties (P')
            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["replayed"] = true,
                ["replayTime"] = DateTime.UtcNow,
                ["replayedEventCount"] = obj.State.Events.Count
            };

            return new SNOAObject<OrderState, EventLog>(replayedState, newProperties, newEventLog);
        }

        /// <summary>
        /// ReplayAllEvents: Replay all events to rebuild state
        /// 1. Start with empty order state (or initial state from OrderCreatedEvent)
        /// 2. Iterate through all events in sequence
        /// 3. Apply each event to current state
        /// 4. Return final state
        /// - State is derived by replaying events
        /// - Events are applied in sequence
        /// - Each event transforms the state
        /// </summary>
        /// <param name="events">List of domain events to replay</param>
        /// <returns>Order state after replaying all events</returns>
        private OrderState ReplayAllEvents(List<DomainEvent> events)
        {
            OrderState? state = null;

            foreach (var evt in events)
            {
                if (evt is OrderCreatedEvent createdEvent)
                {
                    state = new OrderState(createdEvent.OrderId)
                    {
                        Status = OrderStatus.Created
                    };
                }
                else if (state != null)
                {
                    state = ApplyEventToState(state, evt);
                }
            }

            // If no OrderCreatedEvent found, return empty state
            return state ?? new OrderState(string.Empty);
        }

        /// <summary>
        /// ApplyEventToState: Apply domain event to order state (same logic as AppendEventLeftOperator)
        /// </summary>
        private OrderState ApplyEventToState(OrderState state, DomainEvent evt)
        {
            return evt switch
            {
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

