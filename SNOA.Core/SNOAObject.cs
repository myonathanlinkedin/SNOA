using System.Collections.Generic;

namespace SNOA.Core
{
    /// <summary>
    /// SNOA Object: X = (V, P, σ)
    /// - V: main value (core data) of type TValue
    /// - P: properties (attributes, metadata) - Map[String, Value]
    /// - σ: internal state (mutable, changed by operations) of type TState
    /// </summary>
    /// <typeparam name="TValue">Type_V - main value type</typeparam>
    /// <typeparam name="TState">Type_σ - internal state type</typeparam>
    public class SNOAObject<TValue, TState>
    {
        /// <summary>
        /// V: main value (core data)
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// P: properties (attributes, metadata) - Map[String, Value]
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// σ: internal state (mutable, changed by operations)
        /// </summary>
        public TState State { get; set; }

        /// <summary>
        /// Constructor for SNOA Object
        /// 1. Initialize V (Value) with provided value
        /// 2. Initialize P (Properties) with provided properties or empty dictionary (null-safe)
        /// 3. Initialize σ (State) with provided state
        /// - X = (V, P, σ) where:
        ///   - V: main value (core data) of type TValue
        ///   - P: properties (attributes, metadata) - Dictionary<string, object>
        ///   - σ: internal state (mutable, changed by operations) of type TState
        /// - Properties is null-safe: if null, initializes to empty dictionary
        /// - Value and State are not null-checked (type system handles nullable types)
        /// </summary>
        /// <param name="value">V: main value (core data)</param>
        /// <param name="properties">P: properties map (null-safe, defaults to empty dictionary)</param>
        /// <param name="state">σ: internal state (mutable, changed by operations)</param>
        public SNOAObject(TValue value, Dictionary<string, object>? properties, TState state)
        {
            // Initialize V (main value)
            Value = value;
            
            // Initialize P (properties) - null-safe: use empty dictionary if null
            Properties = properties ?? new Dictionary<string, object>();
            
            // Initialize σ (internal state)
            State = state;
        }

        /// <summary>
        /// Object Equality: X₁ = X₂ iff V₁ = V₂ and P₁ = P₂ and σ₁ = σ₂
        /// 1. Check if obj is SNOAObject<TValue, TState> type
        /// 2. Compare V components: V₁ = V₂ using EqualityComparer
        /// 3. Compare P components: P₁ = P₂ (dictionary comparison - same keys and values)
        /// 4. Compare σ components: σ₁ = σ₂ using EqualityComparer
        /// 5. Return true if all components are equal
        /// X₁ = X₂ if and only if:
        /// - V₁ = V₂ (main values are equal)
        /// - P₁ = P₂ (properties maps are equal - same keys and values)
        /// - σ₁ = σ₂ (internal states are equal)
        /// - Same count of key-value pairs
        /// - All keys in P₁ exist in P₂ with same values
        /// - All keys in P₂ exist in P₁ with same values
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if X₁ = X₂ (all components V, P, σ are equal)</returns>
        public override bool Equals(object? obj)
        {
            // Step 1: Check if obj is SNOAObject<TValue, TState> type
            if (obj is not SNOAObject<TValue, TState> other)
                return false;

            // Step 2: Compare V components: V₁ = V₂
            // Algorithm: Use EqualityComparer<TValue> for type-safe equality
            if (!EqualityComparer<TValue>.Default.Equals(Value, other.Value))
                return false;

            // Step 3: Compare P components: P₁ = P₂ (dictionary comparison)
            // Algorithm: Check count first, then compare each key-value pair
            if (Properties.Count != other.Properties.Count)
                return false;

            // Compare all key-value pairs in P₁ with P₂
            foreach (var kvp in Properties)
            {
                // Check if key exists in P₂ and value is equal
                if (!other.Properties.TryGetValue(kvp.Key, out var otherValue) ||
                    !Equals(kvp.Value, otherValue))
                    return false;
            }

            // Step 4: Compare σ components: σ₁ = σ₂
            // Algorithm: Use EqualityComparer<TState> for type-safe equality
            if (!EqualityComparer<TState>.Default.Equals(State, other.State))
                return false;

            // Step 5: All components are equal
            return true;
        }

        /// <summary>
        /// GetHashCode implementation for object equality
        /// 1. Create HashCode builder
        /// 2. Add V (Value) to hash code
        /// 3. Add σ (State) to hash code
        /// 4. Add all P (Properties) key-value pairs to hash code
        /// 5. Return combined hash code
        /// - If X₁ = X₂, then GetHashCode(X₁) = GetHashCode(X₂) (required for Equals)
        /// - Hash code includes all components: V, P, σ
        /// - Properties are included in order-independent way (dictionary iteration)
        /// - Dictionary keys (Dictionary<SNOAObject, ...>)
        /// - HashSet membership (HashSet<SNOAObject>)
        /// - Hash-based collections
        /// </summary>
        /// <returns>Hash code for SNOA object based on V, P, σ</returns>
        public override int GetHashCode()
        {
            // Create HashCode builder
            var hashCode = new HashCode();
            
            // Add V (Value) to hash code
            hashCode.Add(Value);
            
            // Add σ (State) to hash code
            hashCode.Add(State);
            
            // Add all P (Properties) key-value pairs to hash code
            // Algorithm: Iterate through all properties and add key and value
            foreach (var kvp in Properties)
            {
                hashCode.Add(kvp.Key);   // Add property key
                hashCode.Add(kvp.Value); // Add property value
            }

            // Return combined hash code
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Well-formedness check
        /// 1. Check if V is of correct type (not null if type is non-nullable)
        /// 2. Check if P is valid property map (not null, no null keys)
        /// 3. Check if σ is valid state (not null if type is non-nullable)
        /// 4. Return true if all basic checks pass
        /// An object X = (V, P, σ) is well-formed if:
        /// 1. V is of the correct type Type_V (type system + null check)
        /// 2. P is a valid property map (no null keys, type-consistent values)
        /// 3. σ is a valid state of type Type_σ (type system + null check)
        /// 4. All domain-specific invariants I(X) hold (must be checked by domain code)
        /// - This method checks basic structural well-formedness
        /// - Domain-specific invariants (e.g., graph connectivity, state consistency) must be checked separately
        /// - This is a basic validation, not a complete domain validation
        /// </summary>
        /// <returns>True if object is well-formed (basic structural checks pass)</returns>
        public bool IsWellFormed()
        {
            // Step 1: Check if V is of correct type (not null if type is non-nullable)
            // Algorithm: If TValue is non-nullable (default(TValue) != null), then Value must not be null
            // Type system ensures Value is of type TValue, but we check null for non-nullable types
            if (Value == null && default(TValue) != null)
                return false;

            // Step 2: Check if P is valid property map (not null, no null keys)
            // Algorithm: Properties must not be null, and all keys must not be null
            if (Properties == null)
                return false;

            // Check all keys are not null (null keys are invalid in Dictionary)
            foreach (var key in Properties.Keys)
            {
                if (key == null)
                    return false;
            }

            // Step 3: Check if σ is valid state (not null if type is non-nullable)
            // Algorithm: If TState is non-nullable (default(TState) != null), then State must not be null
            // Type system ensures State is of type TState, but we check null for non-nullable types
            if (State == null && default(TState) != null)
                return false;

            // Step 4: Basic structural checks passed
            // Note: Domain-specific invariants (e.g., graph connectivity, state consistency) 
            // must be checked by domain code, not here
            return true;
        }
    }
}



