using Autofac;
using common_types.Services;
using EasyNetQ;
using master_app.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace master_app
{
    public class Program
    {
        public static IConfigurationRoot Configuration;
        public static Autofac.IContainer ApplicationContainer;

        private static Logger _logger;

        public static void Main(string[] args)
        {
            //создание логгера nlog
            _logger = LogManager.GetCurrentClassLogger();

            //создание объекта конфигурации на основе json файла
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

            Configuration = builder.Build();

            //конфигурирование autofac контейнера
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<HttpSender>(x => new HttpSender("http://localhost:5000/api/number")).As<INumberSender>();
            containerBuilder.Register(x => RabbitHutch.CreateBus("host=localhost")).As<IBus>().SingleInstance();
            containerBuilder.RegisterType<NumberProcessor>().As<INumberProcessor>().SingleInstance();
            containerBuilder.RegisterType<SequenceWorker>().As<SequenceWorker>();
            ApplicationContainer = containerBuilder.Build();

            //резолвим воркер и шину рэббита
            SequenceWorker worker = ApplicationContainer.Resolve<SequenceWorker>();
            IBus bus = ApplicationContainer.Resolve<IBus>();
            
            int sequencesNumber;
            //считываем из конфига количество последовательностей и запускаем
            if (int.TryParse(Configuration["sequencesNumber"], out sequencesNumber))
            {
                for (int i = 0; i < sequencesNumber; i++)
                {
                    //в качестве идентификаторов последовательности используются гуиды
                    string guid = Guid.NewGuid().ToString();
                    //запуск расчета последовательности
                    worker.StartSequence(guid);
                    Task.Delay(1000).Wait();
                }
                _logger.Info($"{sequencesNumber} sequences started.");
            }
            else
            {
                _logger.Error("Can not read number of sequences from config file.");
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
            worker.Dispose();
            bus.Dispose();
        }
    }


}
