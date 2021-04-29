using Microsoft.Extensions.Logging;
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

        public readonly ILogger<PublishingService> _logger;

        public PublishingService(IBrokerConfiguration configuration, ILogger<PublishingService> logger)
        {
            _logger = logger;

            connectionFactory = new ConnectionFactory
            {
                UserName = configuration.Username,
                Password = configuration.Password,
                VirtualHost = configuration.VirtualHost,
                HostName = configuration.HostName,
                Port = configuration.Port?? 5672
            };
        }

        public void PublishMessage(object message, string exchange, string routingKey)
        {
            try
            {
                using (var conn = connectionFactory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        try //Si falla intentamos eliminar y crear
                        {
                            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
                        }
                        catch(Exception e)
                        {
                            _logger.LogError($"Error creating exchange {exchange}: {e.Message}. Try to drop exchange and recreate");
                            channel.ExchangeDelete(exchange);
                            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
                            _logger.LogInformation($"Created/Conected exchange {exchange}");
                        }

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
            catch(Exception e)
            {
                _logger.LogError($"Error to publish message to exchange {exchange}: {e.Message}");
            }
        }
    }
}
