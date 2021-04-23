using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ND.MessageBroker.RabbitMQ.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

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
            _publishingService.PublishMessage("Hola", "Test", "Message"); 
            return true;
        }
    }
}
