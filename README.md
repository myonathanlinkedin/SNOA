# SNOA (Stateful Noncommutative Object Algebra)

**A Unified Framework for Stateful, Noncommutative Systems**

with C# .NET 10 Implementation and Validation

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white)
![License](https://img.shields.io/badge/license-Apache%202.0-blue?style=flat-square)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey?style=flat-square)
![Tests](https://img.shields.io/badge/tests-62%20passed-success?style=flat-square)

A C# .NET 10 framework for proofs via programming through property-based testing. SNOA provides a formal algebraic structure for object transformations with validated axioms and comprehensive case studies.

## Overview

SNOA (Stateful Noncommutative Object Algebra) is a unified framework for stateful, noncommutative systems. It implements proofs via programming for object transformations, providing a mathematical foundation for object operations with validated axioms, operator composition, and proof validation through code execution.

### Core Concept

Every object in SNOA is a tuple: **X = (V, P, σ)**

- **V** (Value): Main value component - the core data of type `TValue`
- **P** (Properties): Properties map - attributes and metadata as `Dictionary<string, object>`
- **σ** (State): Internal state - mutable state of type `TState` that changes through operations

## Features

- **Axiom-Based Architecture**: 10 validated axioms (A0-A9) ensuring structural consistency
- **Dual Operator System**: 
  - **Left Operators (𝓛)**: Structural transformations, level updates, event application
  - **Right Operators (𝓡)**: Normalization, state commit, neighbor updates
- **Operator Composition**: Support for composing operators with validated associativity
- **Proof Validation**: Property-based testing for consistency, independence, and theorems
- **Case Studies**: 
  - **Dynamic Graph**: Graph operations with AddEdge, RemoveEdge, NormalizeGraph, etc.
  - **Event Sourcing**: Event-driven state management with AppendEvent, ReplayEvents, Snapshot, etc.
- **Identity Operator**: Neutral element for composition (Axiom A8)
- **Type-Safe**: Full generic type support with `SNOAObject<TValue, TState>`

## Axioms

SNOA is built on 10 fundamental axioms:

- **A0**: Domain - All objects are tuples X = (V, P, σ)
- **A1**: Closure - L(X) ∈ 𝕏 and R(X) ∈ 𝕏 (operators return same type)
- **A2**: Structural Stability - No operator eliminates basic components
- **A3**: State Mutability - Operators may change state: σ' = T(σ)
- **A4**: Property Mutability - Operators may change properties: P' = U(P)
- **A5**: Noncommutativity - (L ∘ R)(X) ≠ (R ∘ L)(X) in general
- **A6**: Composition - (f ∘ g)(X) = f(g(X))
- **A7**: Conditional Associativity - ((f ∘ g) ∘ h)(X) = (f ∘ (g ∘ h))(X) when state mutations are independent
- **A8**: Identity Operator - I(X) = X and I ∘ f = f ∘ I = f
- **A9**: Implementability - Every operator can be implemented as a concrete construction

All axioms are validated through proofs via programming (property-based testing).

## Project Structure

```
SNOA/
├── SNOA.Core/                    # Core library
│   ├── Axioms/                   # Axiom validation and proofs
│   │   ├── AxiomValidator.cs     # Axiom validation through property-based testing
│   │   └── RigorousProofValidator.cs  # Consistency, independence, and theorem proofs
│   ├── CaseStudies/              # Real-world case studies
│   │   ├── EventSourcing/        # Event sourcing implementation
│   │   │   ├── EventLog.cs
│   │   │   ├── OrderState.cs
│   │   │   ├── AppendEventLeftOperator.cs
│   │   │   ├── ReplayEventsLeftOperator.cs
│   │   │   ├── SnapshotLeftOperator.cs
│   │   │   ├── NormalizeEventLogRightOperator.cs
│   │   │   ├── CommitSnapshotRightOperator.cs
│   │   │   └── ValidateEventLogRightOperator.cs
│   │   ├── GraphData.cs          # Graph value component
│   │   ├── GraphNode.cs          # Graph node wrapper
│   │   ├── NodeState.cs          # Graph state component
│   │   ├── AddEdgeLeftOperator.cs
│   │   ├── RemoveEdgeLeftOperator.cs
│   │   ├── SplitNodeLeftOperator.cs
│   │   ├── NormalizeGraphRightOperator.cs
│   │   ├── CommitStateRightOperator.cs
│   │   └── UpdateNeighborsRightOperator.cs
│   ├── Examples/                  # Example implementations
│   │   ├── TransformLeftOperator.cs
│   │   └── NormalizeRightOperator.cs
│   ├── SNOAObject.cs             # Core object structure X = (V, P, σ)
│   ├── ILeftOperator.cs          # Left operator interface
│   ├── IRightOperator.cs         # Right operator interface
│   ├── IdentityOperator.cs       # Identity operator implementation
│   └── OperatorComposition.cs    # Operator composition helpers
├── SNOA.Tests/                   # Unit tests and validation
│   ├── AxiomTests.cs             # Axiom validation tests
│   ├── RigorousProofTests.cs     # Proof tests
│   ├── CaseStudyTests.cs         # Dynamic graph case study tests
│   ├── EventSourcingTests.cs     # Event sourcing case study tests
│   └── Benchmarks/                # Performance benchmarks
└── LICENSE                        # Apache License 2.0
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Windows, Linux, or macOS

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd SNOA
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

### Running Tests

Run all unit tests to validate axioms and case studies:

```bash
dotnet test SNOA.Tests/SNOA.Tests.csproj
```

For verbose output:
```bash
dotnet test SNOA.Tests/SNOA.Tests.csproj --verbosity normal
```

### Running Benchmarks

Benchmarks are available in the `SNOA.Tests/Benchmarks/` directory. Run them using:

```bash
dotnet run --project SNOA.Tests/SNOA.Tests.csproj --configuration Release
```

## Usage Examples

### Basic SNOA Object

```csharp
using SNOA.Core;
using System.Collections.Generic;

// Create a SNOA object X = (V, P, σ)
var value = 42;  // V: main value
var properties = new Dictionary<string, object> 
{ 
    ["name"] = "example",
    ["count"] = 1 
};  // P: properties
var state = 0;  // σ: state

var obj = new SNOAObject<int, int>(value, properties, state);
```

### Using Left Operators

```csharp
using SNOA.Core.Examples;

// Create a transform operator
var transformOp = new TransformLeftOperator();

// Apply transformation: L(X) = (V', P', σ')
var result = transformOp.Apply(obj);

// Result has:
// - V' = V * σ (value transformed)
// - P' = P ∪ {transformed: true, transformation_count: count+1}
// - σ' = σ + 1 (state incremented)
```

### Using Right Operators

```csharp
using SNOA.Core.Examples;

// Create a normalize operator
var normalizeOp = new NormalizeRightOperator();

// Apply normalization: R(X) = (V', P', σ')
var result = normalizeOp.Apply(obj);

// Result has:
// - V' = V % 100 (value normalized)
// - P' = P ∪ {normalized: true}
// - σ' = 0 (state reset)
```

### Operator Composition

```csharp
using SNOA.Core;

// Compose operators: (f ∘ g)(X) = f(g(X))
var composed = OperatorComposition.Compose(
    leftOp1,    // f: second operator
    leftOp2,    // g: first operator
    obj         // X: input object
);

// Or use extension methods for fluent syntax
var result = leftOp1.Compose(leftOp2, obj);
```

### Identity Operator

```csharp
using SNOA.Core;

// Identity operator: I(X) = X
var identity = IdentityOperator<int, int>.Instance;
var result = identity.Apply(obj);

// Result equals original: result.Equals(obj) == true
```

### Dynamic Graph Case Study

```csharp
using SNOA.Core;
using SNOA.Core.CaseStudies;
using System.Collections.Generic;

// Create a graph node
var graphData = new GraphData(nodeId: 1, value: 10.5);
var properties = new Dictionary<string, object> { ["degree"] = 0 };
var state = new NodeState();
var node = new SNOAObject<GraphData, NodeState>(graphData, properties, state);

// Add an edge (Left Operator)
var addEdgeOp = new AddEdgeLeftOperator(targetNodeId: 2);
var nodeWithEdge = addEdgeOp.Apply(node);

// Normalize graph (Right Operator)
var normalizeOp = new NormalizeGraphRightOperator();
var normalizedNode = normalizeOp.Apply(nodeWithEdge);
```

### Event Sourcing Case Study

```csharp
using SNOA.Core;
using SNOA.Core.CaseStudies.EventSourcing;
using System.Collections.Generic;

// Create an order object
var orderState = new OrderState("ORDER-001");
var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
var eventLog = new EventLog();
var order = new SNOAObject<OrderState, EventLog>(orderState, properties, eventLog);

// Append an event (Left Operator)
var createdEvent = new OrderCreatedEvent 
{ 
    OrderId = "ORDER-001", 
    CreatedAt = DateTime.UtcNow 
};
var appendOp = new AppendEventLeftOperator(createdEvent);
var orderAfterCreated = appendOp.Apply(order);

// Replay events to rebuild state
var replayOp = new ReplayEventsLeftOperator();
var replayedOrder = replayOp.Apply(orderAfterCreated);
```

### Axiom Validation

```csharp
using SNOA.Core.Axioms;

// Validate Axiom A0: Domain
bool isValid = AxiomValidator.ValidateA0_Domain(obj);

// Validate Axiom A1: Closure
bool isClosed = AxiomValidator.ValidateA1_Closure_Left(leftOp, obj);

// Validate Axiom A5: Noncommutativity
bool isNoncommutative = AxiomValidator.ValidateA5_Noncommutativity(leftOp, rightOp, obj);
```

## Architecture

### Core Components

1. **SNOAObject<TValue, TState>**: The fundamental object structure representing X = (V, P, σ)
2. **ILeftOperator<TValue, TState>**: Interface for left operators performing structural transformations
3. **IRightOperator<TValue, TState>**: Interface for right operators performing normalization and state management
4. **IdentityOperator<TValue, TState>**: Identity operator satisfying Axiom A8
5. **OperatorComposition**: Helper class for composing operators with validated associativity

### Validation System

- **AxiomValidator**: Validates all 10 axioms through property-based testing
- **RigorousProofValidator**: Provides proofs via programming for:
  - Consistency (by construction)
  - Independence (by counterexample)
  - Theorems (by property-based testing)

### Case Studies

1. **Dynamic Graph**: Demonstrates graph operations with nodes, edges, and normalization
2. **Event Sourcing**: Shows event-driven state management with event log and state projection

## Testing

The project includes comprehensive unit tests:

- **Axiom Tests**: Validate all 10 axioms hold for various operators and objects
- **Proof Tests**: Validate consistency, independence, and theorems
- **Case Study Tests**: Validate dynamic graph and event sourcing implementations
- **Benchmarks**: Performance testing for operator operations

All tests use xUnit framework with FluentAssertions for readable assertions.

## License

This project is licensed under the Apache License 2.0. See the [LICENSE](LICENSE) file for details.

## Author

**Mateus Yonathan**

## Contributing

Contributions are welcome! Please ensure that:
- All axioms are validated for new operators
- Unit tests are added for new functionality
- Code follows the existing style and structure
- All tests pass before submitting

## Acknowledgments

SNOA implements proofs via programming (property-based testing) to validate mathematical axioms through code execution, providing reproducible and verifiable validation of the algebraic structure.

