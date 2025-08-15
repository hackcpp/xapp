using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace test.Controllers
{
    public class RabbitMQController : BaseDbController
    {
        private const string QueueName = "test_queue";
        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMQController(IConfiguration config, ILogger<RabbitMQController> logger)
            : base(config, logger)
        {
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("RabbitMQ");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("RabbitMQ 连接字符串配置缺失，请在 appsettings.json 的 ConnectionStrings 节点下添加 RabbitMQ 配置");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            ViewBag.ConnectionStatus = "未连接";
            return View();
        }

        public async Task<IActionResult> TestRabbitMQ()
        {
            var results = new List<string>();
            var message = $"Test message {Guid.NewGuid()}";
            var receivedMessage = string.Empty;
            var tcs = new TaskCompletionSource<string>();

            try
            {
                results.Add("=== RabbitMQ 测试开始 ===");

                // 初始化连接
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_connectionString)
                };
                
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel?.QueueDeclare(
                    queue: QueueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                results.Add("成功建立RabbitMQ连接");

                // 1. 发送消息
                var body = Encoding.UTF8.GetBytes(message);
                _channel?.BasicPublish(
                    exchange: "",
                    routingKey: QueueName,
                    basicProperties: null,
                    body: body);
                results.Add($"成功发送消息: '{message}'");

                // 2. 接收消息
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var receivedBody = ea.Body.ToArray();
                    var msg = Encoding.UTF8.GetString(receivedBody);
                    tcs.TrySetResult(msg);
                };

                _channel?.BasicConsume(
                    queue: QueueName,
                    autoAck: true,
                    consumer: consumer);
                results.Add("开始监听队列...");

                // 等待接收消息或超时
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(5000));

                if (completedTask == tcs.Task)
                {
                    receivedMessage = await tcs.Task;
                    results.Add($"成功接收到消息: '{receivedMessage}'");
                    results.Add($"\n测试结果: {(message == receivedMessage ? "✅ 成功" : "❌ 失败")}");
                }
                else
                {
                    results.Add("接收消息超时 (5秒)");
                    results.Add("\n测试结果: ❌ 失败");
                }
            }
            catch (Exception ex)
            {
                results.Add($"RabbitMQ 测试失败: {ex.Message}");
                _logger.LogError(ex, "RabbitMQ test failed");
            }
            finally
            {
                if (_channel != null)
                {
                    _channel.Close();
                    _channel.Dispose();
                }
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
                results.Add("=== RabbitMQ 测试结束 ===");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }

    }
}
