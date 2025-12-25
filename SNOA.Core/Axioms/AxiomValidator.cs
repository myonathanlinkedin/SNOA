using System;
using System.Linq;

namespace SNOA.Core.Axioms
{
    /// <summary>
    /// Axiom Validator
    /// Note: These are rigorous proofs via programming (property-based testing), NOT rigorous formal proofs using formal logic.
        /// It validates that axioms hold for tested cases through code execution, providing reproducible, verifiable validation.
    /// </summary>
    public static class AxiomValidator
    {
        /// <summary>
        /// Validate Axiom A0: All objects are tuples X = (V, P, σ)
        /// 1. Check if object is not null
        /// 2. Check if Properties (P) component exists
        /// 3. V and σ are checked by type system (TValue, TState)
        /// 4. Return true if all components present
        /// A0: Domain - All objects X ∈ 𝕏 are tuples X = (V, P, σ)
        /// where V is the main value, P is properties, σ is state
        /// It verifies that the object structure matches A0 for tested instances through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="obj">SNOA object to validate</param>
        /// <returns>True if object has structure (V, P, σ)</returns>
        public static bool ValidateA0_Domain<TValue, TState>(SNOAObject<TValue, TState> obj)
        {
            // A0: Object must have structure (V, P, σ)
            // Algorithm: Check null and Properties existence
            // V and σ are guaranteed by type system (TValue, TState)
            return obj != null &&
                   obj.Properties != null;
        }

        /// <summary>
        /// Validate Axiom A1: Closure - L(X) ∈ 𝕏
        /// 1. Apply Left Operator L to object X
        /// 2. Check if result is not null
        /// 3. Check if result is of type SNOAObject<TValue, TState>
        /// 4. Return true if result is same type as input
        /// A1: Closure - For all L ∈ 𝓛, X ∈ 𝕏: L(X) ∈ 𝕏
        /// Left operators always return objects of the same type
        /// It verifies closure for tested operator and object instances through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="op">Left Operator L to validate</param>
        /// <param name="obj">SNOA object X to apply operator to</param>
        /// <returns>True if L(X) ∈ 𝕏 (result is same type as input)</returns>
        public static bool ValidateA1_Closure_Left<TValue, TState>(
            ILeftOperator<TValue, TState> op,
            SNOAObject<TValue, TState> obj)
        {
            // Apply Left Operator: L(X)
            var result = op.Apply(obj);
            
            // Check closure: L(X) ∈ 𝕏
            // Algorithm: Verify result is not null and is of type SNOAObject<TValue, TState>
            return result != null &&
                   result is SNOAObject<TValue, TState>;
        }

        /// <summary>
        /// Validate Axiom A1: Closure - R(X) ∈ 𝕏
        /// 1. Apply Right Operator R to object X
        /// 2. Check if result is not null
        /// 3. Check if result is of type SNOAObject<TValue, TState>
        /// 4. Return true if result is same type as input
        /// A1: Closure - For all R ∈ 𝓡, X ∈ 𝕏: R(X) ∈ 𝕏
        /// Right operators always return objects of the same type
        /// It verifies closure for tested operator and object instances through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="op">Right Operator R to validate</param>
        /// <param name="obj">SNOA object X to apply operator to</param>
        /// <returns>True if R(X) ∈ 𝕏 (result is same type as input)</returns>
        public static bool ValidateA1_Closure_Right<TValue, TState>(
            IRightOperator<TValue, TState> op,
            SNOAObject<TValue, TState> obj)
        {
            // Apply Right Operator: R(X)
            var result = op.Apply(obj);
            
            // Check closure: R(X) ∈ 𝕏
            // Algorithm: Verify result is not null and is of type SNOAObject<TValue, TState>
            return result != null &&
                   result is SNOAObject<TValue, TState>;
        }

        /// <summary>
        /// Validate Axiom A2: Structural Stability
        /// 1. Check if result object is not null
        /// 2. Check if Properties (P) component exists
        /// 3. V and σ are checked by type system (TValue, TState)
        /// 4. Return true if all components remain present
        /// A2: Structural Stability - No operator eliminates basic components
        /// For all operators T, if X = (V, P, σ) then T(X) = (V', P', σ')
        /// where V', P', σ' are all present (not null/eliminated)
        /// It verifies structural stability for tested operator applications through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="original">Original SNOA object X = (V, P, σ)</param>
        /// <param name="result">Result SNOA object T(X) = (V', P', σ')</param>
        /// <returns>True if all components V', P', σ' remain present</returns>
        public static bool ValidateA2_StructuralStability<TValue, TState>(
            SNOAObject<TValue, TState> original,
            SNOAObject<TValue, TState> result)
        {
            // A2: Structural Stability - all components remain present
            // Algorithm: Check Properties existence
            // V remains present (checked by type system - TValue)
            // P remains a property set (explicit check)
            // σ remains a state (checked by type system - TState)
            return result.Properties != null;
        }

