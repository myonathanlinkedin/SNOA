namespace SNOA.Core
{
    /// <summary>
    /// Left Operator Interface: L ∈ 𝓛, L : X → X
    /// - Structural transformations
    /// - Level updates
    /// - Event application
    /// L(X) = (V', P', σ') where:
    /// - V' = L_V(V, P, σ)  [value transformation function]
    /// - P' = L_P(V, P, σ)  [property update function]
    /// - σ' = L_σ(V, P, σ)  [state mutation function]
    /// </summary>
    /// <typeparam name="TValue">Type_V - main value type</typeparam>
    /// <typeparam name="TState">Type_σ - internal state type</typeparam>
    public interface ILeftOperator<TValue, TState>
    {
        /// <summary>
        /// Apply left operator to object: L(X) = (V', P', σ')
        /// 1. Extract V, P, σ from input object X
        /// 2. Apply left operator transformation functions:
        ///    - V' = L_V(V, P, σ) [value transformation]
        ///    - P' = L_P(V, P, σ) [property update]
        ///    - σ' = L_σ(V, P, σ) [state mutation]
        /// 3. Return new SNOAObject with transformed components
        /// L(X) = (V', P', σ') where:
        /// - V' = L_V(V, P, σ)  [value transformation function]
        /// - P' = L_P(V, P, σ)  [property update function]
        /// - σ' = L_σ(V, P, σ)  [state mutation function]
        /// - A1 (Closure): L(X) ∈ 𝕏 (returns SNOAObject of same type)
        /// - A2 (Structural Stability): Returns object with same structure (V, P, σ)
        /// - A3 (State Mutability): State may change (σ' = L_σ(V, P, σ))
        /// - A4 (Property Mutability): Properties may change (P' = L_P(V, P, σ))
        /// - Structural transformations (e.g., AddEdge, RemoveEdge, SplitNode)
        /// - Level updates (e.g., BFS level increment)
        /// - Event application (e.g., apply event to state)
        /// </summary>
        /// <param name="obj">Input object X = (V, P, σ)</param>
        /// <returns>Result object X' = (V', P', σ') where X' ∈ 𝕏</returns>
        SNOAObject<TValue, TState> Apply(SNOAObject<TValue, TState> obj);
    }
}



