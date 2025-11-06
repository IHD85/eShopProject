using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace RabbitMQEventBus.Abstractions
{
    public class EventBusSubscriptionInfo
    {
        public Dictionary<string, Type> EventTypes { get; } = [];

        public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);

        internal static readonly JsonSerializerOptions DefaultSerializerOptions = new()
        {
            TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault ? CreateDefaultTypeResolver() : JsonTypeInfoResolver.Combine()
        };

        private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
            => new DefaultJsonTypeInfoResolver();

    }
}
