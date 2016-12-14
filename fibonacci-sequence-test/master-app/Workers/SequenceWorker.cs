using common_types.Models;
using common_types.Services;
using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace master_app.Workers
{
    public class SequenceWorker : IDisposable
    {
        private Logger _logger;

        private HashSet<IDisposable> _receivers;

        private IBus _bus;
        private INumberProcessor _processor;
        private INumberSender _sender;

        //внедряем зависимости
        public SequenceWorker(IBus bus, INumberProcessor processor, INumberSender sender)
        {
            _bus = bus;
            _processor = processor;
            _sender = sender;
            _receivers = new HashSet<IDisposable>();
            _logger = LogManager.GetCurrentClassLogger();
        }

        //запуск расчета последовательности с заданным идентификатором
        public Task StartSequence(string seqNum)
        {
            return Task.Run(() =>
            {
                //подписываемся на очередь рэббита с идентификатором, соответствующим идентификатору последовательности
                //на каждое сообщение генерится таск
                var receiver = _bus.Receive<FibonacciNumber>(seqNum, number => Task.Run(()=>
                {
                    _logger.Info($"{number.SequenceId}: Generated {number.Number}");
                    //генерируем следующее число
                    var next = _processor.GetNext(number);
                    _logger.Info($"{next.SequenceId}: Received {next.Number}");
                    try
                    {
                        Task.Delay(1000).Wait();
                        //отправляем
                        _sender.Send(next);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"{next.SequenceId}: Failed to send the number");
                    }
                }));

                //генерируем начало последоавтельности
                var first = _processor.GetNext(new FibonacciNumber() { SequenceId = seqNum, Number = 1 });
                _logger.Info($"{first.SequenceId}: Generated {first.Number}");
                try
                {
                    //и отправляем
                    _sender.Send(first);
                    _receivers.Add(receiver);
                }
                catch (Exception ex)
                {
                    receiver.Dispose();
                    _logger.Error(ex, $"{seqNum}: Failed to send the number");
                }
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
