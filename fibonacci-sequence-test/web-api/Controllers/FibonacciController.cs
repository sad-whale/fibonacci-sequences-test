using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using common_types.Models;
using common_types.Services;

namespace web_api.Controllers
{
    public class FibonacciController : Controller
    {
        private INumberSender _sender;
        private INumberProcessor _processor;

        public FibonacciController(INumberSender sender, INumberProcessor processor)
        {
            _sender = sender;
            _processor = processor;
        }

        [HttpPost("api/number")]
        public async void Post([FromBody]FibonacciNumber value)
        {
            await Task.Delay(2000);
            var next = _processor.GetNext(value);
            _sender.Send(next);
        }
    }
}
