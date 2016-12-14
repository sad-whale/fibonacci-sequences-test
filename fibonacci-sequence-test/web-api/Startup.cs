using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EasyNetQ;
using common_types.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using NLog.Extensions.Logging;

namespace web_api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public Autofac.IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public System.IServiceProvider ConfigureServices(IServiceCollection services)
        {
            
            // Add framework services.
            services.AddMvc();

            //конфигурируем autofac контейнер
            var builder = new ContainerBuilder();
            
            builder.Register(x => RabbitHutch.CreateBus("host=localhost")).As<IBus>().SingleInstance();
            builder.RegisterType<NumberProcessor>().As<INumberProcessor>().SingleInstance();
            builder.RegisterType<RabbitSender>().As<INumberSender>();
            builder.Populate(services);

            this.ApplicationContainer = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            //подключаем nlog
            loggerFactory.AddNLog();
            //конфигурируем nlog
            env.ConfigureNLog("nlog.config");

            app.UseMvc();
        }
    }
}
