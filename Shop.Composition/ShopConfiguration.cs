using System;

using GridDomain.Common;
using GridDomain.EventSourcing.Sagas;
using GridDomain.Node.Actors;
using GridDomain.Node.Configuration.Composition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.Unity;
using Shop.Domain.Aggregates.AccountAggregate;
using Shop.Domain.Aggregates.OrderAggregate;
using Shop.Domain.Aggregates.SkuAggregate;
using Shop.Domain.Aggregates.SkuStockAggregate;
using Shop.Domain.Aggregates.UserAggregate;
using Shop.Domain.DomainServices.PriceCalculator;
using Shop.Domain.Sagas;
using Shop.Infrastructure;
using Shop.ReadModel.Context;
using Shop.ReadModel.DomanServices;
using Account = Shop.Domain.Aggregates.AccountAggregate.Account;
using Order = Shop.Domain.Aggregates.OrderAggregate.Order;
using Sku = Shop.Domain.Aggregates.SkuAggregate.Sku;
using SkuStock = Shop.Domain.Aggregates.SkuStockAggregate.SkuStock;
using User = Shop.Domain.Aggregates.UserAggregate.User;

namespace Shop.Composition
{
    public class ShopConfiguration : IContainerConfiguration
    {
        private readonly DbContextOptions<ShopDbContext> _readModelContextOptions;

        public ShopConfiguration(DbContextOptions<ShopDbContext> readModelContextOptions = null)
        {
            _readModelContextOptions = readModelContextOptions
                                       ?? new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(
                                                                                                    "Server = (local); Database = Shop; Integrated Security = true; MultipleActiveResultSets = True")
                                                                                      .Options;
            ;
        }

        public void Register(IUnityContainer container)
        {
            container.RegisterInstance<Func<ShopDbContext>>(() => new ShopDbContext(_readModelContextOptions));
            container.RegisterType<ISkuPriceQuery, SkuPriceQuery>(new ContainerControlledLifetimeManager());
            container.RegisterType<IPriceCalculator, SqlPriceCalculator>(new ContainerControlledLifetimeManager());

            container.Register(AggregateConfiguration.New<Account, AccountCommandsHandler>());
            container.Register(AggregateConfiguration.New<Order, OrderCommandsHandler>());
            container.Register(AggregateConfiguration.New<Sku, SkuCommandsHandler>());
            container.Register(AggregateConfiguration.New<SkuStock, SkuStockCommandsHandler>());
            container.Register(AggregateConfiguration.New<User, UserCommandsHandler>());
            container.RegisterType<ISequenceProvider, SqlSequenceProvider>();
            container.RegisterType<IPriceCalculator, SqlPriceCalculator>();
            container.Register(SagaConfiguration.New(new DefaultSagaDependencyFactory<BuyNow, BuyNowState>(((Func<ISagaCreator<BuyNowState>>) (() => container.Resolve<BuyNowSagaFactory>()))(), BuyNow.Descriptor)));
        }
    }
}