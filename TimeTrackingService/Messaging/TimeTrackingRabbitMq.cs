using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Svea.TimeTrackingService.Models;
using System.Text;
using System.Text.Json;

namespace Svea.TimeTrackingService.Messaging
{
    public class TimeTrackingRabbitMq : BackgroundService
    {
        private IConnection? _connection;
        private IModel? _channel;
        private string? _replyQueue;
        private EventingBasicConsumer? _consumer;
        private ConnectionFactory? _factory;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting TimeTracking RabbitMQ...");

            _factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            Connect();

            return base.StartAsync(cancellationToken);
        }

        private void Connect()
        {
            bool connected = false;

            while (!connected)
            {
                try
                {
                    _connection = _factory!.CreateConnection();
                    _channel = _connection.CreateModel();

                    SetupReplyQueue();

                    connected = true;
                    Console.WriteLine("TimeTracking RabbitMQ connected.");
                }
                catch
                {
                    Console.WriteLine("RabbitMQ not available, retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }
        }

        private void SetupReplyQueue()
        {
            _replyQueue = _channel!.QueueDeclare("", exclusive: true).QueueName;

            _consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(
                queue: _replyQueue,
                autoAck: true,
                consumer: _consumer
            );
        }

      
        private void EnsureConnection()
        {
            if (_factory == null)
                throw new InvalidOperationException("RabbitMQ factory not initialized");

            if (_connection == null || !_connection.IsOpen)
            {
                Console.WriteLine("Reconnecting TimeTrackingRabbitMq connection...");
                Connect();
            }

            if (_channel == null || !_channel.IsOpen)
            {
                Console.WriteLine("Recreating TimeTrackingRabbitMq channel...");
                _channel = _connection!.CreateModel();
                SetupReplyQueue();
            }
        }

        public async Task<bool> ValidateUserAsync(Guid userId, Guid companyId)
        {
            EnsureConnection();

            if (_channel == null || _consumer == null || _replyQueue == null)
                throw new InvalidOperationException("RabbitMQ not initialized");

            var correlationId = Guid.NewGuid().ToString();

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueue;

            var message = new
            {
                userId = userId,
                companyId = companyId
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "user.validation.exchange",
                routingKey: "validate",
                basicProperties: props,
                body: body
            );

            var tcs = new TaskCompletionSource<bool>();

            void handler(object? sender, BasicDeliverEventArgs ea)
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var response = JsonSerializer.Deserialize<UserValidationResponse>(json);

                    tcs.SetResult(response!.valid);

                    _consumer.Received -= handler;
                }
            }

            _consumer.Received += handler;

            return await tcs.Task;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping TimeTracking RabbitMQ...");

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
