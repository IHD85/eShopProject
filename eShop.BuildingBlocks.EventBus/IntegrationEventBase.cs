using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShop.BuildingBlocks.EventBus
{
    public class IntegrationEventBase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreationDate { get; init; } = DateTime.UtcNow;
    }
}

