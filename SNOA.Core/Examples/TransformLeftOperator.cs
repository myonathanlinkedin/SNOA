using System.Collections.Generic;

namespace SNOA.Core.Examples
{
    /// <summary>
    /// Example Left Operator: Transform
    /// - Transforms value based on current state
    /// - Updates properties
    /// - Mutates state
    /// </summary>
    public class TransformLeftOperator : ILeftOperator<int, int>
    {
        /// <summary>
        /// Apply transformation: L(X) = (V', P', σ')
        /// 1. Extract V, P, σ from input object X
        /// 2. Transform value: V' = V * σ (multiply value by state)
        /// 3. Update properties: P' = P ∪ {transformed: true, transformation_count: count+1}
        /// 4. Mutate state: σ' = σ + 1 (increment state)
        /// 5. Return new SNOAObject with transformed components
        /// - V' = L_V(V, P, σ) = V * σ (value transformation)
        /// - P' = L_P(V, P, σ) = P ∪ {transformed: true, transformation_count: count+1}
        /// - σ' = L_σ(V, P, σ) = σ + 1 (state mutation)
        /// - A1 (Closure): Returns SNOAObject<int, int> (same type)
        /// - A3 (State Mutability): State changes (σ' = σ + 1)
        /// - A4 (Property Mutability): Properties change (transformation metadata added)
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ) where V=int, σ=int</param>
        /// <returns>Result SNOA object X' = (V', P', σ') with transformed components</returns>
        public SNOAObject<int, int> Apply(SNOAObject<int, int> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: main value - will be transformed
            var value = obj.Value;
            
            // P: properties - will be updated with transformation metadata
            var properties = obj.Properties;
            
            // σ: internal state - will be mutated

            // Transform value: V' = V * σ
            // Algorithm: Multiply value by current state
            // This demonstrates value transformation based on state
            var newValue = value * obj.State;

            // Update properties: P' = P ∪ {transformed: true, transformation_count: count+1}
            // Algorithm: Add transformation flag and increment counter
            // This tracks how many times transformation has been applied
            var newProperties = new Dictionary<string, object>(properties)
            {
                ["transformed"] = true,
                ["transformation_count"] = ((int)(properties.GetValueOrDefault("transformation_count", 0))) + 1
            };

            // Mutate state: σ' = σ + 1
            // Algorithm: Increment state by 1
            // This demonstrates state mutability (Axiom A3)
            var newState = obj.State + 1;

            // Return new SNOAObject X' = (V', P', σ')
            // V' = transformed value, P' = updated properties, σ' = mutated state
            // This satisfies Axiom A1 (Closure): returns SNOAObject<int, int>
            return new SNOAObject<int, int>(newValue, newProperties, newState);
        }
    }
}



