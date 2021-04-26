using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ND.MessageBroker.RabbitMQ.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly ILogger<TestController> _logger;

        private readonly IPublishingService _publishingService;

        public TestController(ILogger<TestController> logger, IPublishingService publishingService)
        {
            _logger = logger;
            _publishingService = publishingService;
        }

        [HttpGet]
        public async Task<bool> Get()
        {
            while (true) //publish infinite messages
            {
                try
                {
                    _publishingService.PublishMessage(RandomString(10), "Test", "Message");
                    _publishingService.PublishMessage(RandomString(10), "Test2", "Message");
                    Thread.Sleep(2000);
                }
                catch (Exception e)
                {
                    _logger.LogError("Error to publish message");
                }
            }
            return true;
        }

        private string RandomString(int length)
        {

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

           return new String(stringChars);
        }

    }
}
