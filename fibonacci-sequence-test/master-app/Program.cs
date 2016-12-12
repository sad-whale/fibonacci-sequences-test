using Autofac;
using common_types.Services;
using EasyNetQ;
using master_app.Workers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace master_app
{
    public class Program
    {
        public static IConfigurationRoot Configuration;
        public static Autofac.IContainer ApplicationContainer;

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

            Configuration = builder.Build();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register<HttpSender>(x => new HttpSender("http://localhost:5000/api/number")).As<INumberSender>();
            containerBuilder.Register(x => RabbitHutch.CreateBus("host=localhost")).As<IBus>().SingleInstance();
            containerBuilder.RegisterType<NumberProcessor>().As<INumberProcessor>();
            containerBuilder.RegisterType<SequenceWorker>().As<SequenceWorker>();
            ApplicationContainer = containerBuilder.Build();

            List<SequenceWorker> workers = new List<SequenceWorker>();
            int sequencesNumber;

            if (int.TryParse(Configuration["sequencesNumber"], out sequencesNumber))
            {
                for (int i = 0; i < sequencesNumber; i++)
                {
                    string guid = Guid.NewGuid().ToString();
                    Task.Run(() =>
                    {
                        var worker = ApplicationContainer.Resolve<SequenceWorker>();
                        worker.StartSequence(guid);
                        workers.Add(worker);
                    });
                }
                Console.WriteLine($"{sequencesNumber} tasks started.");
            }
            else
            {
                Console.WriteLine("Can not read number of sequences from config file.");
            }

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }


}
