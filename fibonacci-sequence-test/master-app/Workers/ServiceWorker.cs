using common_types.Models;
using common_types.Services;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace master_app.Workers
{
    public class SequenceWorker : IDisposable
    {
        private HashSet<IDisposable> _receivers;

        private IBus _bus;
        private INumberProcessor _processor;
        private INumberSender _sender;

        public SequenceWorker(IBus bus, INumberProcessor processor, INumberSender sender)
        {
            _bus = bus;
            _processor = processor;
            _sender = sender;
            _receivers = new HashSet<IDisposable>();
        }

        public Task StartSequence(string seqNum)
        {
            return Task.Run(() =>
            {
                _receivers.Add(_bus.Receive<FibonacciNumber>(seqNum, number => Task.Run(()=>
                {
                    Console.WriteLine($"{number.SequenceId}: {number.Number} {Thread.CurrentThread.ManagedThreadId}");
                    var next = _processor.GetNext(number);
                    Console.WriteLine($"{next.SequenceId}: {next.Number} {Thread.CurrentThread.ManagedThreadId}");
                    _sender.Send(next);
                })));

                var first = _processor.GetNext(new FibonacciNumber() { SequenceId = seqNum, Number = 1 });
                Console.WriteLine($"{first.SequenceId}: {first.Number} {Thread.CurrentThread.ManagedThreadId}");
                _sender.Send(first);
            });            
        }
        
        public void Dispose()
        {
            foreach (var receiver in _receivers)
            {
                receiver.Dispose();
            }
        }
    }
}
