
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
        private ConnectionFactory connectionFactory;

        private readonly IBrokerConfiguration _configuration;

        private IConnection _connection;

        private IModel _channel;

        private int ConsumerShutdownRecived = 0;

        public readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(IBrokerConfiguration configuration, ILogger<SubscriptionService> logger)
        {
            _logger = logger;

            if (configuration == null)
            {
                _logger.LogWarning("Rabbit configuration  not found");
                return;
            }

            _configuration = configuration;

            Connect();
        }

        #region conexionManagement
        
        public void Connect()
        {
            CreateConnection();

            CreateChannel();

            CreateExchangeQueuesAndBindings();           
        }

        private void CreateConnection()
        {
            try
            {
                connectionFactory = new ConnectionFactory
                {
                    UserName = _configuration.Username,
                    Password = _configuration.Password,
                    VirtualHost = _configuration.VirtualHost,
                    HostName = _configuration.HostName,
                    Port = _configuration.Port
                };

                _connection = connectionFactory.CreateConnection();

                _connection.ConnectionShutdown += OnConnectionShutdown;

                _logger.LogInformation($"Connected to RabbitMQ server {_configuration.HostName}:{_configuration.Port}. (=^_^=)");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to create conexion to host {_configuration.HostName}:{_configuration.Port}, user {_configuration.Username} : {e.Message}. Try to reconect...");
                Thread.Sleep(5000);
                Connect();
            }
        }

        private void CreateExchangeQueuesAndBindings()
        {
            if (_channel == null) return;
            
                foreach (QueueConfiguration queue in _configuration.Subscriptions)
                    try
                    {
                        _channel.ExchangeDeclare(exchange: queue.Exchange, type: ExchangeType.Fanout, durable: true);

                        _channel.QueueDeclare(
                                        queue: queue.Name,
                                        durable: true,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null
                                    );

                        _channel.QueueBind(queue: queue.Name,
                          exchange: queue.Exchange,
                          routingKey: queue.RoutingKey);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Failed to create or binding queue {queue.Name}, exchange {queue.Exchange} and routing key {queue.RoutingKey} : {e.Message}");
                    }
        }

        private void CreateChannel()
        {
            if (_connection == null) return;

            try
            {
                _channel = _connection.CreateModel();

                _channel.BasicQos(0, 1, true);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to create channel: {e.Message}");
            }
        }

        public void Cleanup()
        {
            try
            {
                _connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to cleanup connexion {_configuration.HostName}:{_configuration.Port}, user {_configuration.Username} : {e.Message}");
            }
        }

        private void CleanupAndReconnect()
        {
            Cleanup();

            Connect();
        }

        private void CreateConsumer()
        {
            try
            {
                foreach (QueueConfiguration queue in _configuration.Subscriptions)
                {
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (ch, ea) =>
                    {
                        try
                        {
                            await Process(ea, ch);
                            if (_channel != null)
                                _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                        }

                        
                    };

                    if(_channel != null)
                        _channel.BasicConsume(queue.Name, false, consumer);

                    consumer.Shutdown += OnConsumerShutdown;
                    consumer.Registered += OnConsumerRegistered;
                    consumer.Unregistered += OnConsumerUnregistered;
                    consumer.ConsumerCancelled += OnConsumerCancelled;

                    //break;
                }
            } 
            catch (Exception e)
            {
                _logger.LogError($"Creating subscription consumer: {e.Message}. Try to reconect and recreate consumer...");

                Thread.Sleep(5000);

                CleanupAndReconnect();

                CreateConsumer();
            }

        }

        #endregion

        #region events
        private void OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
            _logger.LogError("ConsumerCancelled");
        }

        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogError("ConsumerUnregistered");
        }

        private void OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
            _logger.LogInformation("ConsumerRegistered");
        }

        private void OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogError($"ConsumerShutdown: {e.ReplyText}");

            ConsumerShutdownRecived += 1;

            if(ConsumerShutdownRecived == _configuration.Subscriptions.Count)
            {
                ConsumerShutdownRecived = 0;
                CreateConsumer();
            }
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogError("ConnectionShutdown");

            if (!_connection.IsOpen)
            {
                CleanupAndReconnect();
            }
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
        #endregion

        public virtual async Task Process(BasicDeliverEventArgs ea, object? model)
        {
            _logger.LogTrace("Process Task");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_channel == null)
                    throw new Exception("Channel break");

                stoppingToken.ThrowIfCancellationRequested();

                CreateConsumer();

                return Task.CompletedTask;

            }
            catch (Exception e)
            {
                _logger.LogError("Error to subscribe:" + e.Message);

                return Task.CompletedTask;
            }
        }
    }
}
