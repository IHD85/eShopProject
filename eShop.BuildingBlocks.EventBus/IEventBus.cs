using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.BuildingBlocks.EventBus
{
    public interface IEventBus
    {
        void Publish(IntegrationEventBase @event);
        void Subscribe<T, TH>()
            where T : IntegrationEventBase
            where TH : IIntegrationEventHandler<T>;
    }


    public interface IIntegrationEventHandler<TIntegrationEvent>
    {
        Task Handle(TIntegrationEvent @event);
    }
}