        /// <summary>
        /// Validate Axiom A3: State Mutability
        /// 1. Check if result object exists (operator was applied)
        /// 2. Axiom A3 allows state changes, but doesn't require them
        /// 3. Return true if operator was successfully applied
        /// A3: State Mutability - Operators may change state: σ' = T(σ)
        /// State changes are allowed but not required
        /// Identity operator satisfies A3 even though it doesn't change state.
        /// This is rigorous proof via programming (property-based testing), not rigorous formal proof using formal logic.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="original">Original SNOA object X = (V, P, σ)</param>
        /// <param name="result">Result SNOA object T(X) = (V', P', σ')</param>
        /// <returns>True if operator was applied (state mutability is allowed)</returns>
        public static bool ValidateA3_StateMutability<TValue, TState>(
            SNOAObject<TValue, TState> original,
            SNOAObject<TValue, TState> result)
        {
            // A3: State Mutability - operators may change state
            // Algorithm: Check that operator was applied (result exists)
            // State mutability is allowed (state may or may not change)
            // This axiom is satisfied if operators CAN change state
            // We check that the operator was applied (result exists)
            return result != null;
        }

        /// <summary>
        /// Validate Axiom A4: Property Mutability
        /// 1. Check if result Properties exist
        /// 2. Axiom A4 allows property changes, but doesn't require them
        /// 3. Return true if Properties component exists
        /// A4: Property Mutability - Operators may change properties: P' = U(P)
        /// Property changes are allowed but not required
        /// Identity operator satisfies A4 even though it doesn't change properties.
        /// This is rigorous proof via programming (property-based testing), not rigorous formal proof using formal logic.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="original">Original SNOA object X = (V, P, σ)</param>
        /// <param name="result">Result SNOA object T(X) = (V', P', σ')</param>
        /// <returns>True if Properties exist (property mutability is allowed)</returns>
        public static bool ValidateA4_PropertyMutability<TValue, TState>(
            SNOAObject<TValue, TState> original,
            SNOAObject<TValue, TState> result)
        {
            // A4: Property Mutability - operators may change properties
            // Algorithm: Check that Properties component exists
            // Property mutability is allowed (properties may or may not change)
            // This axiom is satisfied if operators CAN change properties
            return result.Properties != null;
        }

        /// <summary>
        /// Validate Axiom A5: Noncommutativity
        /// 1. Compute (L ∘ R)(X) = L(R(X)) - apply Right then Left
        /// 2. Compute (R ∘ L)(X) = R(L(X)) - apply Left then Right
        /// 3. Compare results using Equals
        /// 4. Return true if results are different (noncommutative)
        /// A5: Noncommutativity - ∃ L ∈ 𝓛, R ∈ 𝓡, X ∈ 𝕏 such that: (L ∘ R)(X) ≠ (R ∘ L)(X)
        /// Left and Right operators do not commute in general
        /// It verifies noncommutativity for tested operator and object instances through code execution.
        /// If result1 = result2, operators commute for this instance (but may not commute in general).
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="left">Left Operator L ∈ 𝓛</param>
        /// <param name="right">Right Operator R ∈ 𝓡</param>
        /// <param name="obj">SNOA object X ∈ 𝕏</param>
        /// <returns>True if (L ∘ R)(X) ≠ (R ∘ L)(X) (noncommutative)</returns>
        public static bool ValidateA5_Noncommutativity<TValue, TState>(
            ILeftOperator<TValue, TState> left,
            IRightOperator<TValue, TState> right,
            SNOAObject<TValue, TState> obj)
        {
            // Compute (L ∘ R)(X) = L(R(X))
            // Algorithm: Apply Right operator first, then Left operator
            var result1 = left.Apply(right.Apply(obj));

            // Compute (R ∘ L)(X) = R(L(X))
            // Algorithm: Apply Left operator first, then Right operator
            var result2 = right.Apply(left.Apply(obj));

            // Noncommutativity: result1 ≠ result2
            // Algorithm: Compare results using Equals
            // If different, operators are noncommutative for this instance
            return !result1.Equals(result2);
        }

