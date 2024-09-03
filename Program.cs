using azureservicebusdemo.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System;

namespace azureservicebusdemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        var configuration = context.Configuration;

                        // RabbitMQ configuration
                        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                        var factory = new ConnectionFactory
                        {
                            HostName = rabbitMqConfig["HostName"],
                            Port = int.Parse(rabbitMqConfig["Port"]),
                            UserName = rabbitMqConfig["UserName"],
                            Password = rabbitMqConfig["Password"]
                        };
                        services.AddSingleton(factory);

                        // Register RabbitMQ services
                        services.AddScoped<RabbitMQProducerService>();
                        services.AddScoped<IRabbitMQConsumerService>(sp => new RabbitMQConsumerService(factory, sp.GetRequiredService<ILogger<RabbitMQConsumerService>>()));

                        //services.AddScoped<IRabbitMQConsumerService, RabbitMQConsumerService>();

                        // Register other services
                        services.AddScoped<IServiceBusService, ServiceBusService>();
                        services.AddControllers();
                        services.AddSwaggerGen(c =>
                        {
                            c.SwaggerDoc("v1", new OpenApiInfo { Title = "azureservicebusdemo", Version = "v1" });
                        });
                    })
                    .Configure((context, app) =>
                    {
                        var env = context.HostingEnvironment;

                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                            app.UseSwagger();
                            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "azureservicebusdemo v1"));
                        }

                        app.UseHttpsRedirection();
                        app.UseRouting();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
                });
    }
}
