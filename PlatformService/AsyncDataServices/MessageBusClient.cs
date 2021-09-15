
using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration configuration;
        private readonly IConnection connection;
        private readonly IModel channel;

        public MessageBusClient(IConfiguration configuration)
        {
            this.configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMqHost"],
                Port = Convert.ToInt32(configuration["RabbitMqPort"])
            };

            try
            {
                 connection = factory.CreateConnection();
                 channel = connection.CreateModel();
                 
                 channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                 connection.ConnectionShutdown += RabbitMq_ConnectionShutdown;

                 Console.WriteLine("Connected to message bus");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"--> Could not connect to message bus: {ex.Message}");
            }
        }

        private void RabbitMq_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> Rabbit MQ connection shutdown");
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (connection.IsOpen)
            {
                Console.WriteLine("--> Rabbit MQ connection is open, sending message...");

                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> Rabbit MQ connection is closed, not sending message...");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "trigger", routingKey: "", basicProperties: null, body: body);

            Console.WriteLine($"--> We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("Message bus disposed");

            if (channel.IsOpen)
            {
                channel.Close();
                connection.Close();
            }
        }
    }
}