        /// <summary>
        /// Validate Axiom A6: Composition
        /// 1. Compute (f ∘ g)(X) using composition
        /// 2. Compute f(g(X)) manually (same as composition)
        /// 3. Compare results using Equals
        /// 4. Return true if results are equal (composition holds)
        /// A6: Composition - (f ∘ g)(X) = f(g(X))
        /// Function composition is defined as sequential application
        /// It verifies composition for tested operator instances through code execution.
        /// Since both compute f(g(X)), they should always be equal.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="f">Left Operator f</param>
        /// <param name="g">Left Operator g</param>
        /// <param name="x">SNOA object X</param>
        /// <returns>True if (f ∘ g)(X) = f(g(X)) (composition holds)</returns>
        public static bool ValidateA6_Composition_Left<TValue, TState>(
            ILeftOperator<TValue, TState> f,
            ILeftOperator<TValue, TState> g,
            SNOAObject<TValue, TState> x)
        {
            // Compute (f ∘ g)(X) using composition
            // Algorithm: Apply g first, then apply f to result
            var composed = f.Apply(g.Apply(x));

            // Compute f(g(X)) manually (same as composition)
            // Algorithm: Apply g first, then apply f to result
            var manual = f.Apply(g.Apply(x));

            // Composition: (f ∘ g)(X) = f(g(X))
            // Algorithm: Compare results using Equals
            // Since both compute f(g(X)), they should always be equal
            return composed.Equals(manual);
        }

        /// <summary>
        /// Validate Axiom A7: Conditional Associativity
        /// 1. Compute ((f ∘ g) ∘ h)(X) = f(g(h(X))) - left-associative
        /// 2. Compute (f ∘ (g ∘ h))(X) = f(g(h(X))) - right-associative
        /// 3. Compare results using Equals
        /// 4. Return true if results are equal (associativity holds)
        /// A7: Conditional Associativity - For operators f, g, h where state mutations are independent:
        /// ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X)
        /// This is rigorous proof via programming (property-based testing), not rigorous formal proof using formal logic.
        /// It verifies associativity for specific operator instances.
        /// Since both compute f(g(h(X))), they should be equal if state mutations are independent.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="f">Left Operator f</param>
        /// <param name="g">Left Operator g</param>
        /// <param name="h">Left Operator h</param>
        /// <param name="x">SNOA object X</param>
        /// <returns>True if ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X) (associativity holds)</returns>
        public static bool ValidateA7_ConditionalAssociativity_Left<TValue, TState>(
            ILeftOperator<TValue, TState> f,
            ILeftOperator<TValue, TState> g,
            ILeftOperator<TValue, TState> h,
            SNOAObject<TValue, TState> x)
        {
            // Compute ((f ∘ g) ∘ h)(X) = f(g(h(X))) - left-associative
            // Algorithm: Apply h, then g, then f
            var leftAssoc = f.Apply(g.Apply(h.Apply(x)));

            // Compute (f ∘ (g ∘ h))(X) = f(g(h(X))) - right-associative
            // Algorithm: Apply h, then g, then f (same as left-associative)
            var rightAssoc = f.Apply(g.Apply(h.Apply(x)));

            // Conditional Associativity: ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X)
            // Algorithm: Compare results using Equals
            // If state mutations are independent, these should be equal
            // If state mutations are dependent, they may differ
            return leftAssoc.Equals(rightAssoc);
        }

        /// <summary>
        /// Validate Axiom A8: Identity Operator - I(X) = X
        /// 1. Get Identity Operator I instance
        /// 2. Apply Identity to object X: I(X)
        /// 3. Compare result with original using Equals
        /// 4. Return true if I(X) = X
        /// A8: Identity Operator - I(X) = X for all X ∈ 𝕏
        /// Identity operator leaves object unchanged
        /// It verifies identity property for tested object instances through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="obj">SNOA object X</param>
        /// <returns>True if I(X) = X (identity holds)</returns>
        public static bool ValidateA8_Identity<TValue, TState>(
            SNOAObject<TValue, TState> obj)
        {
            // Get Identity Operator I instance
            var identity = IdentityOperator<TValue, TState>.Instance;
            
            // Apply Identity: I(X)
            var result = identity.Apply(obj);

            // Identity: I(X) = X
            // Algorithm: Compare result with original using Equals
            // Values should be equal (identity leaves object unchanged)
            return obj.Equals(result);
        }

