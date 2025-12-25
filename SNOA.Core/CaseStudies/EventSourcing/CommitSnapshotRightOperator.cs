using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// CommitSnapshotRightOperator: R2 operator for Event Sourcing case study
    /// Right operator because it manages state (commits snapshot, clears old events)
    /// - This is a Right Operator (R ∈ 𝓡)
    /// - Right operators handle normalization and state management
    /// - Follows formal semantics: R(X) = (V', P', σ') where:
    ///   - V' = R_V(V, P, σ) = V (current state preserved)
    ///   - P' = R_P(V, P, σ) = P ∪ {committed: true, commitVersion: σ.version}
    ///   - σ' = R_σ(V, P, σ) = ClearOldEvents(σ) (keep only recent events)
    /// - Commits current state as snapshot
    /// - Clears old events (keeps only recent events for performance)
    /// - Updates commit metadata
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (EventLog cleared)
    /// - A4 (Property Mutability): Properties change (committed flag added)
    /// </summary>
    public class CommitSnapshotRightOperator : IRightOperator<OrderState, EventLog>
    {
        private readonly int _keepRecentEvents;

        /// <summary>
        /// Constructor for CommitSnapshotRightOperator
        /// - keepRecentEvents: Number of recent events to keep (default: 10)
        /// </summary>
        /// <param name="keepRecentEvents">Number of recent events to keep (default: 10)</param>
        public CommitSnapshotRightOperator(int keepRecentEvents = 10)
        {
            _keepRecentEvents = keepRecentEvents;
        }

        /// <summary>
        /// Apply CommitSnapshot operator
        /// 1. Extract current order state (V), properties (P), and event log (σ)
        /// 2. Preserve current state (V' = V)
        /// 3. Clear old events, keep only recent events (σ' = ClearOldEvents(σ))
        /// 4. Update properties: committed = true, commitVersion = σ.version
        /// 5. Return new SNOAObject with same V', updated P', cleared σ'
        /// R(X) = (V', P', σ') where:
        /// - V' = V (current state preserved)
        /// - P' = P ∪ {committed: true, commitVersion: σ.CurrentVersion, commitTime: DateTime.UtcNow}
        /// - σ' = (recentEvents, |recentEvents|, IsReplaying = false)
        ///   where recentEvents = Last N events from Events (N = keepRecentEvents)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with snapshot committed</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Preserve state (V' = V)
            var newState = obj.Value;

            // Clear old events, keep only recent events (σ')
            var recentEvents = obj.State.Events
                .OrderByDescending(e => e.Version)
                .Take(_keepRecentEvents)
                .OrderBy(e => e.Version)
                .ToList();

            var newEventLog = new EventLog
            {
                Events = recentEvents,
                CurrentVersion = obj.State.CurrentVersion, // Keep original version
                IsReplaying = false
            };

            // Update properties (P')
            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["committed"] = true,
                ["commitVersion"] = obj.State.CurrentVersion,
                ["commitTime"] = DateTime.UtcNow,
                ["keptEvents"] = recentEvents.Count,
                ["clearedEvents"] = obj.State.Events.Count - recentEvents.Count
            };

            return new SNOAObject<OrderState, EventLog>(newState, newProperties, newEventLog);
        }
    }
}

