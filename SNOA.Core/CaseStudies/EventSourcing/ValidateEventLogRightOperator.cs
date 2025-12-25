using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;

namespace SNOA.Core.CaseStudies.EventSourcing
{
    /// <summary>
    /// ValidateEventLogRightOperator: R3 operator for Event Sourcing case study
    /// Right operator because it manages state (validates consistency)
    /// - This is a Right Operator (R ∈ 𝓡)
    /// - Right operators handle normalization and state management
    /// - Follows formal semantics: R(X) = (V', P', σ') where:
    ///   - V' = R_V(V, P, σ) = V (unchanged)
    ///   - P' = R_P(V, P, σ) = P ∪ {validated: true, validationTime: now, validationResult: result}
    ///   - σ' = R_σ(V, P, σ) = σ (unchanged, or fixed if inconsistencies found)
    /// - Validates event log consistency
    /// - Checks for gaps in version sequence
    /// - Validates event ordering
    /// - Reports validation results
    /// - A1 (Closure): Returns SNOAObject<OrderState, EventLog> (same type)
    /// - A2 (Structural Stability): V, P, σ all remain present (structure preserved)
    /// - A4 (Property Mutability): Properties change (validation metadata added)
    /// </summary>
    public class ValidateEventLogRightOperator : IRightOperator<OrderState, EventLog>
    {
        /// <summary>
        /// Apply ValidateEventLog operator
        /// 1. Extract current event log (σ)
        /// 2. Validate event log consistency:
        ///    - Check for gaps in version sequence
        ///    - Validate event ordering (versions should be sequential)
        ///    - Check for duplicate versions
        /// 3. Update properties: validated = true, validationTime = now, validationResult = result
        /// 4. Return new SNOAObject with same V, updated P', same σ' (or fixed σ' if inconsistencies found)
        /// R(X) = (V', P', σ') where:
        /// - V' = V (unchanged)
        /// - P' = P ∪ {validated: true, validationTime: DateTime.UtcNow, validationResult: "PASS" | "FAIL", validationErrors: [...]}
        /// - σ' = σ (unchanged, or fixed if inconsistencies found)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=OrderState, σ=EventLog</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with validation metadata</returns>
        public SNOAObject<OrderState, EventLog> Apply(SNOAObject<OrderState, EventLog> obj)
        {
            // Validate event log consistency
            var validationResult = ValidateEventLog(obj.State);

            // Preserve state (V' = V)
            var newState = obj.Value;

            // Preserve or fix event log (σ')
            var newEventLog = validationResult.IsValid
                ? new EventLog
                {
                    Events = new List<DomainEvent>(obj.State.Events),
                    CurrentVersion = obj.State.CurrentVersion,
                    IsReplaying = obj.State.IsReplaying
                }
                : FixEventLog(obj.State, validationResult); // Fix inconsistencies if found

            // Update properties (P')
            var newProperties = new Dictionary<string, object>(obj.Properties)
            {
                ["validated"] = true,
                ["validationTime"] = DateTime.UtcNow,
                ["validationResult"] = validationResult.IsValid ? "PASS" : "FAIL",
                ["validationErrors"] = validationResult.Errors
            };

            return new SNOAObject<OrderState, EventLog>(newState, newProperties, newEventLog);
        }

        /// <summary>
        /// ValidateEventLog: Validate event log consistency
        /// 1. Check for gaps in version sequence
        /// 2. Validate event ordering (versions should be sequential)
        /// 3. Check for duplicate versions
        /// 4. Return validation result
        /// </summary>
        private ValidationResult ValidateEventLog(EventLog eventLog)
        {
            var errors = new List<string>();
            var events = eventLog.Events.OrderBy(e => e.Version).ToList();

            // Check for gaps in version sequence
            for (int i = 0; i < events.Count; i++)
            {
                var expectedVersion = i + 1;
                if (events[i].Version != expectedVersion)
                {
                    errors.Add($"Version gap at position {i}: expected {expectedVersion}, found {events[i].Version}");
                }
            }

            // Check for duplicate versions
            var duplicateVersions = events
                .GroupBy(e => e.Version)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateVersions.Any())
            {
                errors.Add($"Duplicate versions found: {string.Join(", ", duplicateVersions)}");
            }

            // Check version matches CurrentVersion
            if (events.Count > 0 && events.Last().Version != eventLog.CurrentVersion)
            {
                errors.Add($"Last event version ({events.Last().Version}) does not match CurrentVersion ({eventLog.CurrentVersion})");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// FixEventLog: Fix event log inconsistencies
        /// 1. Reorder events by version
        /// 2. Remove duplicates (keep first occurrence)
        /// 3. Update CurrentVersion to match last event version
        /// 4. Return fixed event log
        /// </summary>
        private EventLog FixEventLog(EventLog eventLog, ValidationResult validationResult)
        {
            // Reorder events by version and remove duplicates
            var fixedEvents = eventLog.Events
                .GroupBy(e => e.Version)
                .Select(g => g.First())
                .OrderBy(e => e.Version)
                .ToList();

            return new EventLog
            {
                Events = fixedEvents,
                CurrentVersion = fixedEvents.Count > 0 ? fixedEvents.Last().Version : 0,
                IsReplaying = eventLog.IsReplaying
            };
        }

        /// <summary>
        /// ValidationResult: Result of event log validation
        /// </summary>
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
}

