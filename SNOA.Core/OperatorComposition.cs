namespace SNOA.Core
{
    /// <summary>
    /// Operator Composition Helper
    /// - Axiom A6 (Composition): (f ∘ g)(X) = f(g(X))
    ///   - Composition is function composition: apply g first, then f
    ///   - Composition is fundamental in category theory (morphism composition)
    /// - Axiom A7 (Conditional Associativity): 
    ///   For operators f, g, h where state mutations are independent:
    ///   ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X)
    ///   - Associativity holds when intermediate states don't affect subsequent operations
    ///   - If operations depend on intermediate states, associativity may not hold
    /// - Composition is fundamental operation in categories
    /// - In monoidal categories, composition must satisfy associativity (when applicable)
    /// - Composition preserves structure: if f, g preserve type, then f ∘ g preserves type
    /// - Left operators (𝓛) can be composed: L1 ∘ L2 ∈ 𝓛
    /// - Right operators (𝓡) can be composed: R1 ∘ R2 ∈ 𝓡
    /// - Mixed composition: L ∘ R and R ∘ L are valid (demonstrates noncommutativity A5)
    /// - Composition order matters: (L ∘ R)(X) ≠ (R ∘ L)(X) in general
    /// - L ∘ R ≠ R ∘ L in general
    /// - This is fundamental to SNOA: order of operations matters
    /// - Example: AddEdge ∘ NormalizeGraph ≠ NormalizeGraph ∘ AddEdge
    /// - Extension methods for fluent composition syntax
    /// - Enables: operator1.Compose(operator2, obj)
    /// - Type-safe: composition preserves generic types (TValue, TState)
    /// </summary>
    public static class OperatorComposition
    {
        /// <summary>
        /// Compose two left operators: (f ∘ g)(X) = f(g(X))
        /// 1. Apply g to X: g(X) = X1
        /// 2. Apply f to X1: f(X1) = f(g(X))
        /// 3. Return result: (f ∘ g)(X) = f(g(X))
        /// (f ∘ g)(X) = f(g(X)) where:
        /// - g: ILeftOperator<TValue, TState> (first operator)
        /// - f: ILeftOperator<TValue, TState> (second operator)
        /// - X: SNOAObject<TValue, TState> (input object)
        /// - Result: SNOAObject<TValue, TState> (composed result)
        /// - Closure: (f ∘ g)(X) ∈ 𝕏 (Axiom A1)
        /// - Type preservation: composition preserves (TValue, TState) types
        /// - Order matters: f ∘ g ≠ g ∘ f in general
        /// var composed = addEdge.Compose(removeEdge, graphNode);
        /// </summary>
        /// <typeparam name="TValue">Type_V - main value type</typeparam>
        /// <typeparam name="TState">Type_σ - internal state type</typeparam>
        /// <param name="f">Second operator (applied after g)</param>
        /// <param name="g">First operator (applied first)</param>
        /// <param name="x">Input SNOA object X = (V, P, σ)</param>
        /// <returns>Result SNOA object (f ∘ g)(X) = f(g(X))</returns>
        public static SNOAObject<TValue, TState> Compose<TValue, TState>(
            this ILeftOperator<TValue, TState> f,
            ILeftOperator<TValue, TState> g,
            SNOAObject<TValue, TState> x)
        {
            // Algorithm: (f ∘ g)(X) = f(g(X))
            // Step 1: Apply g to X: g(X) = X1
            var intermediate = g.Apply(x);
            
            // Step 2: Apply f to X1: f(X1) = f(g(X))
            var result = f.Apply(intermediate);
            
            // Step 3: Return (f ∘ g)(X) = f(g(X))
            // This satisfies Axiom A6: composition is function composition
            return result;
        }

        /// <summary>
        /// Compose two right operators: (f ∘ g)(X) = f(g(X))
        /// 1. Apply g to X: g(X) = X1
        /// 2. Apply f to X1: f(X1) = f(g(X))
        /// 3. Return result: (f ∘ g)(X) = f(g(X))
        /// (f ∘ g)(X) = f(g(X)) where:
        /// - g: IRightOperator<TValue, TState> (first operator)
        /// - f: IRightOperator<TValue, TState> (second operator)
        /// - X: SNOAObject<TValue, TState> (input object)
        /// - Result: SNOAObject<TValue, TState> (composed result)
        /// - Closure: (f ∘ g)(X) ∈ 𝕏 (Axiom A1)
        /// - Type preservation: composition preserves (TValue, TState) types
        /// - Order matters: f ∘ g ≠ g ∘ f in general
        /// var composed = normalize.Compose(updateNeighbors, graphNode);
        /// </summary>
        /// <typeparam name="TValue">Type_V - main value type</typeparam>
        /// <typeparam name="TState">Type_σ - internal state type</typeparam>
        /// <param name="f">Second operator (applied after g)</param>
        /// <param name="g">First operator (applied first)</param>
        /// <param name="x">Input SNOA object X = (V, P, σ)</param>
        /// <returns>Result SNOA object (f ∘ g)(X) = f(g(X))</returns>
        public static SNOAObject<TValue, TState> Compose<TValue, TState>(
            this IRightOperator<TValue, TState> f,
            IRightOperator<TValue, TState> g,
            SNOAObject<TValue, TState> x)
        {
            // Algorithm: (f ∘ g)(X) = f(g(X))
            // Step 1: Apply g to X: g(X) = X1
            var intermediate = g.Apply(x);
            
            // Step 2: Apply f to X1: f(X1) = f(g(X))
            var result = f.Apply(intermediate);
            
            // Step 3: Return (f ∘ g)(X) = f(g(X))
            // This satisfies Axiom A6: composition is function composition
            return result;
        }

        /// <summary>
        /// Compose left and right operator: (L ∘ R)(X) = L(R(X))
        /// 1. Apply R to X: R(X) = X1
        /// 2. Apply L to X1: L(X1) = L(R(X))
        /// 3. Return result: (L ∘ R)(X) = L(R(X))
        /// (L ∘ R)(X) = L(R(X)) where:
        /// - R: IRightOperator<TValue, TState> (first operator, normalization/state management)
        /// - L: ILeftOperator<TValue, TState> (second operator, structural transformation)
        /// - X: SNOAObject<TValue, TState> (input object)
        /// - Result: SNOAObject<TValue, TState> (composed result)
        /// - This demonstrates noncommutativity: (L ∘ R)(X) ≠ (R ∘ L)(X) in general
        /// - Order matters: normalize then add edge ≠ add edge then normalize
        /// - Example: NormalizeGraph ∘ AddEdge ≠ AddEdge ∘ NormalizeGraph
        /// - Closure: (L ∘ R)(X) ∈ 𝕏 (Axiom A1)
        /// - Type preservation: composition preserves (TValue, TState) types
        /// - Noncommutativity: L ∘ R ≠ R ∘ L (Axiom A5)
        /// var result = addEdge.Compose(normalizeGraph, graphNode);
        /// </summary>
        /// <typeparam name="TValue">Type_V - main value type</typeparam>
        /// <typeparam name="TState">Type_σ - internal state type</typeparam>
        /// <param name="left">Left operator (applied after right, structural transformation)</param>
        /// <param name="right">Right operator (applied first, normalization/state management)</param>
        /// <param name="x">Input SNOA object X = (V, P, σ)</param>
        /// <returns>Result SNOA object (L ∘ R)(X) = L(R(X))</returns>
        public static SNOAObject<TValue, TState> Compose<TValue, TState>(
            this ILeftOperator<TValue, TState> left,
            IRightOperator<TValue, TState> right,
            SNOAObject<TValue, TState> x)
        {
            // Algorithm: (L ∘ R)(X) = L(R(X))
            // Step 1: Apply R to X: R(X) = X1 (normalization/state management first)
            var normalized = right.Apply(x);
            
            // Step 2: Apply L to X1: L(X1) = L(R(X)) (structural transformation after)
            var result = left.Apply(normalized);
            
            // Step 3: Return (L ∘ R)(X) = L(R(X))
            // This satisfies Axiom A6: composition is function composition
            // This demonstrates Axiom A5: (L ∘ R)(X) ≠ (R ∘ L)(X) in general
            return result;
        }

        /// <summary>
        /// Compose right and left operator: (R ∘ L)(X) = R(L(X))
        /// 1. Apply L to X: L(X) = X1
        /// 2. Apply R to X1: R(X1) = R(L(X))
        /// 3. Return result: (R ∘ L)(X) = R(L(X))
        /// (R ∘ L)(X) = R(L(X)) where:
        /// - L: ILeftOperator<TValue, TState> (first operator, structural transformation)
        /// - R: IRightOperator<TValue, TState> (second operator, normalization/state management)
        /// - X: SNOAObject<TValue, TState> (input object)
        /// - Result: SNOAObject<TValue, TState> (composed result)
        /// - This demonstrates noncommutativity: (R ∘ L)(X) ≠ (L ∘ R)(X) in general
        /// - Order matters: add edge then normalize ≠ normalize then add edge
        /// - Example: AddEdge ∘ NormalizeGraph ≠ NormalizeGraph ∘ AddEdge
        /// - Closure: (R ∘ L)(X) ∈ 𝕏 (Axiom A1)
        /// - Type preservation: composition preserves (TValue, TState) types
        /// - Noncommutativity: R ∘ L ≠ L ∘ R (Axiom A5)
        /// var result = normalizeGraph.Compose(addEdge, graphNode);
        /// </summary>
        /// <typeparam name="TValue">Type_V - main value type</typeparam>
        /// <typeparam name="TState">Type_σ - internal state type</typeparam>
        /// <param name="right">Right operator (applied after left, normalization/state management)</param>
        /// <param name="left">Left operator (applied first, structural transformation)</param>
        /// <param name="x">Input SNOA object X = (V, P, σ)</param>
        /// <returns>Result SNOA object (R ∘ L)(X) = R(L(X))</returns>
        public static SNOAObject<TValue, TState> Compose<TValue, TState>(
            this IRightOperator<TValue, TState> right,
            ILeftOperator<TValue, TState> left,
            SNOAObject<TValue, TState> x)
        {
            // Algorithm: (R ∘ L)(X) = R(L(X))
            // Step 1: Apply L to X: L(X) = X1 (structural transformation first)
            var transformed = left.Apply(x);
            
            // Step 2: Apply R to X1: R(X1) = R(L(X)) (normalization/state management after)
            var result = right.Apply(transformed);
            
            // Step 3: Return (R ∘ L)(X) = R(L(X))
            // This satisfies Axiom A6: composition is function composition
            // This demonstrates Axiom A5: (R ∘ L)(X) ≠ (L ∘ R)(X) in general
            return result;
        }
    }
}



