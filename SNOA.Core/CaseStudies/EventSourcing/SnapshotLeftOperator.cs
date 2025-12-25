using System;
using System.Collections.Generic;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// SnapshotLeftOperator: L3 operator for Event Sourcing case study
    /// Left operator because it captures state structure (snapshot)
    /// - This is a Left Operator (L ∈ 𝓛)
    /// - Left operators handle structural transformations
    /// - Follows formal semantics: L(X) = (V', P', σ') where:
    ///   - V' = L_V(V, P, σ) = V (current state preserved)
    ///   - P' = L_P(V, P, σ) = P ∪ {snapshotVersion: σ.version, snapshotTime: now}
    ///   - σ' = L_σ(V, P, σ) = σ (unchanged)
    /// - Creates a snapshot of current state at current version
    /// - Used for performance optimization (avoid replaying all events)
    /// - Snapshot metadata stored in properties
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A4 (Property Mutability): Properties change (snapshot metadata added)
    /// </summary>
    public class SnapshotLeftOperator : ILeftOperator<OrderState, EventLog>
    {
        /// <summary>
        /// Apply Snapshot operator
        /// 1. Extract current order state (V), properties (P), and event log (σ)
        /// 2. Preserve current state (V' = V)
        /// 3. Preserve event log (σ' = σ)
        /// 4. Update properties: snapshotVersion = σ.version, snapshotTime = now
        /// 5. Return new SNOAObject with same V', updated P', same σ'
        /// L(X) = (V', P', σ') where:
        /// - V' = V (current state preserved)
        /// - P' = P ∪ {snapshotVersion: σ.CurrentVersion, snapshotTime: DateTime.UtcNow}
        /// - σ' = σ (unchanged)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with snapshot metadata</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Preserve state (V' = V)
            var newState = obj.Value;

            // Preserve event log (σ' = σ)
            var newEventLog = new EventLog
            {
                Events = new List<DomainEvent>(obj.State.Events),
                CurrentVersion = obj.State.CurrentVersion,
                IsReplaying = obj.State.IsReplaying
            };

            // Update properties (P')
            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["snapshotVersion"] = obj.State.CurrentVersion,
                ["snapshotTime"] = DateTime.UtcNow,
                ["hasSnapshot"] = true
            };

            return new SNOAObject<OrderState, EventLog>(newState, newProperties, newEventLog);
        }
    }
}

