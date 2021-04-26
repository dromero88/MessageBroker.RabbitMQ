using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ND.MessageBroker.RabbitMQ.Contracts;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiTest.Subscriptions
{
    public class SubscriptionService : ND.MessageBroker.Services.SubscriptionService
    {
        public SubscriptionService(IBrokerConfiguration configuration, ILogger<ND.MessageBroker.Services.SubscriptionService> logger) : base(configuration, logger)
        {
            
        }

        public override async Task Process(BasicDeliverEventArgs ea, object model)
        {
            var body = ea.Body.ToArray();

            var message = Encoding.UTF8.GetString(body);

            base._logger.LogInformation($"Message recived: {message}, routingKey => {ea.RoutingKey}, exchange => {ea.Exchange}");
        }
    }
}
