
namespace ND.MessageBroker.RabbitMQ.Contracts
{
    public interface IPublishingService
    {
        void PublishMessage(object message, string exchange, string routingKey);
    }
}
