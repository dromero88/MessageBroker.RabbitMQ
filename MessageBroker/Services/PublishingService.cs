using ND.MessageBroker.RabbitMQ.Contracts;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;


namespace ND.MessageBroker.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly ConnectionFactory connectionFactory;

        public PublishingService(IBrokerConfiguration configuration)
        {
            connectionFactory = new ConnectionFactory
            {
                UserName = configuration.Username,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                HostName = configuration.HostName,
                Port = configuration.Port
            };
        }

        public void PublishMessage(object message, string exchange, string routingKey)
        {
            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout, durable: true);

                    var jsonPayload = JsonConvert.SerializeObject(message);

                    var body = Encoding.UTF8.GetBytes(jsonPayload);

                    channel.BasicPublish(exchange: exchange,
                        routingKey: routingKey,
                        basicProperties: null,
                        body: body
                    );
                }
            }
        }
    }
}
