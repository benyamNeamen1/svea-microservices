using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Svea.UserService.Models;
using Svea.UserService.Services;

namespace Svea.UserService.Messaging
{
    public class UserServiceRabbitMq : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;

        public UserServiceRabbitMq(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting RabbitMQ listener...");

            var factory = new ConnectionFactory
            {
                
                HostName = "rabbitmq",//"localhost",
                UserName = "guest",
                Password = "guest",
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,            
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5) 
            };

            bool connected = false;

            while (!connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    
                    _channel.ExchangeDeclare("user.validation.exchange", ExchangeType.Direct, durable: true);
                    _channel.QueueDeclare("user.validation.requests", durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind("user.validation.requests", "user.validation.exchange", "validate");


                    connected = true;
                    Console.WriteLine("RabbitMQ connected.");
                }
                catch
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    Console.WriteLine("RabbitMQ not ready, retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null || _channel.IsClosed)
            {
                Console.WriteLine("RabbitMQ channel lost, attempting reconnect...");
                StartAsync(stoppingToken);
            }

            SetupConsumer();

            Console.WriteLine("RabbitMQ user validation listener started.");
            return Task.CompletedTask;
        }

        private void SetupConsumer()
        {
            if (_channel == null || _channel.IsClosed)
                return;

            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicQos(0, 1, false);

            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var msg = JsonSerializer.Deserialize<UserValidationMessage>(
                        Encoding.UTF8.GetString(ea.Body.ToArray()));

                    bool result = false;

                    using var scope = _scopeFactory.CreateScope();
                    var validator = scope.ServiceProvider.GetRequiredService<IUserValidationService>();

                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            result = validator.CheckUser(msg.userId, msg.companyId);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"DB retry {i + 1}: {ex.Message}");
                            await Task.Delay(200 * (i + 1));
                        }
                    }

                    var responseJson = JsonSerializer.Serialize(new { valid = result });

                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                    if (!string.IsNullOrEmpty(ea.BasicProperties.ReplyTo))
                    {
                        _channel.BasicPublish(
                            exchange: "",
                            routingKey: ea.BasicProperties.ReplyTo,
                            basicProperties: replyProps,
                            body: Encoding.UTF8.GetBytes(responseJson));
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RabbitMQ handler error: " + ex.Message);
                    _channel?.BasicAck(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume("user.validation.requests", autoAck: false, consumer: consumer);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping RabbitMQ listener...");

            _channel?.Close();
            _connection?.Close();

            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
