using System.Collections.Generic;

namespace SNOA.Core.Examples
{
    /// <summary>
    /// Example Right Operator: Normalize
    /// - Normalizes value
    /// - Updates properties
    /// - Commits/resets state
    /// </summary>
    public class NormalizeRightOperator : IRightOperator<int, int>
    {
        /// <summary>
        /// Apply normalization: R(X) = (V', P', σ')
        /// 1. Extract V, P, σ from input object X
        /// 2. Normalize value: V' = V % 100 (modulo operation to keep value in range 0-99)
        /// 3. Update properties: P' = P ∪ {normalized: true}
        /// 4. Reset state: σ' = 0 (clear state after normalization)
        /// 5. Return new SNOAObject with normalized components
        /// - V' = R_V(V, P, σ) = V % 100 (value normalization)
        /// - P' = R_P(V, P, σ) = P ∪ {normalized: true}
        /// - σ' = R_σ(V, P, σ) = 0 (state reset after normalization)
        /// - A1 (Closure): Returns SNOAObject<int, int> (same type)
        /// - A3 (State Mutability): State changes (σ' = 0, reset)
        /// - A4 (Property Mutability): Properties change (normalization flag added)
        /// - Normalization ensures value is in valid range (0-99)
        /// - State reset clears transient state after normalization
        /// - This is typical for Right Operators: normalize then reset
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=int, σ=int</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with normalized components</returns>
        public SNOAObject<int, int> Apply(SNOAObject<int, int> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: main value - will be normalized
            var value = obj.Value;
            
            // P: properties - will be updated with normalization flag
            var properties = obj.Properties;
            
            // σ: internal state - will be reset

            // Normalize value: V' = V % 100
            // Algorithm: Modulo operation to keep value in range 0-99
            // This ensures value is within valid normalized range
            var normalizedValue = value % 100;

            // Update properties: P' = P ∪ {normalized: true}
            // Algorithm: Add normalization flag
            // This tracks that normalization has been applied
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["normalized"] = true
            };

            // Reset state: σ' = 0
            // Algorithm: Clear state after normalization
            // This follows Right Operator pattern: normalize then reset transient state
            // State reset is common in normalization operations
            var newState = 0;

            // Return new SNOAObject X' = (V', P', σ')
            // V' = normalized value, P' = updated properties, σ' = reset state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<int, int>
            return new SNOAObject<int, int>(normalizedValue, newProperties, newState);
        }
    }
}



