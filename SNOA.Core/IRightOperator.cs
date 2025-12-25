namespace SNOA.Core
{
    /// <summary>
    /// Right Operator Interface: R ∈ 𝓡, R : X → X
    /// - Normalization
    /// - State commit
    /// - Neighbor updates
    /// R(X) = (V', P', σ') where:
    /// - V' = R_V(V, P, σ)  [value transformation function]
    /// - P' = R_P(V, P, σ)  [property update function]
    /// - σ' = R_σ(V, P, σ)  [state mutation function]
    /// </summary>
    /// <typeparam name="TValue">Type_V - main value type</typeparam>
    /// <typeparam name="TState">Type_σ - internal state type</typeparam>
    public interface IRightOperator<TValue, TState>
    {
        /// <summary>
        /// Apply right operator to object: R(X) = (V', P', σ')
        /// 1. Extract V, P, σ from input object X
        /// 2. Apply right operator transformation functions:
        ///    - V' = R_V(V, P, σ) [value transformation]
        ///    - P' = R_P(V, P, σ) [property update]
        ///    - σ' = R_σ(V, P, σ) [state mutation]
        /// 3. Return new SNOAObject with transformed components
        /// R(X) = (V', P', σ') where:
        /// - V' = R_V(V, P, σ)  [value transformation function]
        /// - P' = R_P(V, P, σ)  [property update function]
        /// - σ' = R_σ(V, P, σ)  [state mutation function]
        /// - A1 (Closure): R(X) ∈ 𝕏 (returns SNOAObject of same type)
        /// - A2 (Structural Stability): Returns object with same structure (V, P, σ)
        /// - A3 (State Mutability): State may change (σ' = R_σ(V, P, σ))
        /// - A4 (Property Mutability): Properties may change (P' = R_P(V, P, σ))
        /// - Normalization (e.g., NormalizeGraph - remove duplicates, sort)
        /// - State commit (e.g., CommitState - commit state changes)
        /// - Neighbor updates (e.g., UpdateNeighbors - synchronize with graph state)
        /// </summary>
        /// <param name="obj">Input object X = (V, P, σ)</param>
        /// <returns>Result object X' = (V', P', σ') where X' ∈ 𝕏</returns>
        SNOAObject<TValue, TState> Apply(SNOAObject<TValue, TState> obj);
    }
}



