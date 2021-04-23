# MessageBroker.RabbitMQ
RabbitMQ implementation

Install nuget package:
  ```
  npm install NinjaDevelopers.MessageBroker.RabbitMQ	
  ```

# Configuration 

In your appsettings.json add this section to configure:
```json
"RabbitConfig": {
    "username": "username",
    "password": "pass",
    "hostname": "yourhost",
    "port": "yourport",
    "virtualhost": "/",
    "subscriptions": [
      {
        "name": "queue_to_suscribe_name",
        "exchange": "exchange to suscribe name"
      }
    ]
  }
  ```
  
  # How use 

  To subscribe to exchange or queue
  
  Create a service class inheriting from base ND.MessageBroker.Services.SubscriptionService:
  
  ```c#
  public class SubscriptionService : ND.MessageBroker.Services.SubscriptionService
    {


        public SubscriptionService(IBrokerConfiguration configuration, ILogger<ND.MessageBroker.Services.SubscriptionService> logger : base(configuration, logger)
        {
            
        }

        public override async Task Process(BasicDeliverEventArgs ea, object model)
        {
            //Do something and use a.Routing Key to segregate subscribed queues
        }
    }
  ```
  
  In our startup ConfigureServices add config service:
  
  ```c#
  services.AddTransient<IBrokerConfiguration>(x => Configuration.GetSection("RabbitConfig").Get<BrokerConfiguration>()); //add configuration
  
  services.AddHostedService<SubscriptionService>(); //add hosted service
   ```
  
  
  
