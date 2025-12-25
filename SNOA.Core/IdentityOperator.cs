namespace SNOA.Core
{
    /// <summary>
    /// Identity Operator: I(X) = X
    /// - Axiom A8: There exists an identity operator I such that:
    ///   - I(X) = X for all X ∈ 𝕏
    ///   - I ∘ f = f ∘ I = f for all operators f
    /// - Identity operator is fundamental in category theory (identity morphism)
    /// - In monoidal categories, identity is required for monoid structure
    /// - Identity preserves all structure: I(X) = (V, P, σ) unchanged
    /// - This operator implements both ILeftOperator and IRightOperator
    ///   since identity should work for both operator types (𝓛 and 𝓡)
    /// - Identity is neutral element for composition (Axiom A6, A7)
    /// - Identity satisfies: I ∘ L = L ∘ I = L for all L ∈ 𝓛
    /// - Identity satisfies: I ∘ R = R ∘ I = R for all R ∈ 𝓡
    /// - Uses immutable pattern: creates new instance with same values
        /// - Singleton pattern for efficiency (same identity operator can be reused)
    /// - A1 (Closure): I(X) ∈ 𝕏 (returns SNOAObject of same type)
    /// - A2 (Structural Stability): I(X) preserves V, P, σ (all unchanged)
    /// - A6 (Composition): I ∘ f = f ∘ I = f (identity is neutral for composition)
    /// - A8 (Identity): This operator directly implements Axiom A8
    /// </summary>
    /// <typeparam name="TValue">Type_V - main value type</typeparam>
    /// <typeparam name="TState">Type_σ - internal state type</typeparam>
    public class IdentityOperator<TValue, TState> : ILeftOperator<TValue, TState>, IRightOperator<TValue, TState>
    {
        /// <summary>
        /// Apply identity operator: I(X) = X
        /// 1. Extract V, P, σ from input object X
        /// 2. Create new SNOAObject with same V, P, σ (immutable pattern)
        /// 3. Return new object X' where X' = X (value-wise equality)
        /// I(X) = (V', P', σ') where:
        /// - V' = V (unchanged)
        /// - P' = P (unchanged, new dictionary with same entries)
        /// - σ' = σ (unchanged)
        /// - I(X) = X (by definition of identity)
        /// - I preserves all structure: no modifications to V, P, or σ
        /// - Creates new instance to avoid side effects
        /// - Ensures I(X) returns new object (not reference to same object)
        /// - Maintains value equality: I(X).Equals(X) = true
        /// </summary>
        /// <param name="obj">Input SNOA object X = (V, P, σ)</param>
        /// <returns>Result SNOA object X' = (V, P, σ) where X' = X (value-wise)</returns>
        public SNOAObject<TValue, TState> Apply(SNOAObject<TValue, TState> obj)
        {
            // Extract components from input object X = (V, P, σ)
            // V: main value - remains unchanged
            var value = obj.Value;
            
            // P: properties - create new dictionary with same entries (immutable pattern)
            // Algorithm: Copy all key-value pairs to new dictionary
            // This ensures I(X) returns new object, not reference to same object
            var newProperties = new Dictionary<string, object>(obj.Properties);
            
            // σ: internal state - remains unchanged
            var state = obj.State;

            // Return new SNOAObject X' = (V', P', σ') where V'=V, P'=P, σ'=σ
            // This satisfies Axiom A8: I(X) = X (value-wise equality)
            // Note: New instance created (immutable pattern), but values are identical
            return new SNOAObject<TValue, TState>(value, newProperties, state);
        }

        /// <summary>
        /// Singleton instance for efficiency
        /// - Identity operator is stateless (no parameters needed)
        /// - Same identity operator can be reused for all objects of type (TValue, TState)
        /// - Singleton pattern avoids creating multiple instances of identical operator
        /// - IdentityOperator<TValue, TState>.Instance.Apply(obj)
        /// - Can be used in composition: identity.Compose(otherOperator, obj)
        /// </summary>
        public static IdentityOperator<TValue, TState> Instance { get; } = new IdentityOperator<TValue, TState>();
    }
}

