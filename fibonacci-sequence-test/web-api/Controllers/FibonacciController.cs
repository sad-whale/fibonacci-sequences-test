using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using common_types.Models;
using common_types.Services;
using Microsoft.Extensions.Logging;

namespace web_api.Controllers
{
    public class FibonacciController : Controller
    {
        private ILogger _logger;
        private INumberSender _sender;
        private INumberProcessor _processor;

        //внедрение зависимостей
        public FibonacciController(INumberSender sender, INumberProcessor processor, ILoggerFactory loggerFactory)
        {
            _sender = sender;
            _processor = processor;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        [HttpPost("api/number")]
        public async void Post([FromBody]FibonacciNumber value)
        {
            _logger.LogInformation($"{value.SequenceId}: Received {value.Number}");
            await Task.Delay(1000);
            //генерируем новое число на основе полученного
            var next = _processor.GetNext(value);
            _logger.LogInformation($"{value.SequenceId}: Generated {value.Number}");
            //и отправляем его
            _sender.Send(next);
        }
    }
}
