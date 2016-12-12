using common_types.Models;
using common_types.Services;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace master_app.Workers
{
    public class SequenceWorker : IDisposable
    {
        private string _sequenceId;
        private IBus _bus;
        private IDisposable _messageReceiver;
        private INumberProcessor _processor;
        private INumberSender _sender;

        public SequenceWorker(IBus bus, INumberProcessor processor, INumberSender sender)
        {
            _bus = bus;
            _processor = processor;
            _sender = sender;
        }

        public void StartSequence(string seqNum)
        {
            _sequenceId = seqNum;
            _messageReceiver = _bus.Receive<FibonacciNumber>(seqNum, number =>
            {
                Console.WriteLine($"{number.SequenceId}: {number.Number}");
                var next = _processor.GetNext(number);
                Task.Delay(2000).Wait();
                Console.WriteLine($"{next.SequenceId}: {next.Number}");
                _sender.Send(next);
            });

            var first = _processor.GetNext(new FibonacciNumber() { SequenceId = seqNum, Number = 1 });
            Console.WriteLine($"{first.SequenceId}: {first.Number}");
            _sender.Send(first);
        }

        private void Print()
        {
            Console.Write(_sequenceId);
        }

        public void Dispose()
        {
            if (_messageReceiver != null)
                _messageReceiver.Dispose();
        }
    }
}
