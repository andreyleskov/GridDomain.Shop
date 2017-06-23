using System;
using GridDomain.Common;
using GridDomain.EventSourcing.FutureEvents;
using GridDomain.Tests.Common;
using Shop.Domain.Aggregates.SkuStockAggregate;
using Shop.Domain.Aggregates.SkuStockAggregate.Commands;
using Shop.Domain.Aggregates.SkuStockAggregate.Events;
using Xunit;

namespace Shop.Tests.Unit.XUnit.SkuStockAggregate.Aggregate
{
   
    public class SkuStock_reserve_renew_tests
    {
        public SkuStock_reserve_renew_tests()// Given_sku_stock_with_amount_and_reserve_When_renew_reserve()
        {
            var aggregateId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var reservationStartTime = BusinessDateTime.Now;
            var reserveTime = TimeSpan.FromMilliseconds(100);
            _expirationDate = reservationStartTime + reserveTime;

            _scenario =
                AggregateScenario.New<SkuStock, SkuStockCommandsHandler>()
                        .Given(new SkuStockCreated(aggregateId, Guid.NewGuid(), 50, reserveTime),
                               new StockAdded(aggregateId, 10, "test batch 2"),
                               _stockReservedEvent = new StockReserved(aggregateId, customerId, _expirationDate, 5),
                               new FutureEventScheduledEvent(Guid.NewGuid(),
                                                             aggregateId,
                                                             _expirationDate,
                                                             new ReserveExpired(aggregateId, customerId)))
                        .When(
                              _reserveStockCommand = new ReserveStockCommand(aggregateId, customerId, 10, reservationStartTime));

            _scenario.Run();
        }

        private AggregateScenario<SkuStock> _scenario;
        private DateTime _expirationDate;
        private ReserveStockCommand _reserveStockCommand;
        private StockReserved _stockReservedEvent;

        private Reservation NewReservation
        {
            get
            {
                var reserveStockCommand = _scenario.GivenCommand<ReserveStockCommand>();
                Reservation reservation;
                _scenario.Aggregate.Reservations.TryGetValue(reserveStockCommand.CustomerId, out reservation);
                return reservation;
            }
        }

       [Fact]
        public void Then_aggregate_reservation_quantity_is_sum_of_initial_and_new_reservations()
        {
            Assert.Equal(_reserveStockCommand.Quantity + _stockReservedEvent.Quantity, NewReservation.Quantity);
        }

       [Fact]
        public void Then_Aggregate_Reservation_should_have_new_expiration_time()
        {
            Assert.Equal(_expirationDate, NewReservation.ExpirationDate);
        }

       [Fact]
        public void Then_Aggregate_Reservation_should_remain_for_same_customer()
        {
            Assert.NotNull(NewReservation);
        }

       [Fact]
        public void Then_correct_events_should_be_raised()
        {
            var customerId = _stockReservedEvent.ReserveId;

            var sourceId = _scenario.Aggregate.Id;

            _scenario.Then(new FutureEventCanceledEvent(Any.GUID, sourceId),
                           new ReserveRenewed(sourceId, customerId),
                           new StockReserved(sourceId,
                                             customerId,
                                             _expirationDate,
                                             _reserveStockCommand.Quantity + _stockReservedEvent.Quantity),
                           new FutureEventScheduledEvent(Any.GUID, sourceId, _expirationDate, new ReserveExpired(sourceId, customerId)));

            _scenario.Check();
        }
    }
}