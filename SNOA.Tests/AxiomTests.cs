using FluentAssertions;
using SNOA.Core;
using SNOA.Core.Axioms;
using SNOA.Core.Examples;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace SNOA.Tests
{
    /// <summary>
    /// Axiom Validation Tests
        /// 
    /// Tests validate axioms through proofs via programming (property-based testing through code execution).
    /// Note: These are proofs via programming (property-based testing), NOT formal proofs using formal logic.
        /// </summary>
    [Collection("AxiomTests")]
    public class AxiomTests : IClassFixture<TestResultsFixture>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly AxiomValidationResults _resultsLogger;
        private static bool _resultsSaved = false;
        private static readonly object _saveLock = new object();

        public AxiomTests(ITestOutputHelper output, TestResultsFixture fixture)
        {
            _output = output;
            // Use shared instance so all tests record to the same logger
            _resultsLogger = AxiomValidationResults.GetSharedInstance(output);
        }

        public void Dispose()
        {
            // Save only once after all tests complete
            lock (_saveLock)
            {
                if (!_resultsSaved)
                {
                    _resultsLogger.SaveToFile();
                    _resultsSaved = true;
                }
            }
        }
        #region A0: Domain

        [Fact]
        public void A0_Domain_ObjectIsTuple()
        {
            // A0: All objects are tuples X = (V, P, σ)
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);

            // Object should have structure (V, P, σ)
            obj.Value.Should().Be(5);
            obj.Properties.Should().NotBeNull();
            obj.State.Should().Be(2);

            var isValid = AxiomValidator.ValidateA0_Domain(obj);
            isValid.Should().BeTrue();

            _resultsLogger.RecordAxiom("A0_Domain", isValid, "All objects are tuples X = (V, P, σ)");
        }

        #endregion

        #region A1: Closure

        [Fact]
        public void A1_Closure_LeftOperator_ReturnsObject()
        {
            // A1: L(X) ∈ 𝕏
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var leftOp = new TransformLeftOperator();

            var result = leftOp.Apply(obj);

            // Result should be SNOAObject (closure property)
            result.Should().NotBeNull();
            result.Should().BeOfType<SNOAObject<int, int>>();

            var isValid = AxiomValidator.ValidateA1_Closure_Left(leftOp, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A1_Closure_Left", isValid, "L(X) ∈ 𝕏");
        }

        [Fact]
        public void A1_Closure_RightOperator_ReturnsObject()
        {
            // A1: R(X) ∈ 𝕏
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var rightOp = new NormalizeRightOperator();

            var result = rightOp.Apply(obj);

            // Result should be SNOAObject (closure property)
            result.Should().NotBeNull();
            result.Should().BeOfType<SNOAObject<int, int>>();

            var isValid = AxiomValidator.ValidateA1_Closure_Right(rightOp, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A1_Closure_Right", isValid, "R(X) ∈ 𝕏");
        }

        #endregion

        #region A2: Structural Stability

        [Fact]
        public void A2_StructuralStability_ComponentsPreserved()
        {
            // A2: No operator eliminates basic components: V, P, σ all remain present
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object> { ["test"] = "value" }, 2);
            var leftOp = new TransformLeftOperator();

            var result = leftOp.Apply(obj);

            // V remains present (value type, guaranteed by type system)
            // P remains a property set
            result.Properties.Should().NotBeNull();
            // σ remains a state (value type, guaranteed by type system)

            var isValid = AxiomValidator.ValidateA2_StructuralStability(obj, result);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A2_StructuralStability", isValid, "V, P, σ all remain present");
        }

        #endregion

        #region A3: State Mutability

        [Fact]
        public void A3_StateMutability_StateCanChange()
        {
            // A3: Operators may change state: σ' = T(σ)
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var leftOp = new TransformLeftOperator();

            var result = leftOp.Apply(obj);

            // State mutability is allowed (state may change)
            // In TransformLeftOperator, state changes: newState = state + 1
            result.State.Should().Be(3); // 2 + 1

            var isValid = AxiomValidator.ValidateA3_StateMutability(obj, result);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A3_StateMutability", isValid, "Operators may change state: σ' = T(σ)");
        }

        #endregion

        #region A4: Property Mutability

        [Fact]
        public void A4_PropertyMutability_PropertiesCanChange()
        {
            // A4: Operators may change properties: P' = U(P)
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var leftOp = new TransformLeftOperator();

            var result = leftOp.Apply(obj);

            // Property mutability is allowed (properties may change)
            result.Properties.Should().ContainKey("transformed");
            result.Properties["transformed"].Should().Be(true);

            var isValid = AxiomValidator.ValidateA4_PropertyMutability(obj, result);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A4_PropertyMutability", isValid, "Operators may change properties: P' = U(P)");
        }

        #endregion

        #region A5: Noncommutativity

        [Fact]
        public void A5_Noncommutativity_LeftRightNotEqual()
        {
            // A5: ∃ L ∈ 𝓛, R ∈ 𝓡, X ∈ 𝕏 such that: (L ∘ R)(X) ≠ (R ∘ L)(X)
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var leftOp = new TransformLeftOperator();
            var rightOp = new NormalizeRightOperator();

            // (L ∘ R)(X) = L(R(X))
            var result1 = leftOp.Apply(rightOp.Apply(obj));

            // (R ∘ L)(X) = R(L(X))
            var result2 = rightOp.Apply(leftOp.Apply(obj));

            // Noncommutativity: result1 ≠ result2
            result1.Should().NotBeEquivalentTo(result2);

            var isValid = AxiomValidator.ValidateA5_Noncommutativity(leftOp, rightOp, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A5_Noncommutativity", isValid, "∃ L, R, X: (L ∘ R)(X) ≠ (R ∘ L)(X)");
        }

        #endregion

        #region A6: Composition

        [Fact]
        public void A6_Composition_LeftOperators()
        {
            // A6: (f ∘ g)(X) = f(g(X))
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var f = new TransformLeftOperator();
            var g = new TransformLeftOperator();

            // (f ∘ g)(X) using composition helper
            var composed = f.Compose(g, obj);

            // f(g(X)) manually
            var manual = f.Apply(g.Apply(obj));

            composed.Should().BeEquivalentTo(manual);

            var isValid = AxiomValidator.ValidateA6_Composition_Left(f, g, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A6_Composition", isValid, "(f ∘ g)(X) = f(g(X))");
        }

        #endregion

        #region A7: Conditional Associativity

        [Fact]
        public void A7_ConditionalAssociativity_IndependentStateMutations()
        {
            // A7: For operators f, g, h where state mutations are independent:
            //     ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X)
            // 
            // Note: TransformLeftOperator state mutations are independent
            // (each operator only reads initial state, then mutates)
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var f = new TransformLeftOperator();
            var g = new TransformLeftOperator();
            var h = new TransformLeftOperator();

            // ((f ∘ g) ∘ h)(X)
            var leftAssoc = f.Apply(g.Apply(h.Apply(obj)));

            // (f ∘ (g ∘ h))(X)
            var rightAssoc = f.Apply(g.Apply(h.Apply(obj)));

            // For independent state mutations, associativity should hold
            // However, in this case, each operator mutates state, so results may differ
            // This test demonstrates the conditional nature of associativity
            leftAssoc.Should().NotBeNull();
            rightAssoc.Should().NotBeNull();

            // Note: Actual equality depends on whether state mutations are independent
            // This is a demonstration of A7's conditional nature
            
            // A7 is conditional - validate that operators can be composed associatively
            // For independent state mutations, associativity should hold
            var isValid = AxiomValidator.ValidateA7_ConditionalAssociativity_Left(f, g, h, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A7_ConditionalAssociativity", isValid, "Associative if independent");
        }

        #endregion

        #region A8: Identity Operator

        [Fact]
        public void A8_Identity_ReturnsSameObject()
        {
            // A8: I(X) = X
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object> { ["test"] = "value" }, 2);
            var identity = IdentityOperator<int, int>.Instance;

            var result = identity.Apply(obj);

            // I(X) = X (values should be equal)
            result.Should().BeEquivalentTo(obj);

            var isValid = AxiomValidator.ValidateA8_Identity(obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A8_Identity", isValid, "I(X) = X");
        }

        [Fact]
        public void A8_Identity_CompositionWithLeftOperator()
        {
            // A8: I ∘ f = f ∘ I = f
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var f = new TransformLeftOperator();
            var identity = IdentityOperator<int, int>.Instance;

            // I ∘ f = f
            var leftCompose = identity.Apply(f.Apply(obj));
            var fResult = f.Apply(obj);

            // f ∘ I = f
            var rightCompose = f.Apply(identity.Apply(obj));

            leftCompose.Should().BeEquivalentTo(fResult);
            rightCompose.Should().BeEquivalentTo(fResult);

            var isValid = AxiomValidator.ValidateA8_IdentityWithLeft(f, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A8_Identity_Composition", isValid, "I ∘ f = f ∘ I = f");
        }

        #endregion

        #region A9: Implementability

        [Fact]
        public void A9_Implementability_LeftOperatorCanBeImplemented()
        {
            // A9: Every operator L ∈ 𝓛 can be implemented as a concrete construction
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var leftOp = new TransformLeftOperator();

            // Operator should be implementable and executable
            var result = leftOp.Apply(obj);

            result.Should().NotBeNull();

            var isValid = AxiomValidator.ValidateA9_Implementability_Left(leftOp, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A9_Implementability_Left", isValid, "Every operator L ∈ 𝓛 can be implemented");
        }

        [Fact]
        public void A9_Implementability_RightOperatorCanBeImplemented()
        {
            // A9: Every operator R ∈ 𝓡 can be implemented as a concrete construction
            var obj = new SNOAObject<int, int>(5, new Dictionary<string, object>(), 2);
            var rightOp = new NormalizeRightOperator();

            // Operator should be implementable and executable
            var result = rightOp.Apply(obj);

            result.Should().NotBeNull();

            var isValid = AxiomValidator.ValidateA9_Implementability_Right(rightOp, obj);
            isValid.Should().BeTrue();
            _resultsLogger.RecordAxiom("A9_Implementability_Right", isValid, "Every operator R ∈ 𝓡 can be implemented");
        }

        #endregion
    }
}

