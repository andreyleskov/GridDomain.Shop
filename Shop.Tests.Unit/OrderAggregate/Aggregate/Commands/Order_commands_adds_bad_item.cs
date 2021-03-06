using System;
using System.Threading.Tasks;
using GridDomain.Tests.Common;
using NMoneys;
using Shop.Domain.Aggregates.OrderAggregate;
using Shop.Domain.Aggregates.OrderAggregate.Commands;
using Shop.Domain.Aggregates.OrderAggregate.Events;
using Shop.Domain.Aggregates.OrderAggregate.Exceptions;
using Shop.Infrastructure;
using Xunit;

namespace Shop.Tests.Unit.OrderAggregate.Aggregate.Commands
{
    public class Order_commands_adds_bad_item
    {
        private AggregateScenario<Order> GivenNewOrder()
        {
            var scenario = AggregateScenario.New(new OrderCommandsHandler(new InMemorySequenceProvider()));
            scenario.Given(new OrderCreated(Guid.NewGuid(), 123, Guid.NewGuid()));
            return scenario;
        }

        [Fact]
        public async Task Order_should_throw_ex�eption_on_item_add_with_negative_money()
        {
            var scenario = GivenNewOrder();
            await scenario.When(new AddItemToOrderCommand(scenario.Aggregate.Id,
                                                          Guid.NewGuid(),
                                                          20,
                                                          new Money(-123)))
                          .Run()
                          .CommandShouldThrow<InvalidMoneyException>();
        }

        [Fact]
        public async Task Order_should_throw_ex�eption_on_item_add_with_negative_quantity()
        {
            var scenario = GivenNewOrder();
            await scenario.When(new AddItemToOrderCommand(scenario.Aggregate.Id,
                                                          Guid.NewGuid(),
                                                          -1,
                                                          new Money(123)))
                          .Run()
                          .CommandShouldThrow<InvalidQuantityException>();
        }
    }
}