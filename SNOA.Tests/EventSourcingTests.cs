using System;
using System.Collections.Generic;
using System.Linq;
using SNOA.Core;
using SNOA.Core.CaseStudies.EventSourcing;
using Xunit;

namespace SNOA.Tests
{
    /// <summary>
    /// Tests for Event Sourcing Case Study
    /// </summary>
    public class EventSourcingTests
    {
        [Fact]
        public void AppendEventLeftOperator_OrderCreated_ShouldCreateOrder()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            var createdEvent = new OrderCreatedEvent
            {
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow
            };
            var appendOperator = new AppendEventLeftOperator(createdEvent);

            // Act
            var result = appendOperator.Apply(orderObject);

            // Assert
            Assert.Equal(orderId, result.Value.OrderId);
            Assert.Equal(OrderStatus.Created, result.Value.Status);
            Assert.Equal(1, result.State.CurrentVersion);
            Assert.Single(result.State.Events);
            Assert.Equal(1, result.Properties["eventCount"]);
        }

        [Fact]
        public void AppendEventLeftOperator_ItemAdded_ShouldAddItem()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var orderAfterCreated = appendCreated.Apply(orderObject);

            var itemEvent = new ItemAddedEvent
            {
                ItemId = "ITEM-001",
                Name = "Test Item",
                Price = 10.50m,
                Quantity = 2
            };
            var appendItem = new AppendEventLeftOperator(itemEvent);

            // Act
            var result = appendItem.Apply(orderAfterCreated);

            // Assert
            Assert.Single(result.Value.Items);
            Assert.Equal(21.00m, result.Value.TotalAmount); // 10.50 * 2
            Assert.Equal(2, result.State.CurrentVersion);
            Assert.Equal(2, result.State.Events.Count);
        }

        [Fact]
        public void ReplayEventsLeftOperator_ShouldRebuildState()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            // Add multiple events
            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var orderAfterCreated = appendCreated.Apply(orderObject);

            var itemEvent1 = new ItemAddedEvent { ItemId = "ITEM-001", Name = "Item 1", Price = 10m, Quantity = 1 };
            var appendItem1 = new AppendEventLeftOperator(itemEvent1);
            var orderAfterItem1 = appendItem1.Apply(orderAfterCreated);

            var itemEvent2 = new ItemAddedEvent { ItemId = "ITEM-002", Name = "Item 2", Price = 20m, Quantity = 1 };
            var appendItem2 = new AppendEventLeftOperator(itemEvent2);
            var orderAfterItem2 = appendItem2.Apply(orderAfterItem1);

            var replayOperator = new ReplayEventsLeftOperator();

            // Act
            var result = replayOperator.Apply(orderAfterItem2);

            // Assert
            Assert.Equal(2, result.Value.Items.Count);
            Assert.Equal(30m, result.Value.TotalAmount);
            Assert.True((bool)result.Properties["replayed"]);
            Assert.True(result.State.IsReplaying);
        }

        [Fact]
        public void NormalizeEventLogRightOperator_ShouldRemoveDuplicates()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var orderAfterCreated = appendCreated.Apply(orderObject);

            // Add duplicate event (same EventId)
            var duplicateEvent = new OrderCreatedEvent { EventId = createdEvent.EventId, OrderId = orderId };
            var appendDuplicate = new AppendEventLeftOperator(duplicateEvent);
            var orderWithDuplicate = appendDuplicate.Apply(orderAfterCreated);

            var normalizeOperator = new NormalizeEventLogRightOperator();

            // Act
            var result = normalizeOperator.Apply(orderWithDuplicate);

            // Assert
            Assert.True((bool)result.Properties["normalized"]);
            Assert.Equal(1, result.State.Events.Count); // Duplicate removed
        }

        [Fact]
        public void CommitSnapshotRightOperator_ShouldKeepRecentEvents()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            // Add 15 events
            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var current = appendCreated.Apply(orderObject);

            for (int i = 0; i < 14; i++)
            {
                var itemEvent = new ItemAddedEvent { ItemId = $"ITEM-{i}", Name = $"Item {i}", Price = 10m, Quantity = 1 };
                var appendItem = new AppendEventLeftOperator(itemEvent);
                current = appendItem.Apply(current);
            }

            var commitOperator = new CommitSnapshotRightOperator(keepRecentEvents: 10);

            // Act
            var result = commitOperator.Apply(current);

            // Assert
            Assert.True((bool)result.Properties["committed"]);
            Assert.Equal(10, result.State.Events.Count); // Only 10 recent events kept
            Assert.Equal(5, (int)result.Properties["clearedEvents"]); // 5 events cleared
        }

        [Fact]
        public void ValidateEventLogRightOperator_ShouldValidateEventLog()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var orderAfterCreated = appendCreated.Apply(orderObject);

            var validateOperator = new ValidateEventLogRightOperator();

            // Act
            var result = validateOperator.Apply(orderAfterCreated);

            // Assert
            Assert.True((bool)result.Properties["validated"]);
            Assert.Equal("PASS", result.Properties["validationResult"]);
        }

        [Fact]
        public void SnapshotLeftOperator_ShouldCreateSnapshot()
        {
            // Arrange
            var orderId = "ORDER-001";
            var initialState = new OrderState(orderId);
            var properties = new Dictionary<string, object> { ["eventCount"] = 0 };
            var eventLog = new EventLog();
            var orderObject = new SNOAObject<OrderState, EventLog>(initialState, properties, eventLog);

            var createdEvent = new OrderCreatedEvent { OrderId = orderId, CreatedAt = DateTime.UtcNow };
            var appendCreated = new AppendEventLeftOperator(createdEvent);
            var orderAfterCreated = appendCreated.Apply(orderObject);

            var snapshotOperator = new SnapshotLeftOperator();

            // Act
            var result = snapshotOperator.Apply(orderAfterCreated);

            // Assert
            Assert.True((bool)result.Properties["hasSnapshot"]);
            Assert.Equal(1, result.Properties["snapshotVersion"]);
            Assert.NotNull(result.Properties["snapshotTime"]);
        }
    }
}

