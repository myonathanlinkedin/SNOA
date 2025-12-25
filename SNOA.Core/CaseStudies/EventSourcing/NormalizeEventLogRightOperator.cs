using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// NormalizeEventLogRightOperator: R1 operator for Event Sourcing case study
    /// Right operator because it normalizes event log (removes duplicates, sorts, compresses)
    /// - This is a Right Operator (R ∈ 𝓡)
    /// - Right operators handle normalization and state management
    /// - Follows formal semantics: R(X) = (V', P', σ') where:
    ///   - V' = R_V(V, P, σ) = V (unchanged)
    ///   - P' = R_P(V, P, σ) = P ∪ {normalized: true, normalizationTime: now}
    ///   - σ' = R_σ(V, P, σ) = Normalize(σ) (remove duplicates, sort, compress)
    /// - Normalizes event log by removing duplicates
    /// - Sorts events by version
    /// - Updates event count after normalization
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A3 (State Mutability): State changes (EventLog normalized)
    /// - A4 (Property Mutability): Properties change (normalized flag added)
    /// </summary>
    public class NormalizeEventLogRightOperator : IRightOperator<OrderState, EventLog>
    {
        /// <summary>
        /// Apply NormalizeEventLog operator
        /// 1. Extract current event log (σ)
        /// 2. Remove duplicate events (by EventId)
        /// 3. Sort events by version
        /// 4. Create normalized event log (σ' = Normalize(σ))
        /// 5. Update properties: normalized = true, normalizationTime = now, eventCount = normalized count
        /// 6. Return new SNOAObject with same V, updated P', normalized σ'
        /// R(X) = (V', P', σ') where:
        /// - V' = V (unchanged)
        /// - P' = P ∪ {normalized: true, normalizationTime: DateTime.UtcNow, eventCount: |normalizedEvents|}
        /// - σ' = (normalizedEvents, |normalizedEvents|, IsReplaying = false)
        ///   where normalizedEvents = Distinct(Events) ordered by Version
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with normalized event log</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Normalize event log (remove duplicates, sort by version)
            var normalizedEvents = obj.State.Events
                .GroupBy(e => e.EventId)
                .Select(g => g.First())
                .OrderBy(e => e.Version)
                .ToList();

            // Create normalized event log (σ')
            var newEventLog = new EventLog
            {
                Events = normalizedEvents,
                CurrentVersion = normalizedEvents.Count,
                IsReplaying = false
            };

            // Update properties (P')
            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["normalized"] = true,
                ["normalizationTime"] = DateTime.UtcNow,
                ["eventCount"] = normalizedEvents.Count,
                ["removedDuplicates"] = obj.State.Events.Count - normalizedEvents.Count
            };

            return new SNOAObject<OrderState, EventLog>(obj.Value, newProperties, newEventLog);
        }
    }
}

