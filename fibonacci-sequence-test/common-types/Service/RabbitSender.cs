using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common_types.Models;
using EasyNetQ;

namespace common_types.Services
{
    //реализация отправителя посредством rabbit и easynetq
    public class RabbitSender : INumberSender
    {
        private IBus _bus;

        public RabbitSender(IBus bus)
        {
            _bus = bus;
        }

        public void Send(FibonacciNumber number)
        {
            _bus.Send<FibonacciNumber>(number.SequenceId, number);
        }
    }
}