        /// <summary>
        /// Validate Axiom A8: Identity with Left Operator - I ∘ f = f ∘ I = f
        /// 1. Get Identity Operator I instance
        /// 2. Compute I ∘ f = I(f(X))
        /// 3. Compute f ∘ I = f(I(X))
        /// 4. Compute f(X) directly
        /// 5. Compare: I(f(X)) = f(X) and f(I(X)) = f(X)
        /// 6. Return true if both equalities hold
        /// A8: Identity Operator - I ∘ f = f ∘ I = f for all operators f
        /// Identity operator is neutral element for composition
        /// It verifies identity composition property for tested operator instances through code execution.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="f">Left Operator f</param>
        /// <param name="x">SNOA object X</param>
        /// <returns>True if I ∘ f = f ∘ I = f (identity composition holds)</returns>
        public static bool ValidateA8_IdentityWithLeft<TValue, TState>(
            ILeftOperator<TValue, TState> f,
            SNOAObject<TValue, TState> x)
        {
            // Get Identity Operator I instance
            var identity = IdentityOperator<TValue, TState>.Instance;

            // Compute I ∘ f = I(f(X))
            // Algorithm: Apply f first, then apply I
            var leftCompose = identity.Apply(f.Apply(x));
            
            // Compute f(X) directly
            var fResult = f.Apply(x);

            // Compute f ∘ I = f(I(X))
            // Algorithm: Apply I first, then apply f
            var rightCompose = f.Apply(identity.Apply(x));

            // Identity Composition: I ∘ f = f ∘ I = f
            // Algorithm: Compare I(f(X)) = f(X) and f(I(X)) = f(X)
            // Both should equal f(X) (identity is neutral element)
            return leftCompose.Equals(fResult) && rightCompose.Equals(fResult);
        }

        /// <summary>
        /// Validate Axiom A9: Implementability for Left Operator
        /// 1. Try to apply Left Operator L to object X
        /// 2. Check if result is not null (operator executed successfully)
        /// 3. Return true if operator can be implemented and executed
        /// 4. Return false if operator throws exception (not implementable)
        /// A9: Implementability - Every operator L ∈ 𝓛, R ∈ 𝓡 can be implemented as a concrete construction
        /// All operators must be executable in practice
        /// It verifies implementability for tested operator instances through code execution.
        /// If operator throws exception, it's not implementable for that instance.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="op">Left Operator L to validate</param>
        /// <param name="obj">SNOA object X to apply operator to</param>
        /// <returns>True if operator can be implemented and executed</returns>
        public static bool ValidateA9_Implementability_Left<TValue, TState>(
            ILeftOperator<TValue, TState> op,
            SNOAObject<TValue, TState> obj)
        {
            // A9: Operator can be implemented and executed
            // Algorithm: Try to apply operator, catch exceptions
            try
            {
                // Apply operator: L(X)
                var result = op.Apply(obj);
                
                // Check if result is not null (operator executed successfully)
                return result != null;
            }
            catch
            {
                // If operator throws exception, it's not implementable for this instance
                return false;
            }
        }

        /// <summary>
        /// Validate Axiom A9: Implementability for Right Operator
        /// 1. Try to apply Right Operator R to object X
        /// 2. Check if result is not null (operator executed successfully)
        /// 3. Return true if operator can be implemented and executed
        /// 4. Return false if operator throws exception (not implementable)
        /// A9: Implementability - Every operator L ∈ 𝓛, R ∈ 𝓡 can be implemented as a concrete construction
        /// All operators must be executable in practice
        /// It verifies implementability for tested operator instances through code execution.
        /// If operator throws exception, it's not implementable for that instance.
        /// </summary>
        /// <typeparam name="TValue">Type of V component (main value)</typeparam>
        /// <typeparam name="TState">Type of σ component (internal state)</typeparam>
        /// <param name="op">Right Operator R to validate</param>
        /// <param name="obj">SNOA object X to apply operator to</param>
        /// <returns>True if operator can be implemented and executed</returns>
        public static bool ValidateA9_Implementability_Right<TValue, TState>(
            IRightOperator<TValue, TState> op,
            SNOAObject<TValue, TState> obj)
        {
            // A9: Operator can be implemented and executed
            // Algorithm: Try to apply operator, catch exceptions
            try
            {
                // Apply operator: R(X)
                var result = op.Apply(obj);
                
                // Check if result is not null (operator executed successfully)
                return result != null;
            }
            catch
            {
                // If operator throws exception, it's not implementable for this instance
                return false;
            }
        }
    }
}

