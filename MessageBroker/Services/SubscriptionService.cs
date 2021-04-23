
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ND.MessageBroker.RabbitMQ.Configurations;
using ND.MessageBroker.RabbitMQ.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ND.MessageBroker.Services
{
    public class SubscriptionService :  BackgroundService
    {
        private readonly ConnectionFactory connectionFactory;

        private readonly ILogger<SubscriptionService> _logger;

        private readonly IBrokerConfiguration _configuration;

        private readonly IConnection _connection;

        private readonly IModel _channel;

        public SubscriptionService(IBrokerConfiguration configuration, ILogger<SubscriptionService> logger)
        {
            _logger = logger;

            _configuration = configuration;

            connectionFactory = new ConnectionFactory
            {
                UserName = _configuration.Username,
                Password = _configuration.Password,
                VirtualHost = _configuration.VirtualHost,
                HostName = _configuration.HostName,
                Port = _configuration.Port
            }; 

            try
            {
                _connection = connectionFactory.CreateConnection();

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                _channel = _connection.CreateModel();

                _channel.BasicQos(0, 1, true);

                //Create queues and binds

                foreach (QueueConfiguration queue in _configuration.Subscriptions)
                {
                    _channel.QueueDeclare(
                                    queue: queue.Name,
                                    durable: false,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null
                                );

                    _channel.QueueBind(queue: queue.Name,
                      exchange: queue.Exchange,
                      routingKey: queue.RoutingKey);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

        }


        public virtual async Task Process(BasicDeliverEventArgs ea, object? model)
        {
            _logger.LogTrace("Process Task");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (ch, ea) =>
                {
                    try
                    {
                        await Process(ea, ch);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                    }
                };

                consumer.Shutdown += OnConsumerShutdown;
                consumer.Registered += OnConsumerRegistered;
                consumer.Unregistered += OnConsumerUnregistered;
                consumer.ConsumerCancelled += OnConsumerCancelled;

                foreach (QueueConfiguration queue in _configuration.Subscriptions)
                    _channel.BasicConsume(queue.Name, false, consumer);

                return Task.CompletedTask;

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Task.CompletedTask;
            }
        }

        private void OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
