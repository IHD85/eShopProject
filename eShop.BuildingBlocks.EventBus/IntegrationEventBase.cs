using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.BuildingBlocks.EventBus
{
    public abstract class IntegrationEventBase
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public DateTime CreationDate { get; private set; } = DateTime.UtcNow;
    }
}
