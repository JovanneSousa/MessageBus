using Messages;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace MessageBus.Bus
{
    public interface IEventRouteResolver
    {
        (string exchange, string routingKey) Resolve(IntegrationEvent @event);
    }
}
