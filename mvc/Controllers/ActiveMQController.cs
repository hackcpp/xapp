using Microsoft.AspNetCore.Mvc;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISession = Apache.NMS.ISession;

namespace test.Controllers
{
    public class ActiveMQController : BaseDbController
    {
        private const string QueueName = "test_queue";
        private IConnection? _connection;
        private ISession? _session;

        public ActiveMQController(IConfiguration config, ILogger<ActiveMQController> logger)
            : base(config, logger)
        {
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("ActiveMQConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ActiveMQ 连接字符串配置缺失，请在 appsettings.json 的 ConnectionStrings 节点下添加 ActiveMQConnection 配置");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            ViewBag.ConnectionStatus = "未连接";
            return View();
        }

        public async Task<IActionResult> TestActiveMQ()
        {
            var results = new List<string>();
            var message = $"Test message {Guid.NewGuid()}";
            var receivedMessage = string.Empty;
            try
            {
                results.Add("=== ActiveMQ 测试开始 ===");
                var tcs = new TaskCompletionSource<string>();

                // 初始化连接
                var factory = new ConnectionFactory(_connectionString);
                _connection = await factory.CreateConnectionAsync();
                _session = await _connection.CreateSessionAsync();
                
                results.Add("成功建立ActiveMQ连接");

                // 1. 异步方式测试
                results.Add("\n=== 异步方式测试 ===");
                var asyncProducer = await _session.CreateProducerAsync(
                    new ActiveMQQueue(QueueName));
                var asyncTextMessage = await _session.CreateTextMessageAsync(message);
                await asyncProducer.SendAsync(asyncTextMessage);
                results.Add($"异步发送消息: '{message}'");

                var asyncConsumer = await _session.CreateConsumerAsync(
                    new ActiveMQQueue(QueueName));
                var asyncTcs = new TaskCompletionSource<string>();
                asyncConsumer.Listener += msg =>
                {
                    var textMsg = msg as ITextMessage;
                    asyncTcs.TrySetResult(textMsg?.Text ?? string.Empty);
                };

                var asyncReceived = await asyncTcs.Task;
                results.Add($"异步接收消息: '{asyncReceived}'");
                results.Add($"异步测试结果: {(message == asyncReceived ? "✅ 成功" : "❌ 失败")}");

                // 2. 同步方式测试
                results.Add("\n=== 同步方式测试 ===");
                var syncProducer = _session.CreateProducer(
                    new ActiveMQQueue(QueueName));
                var syncTextMessage = _session.CreateTextMessage(message);
                syncProducer.Send(syncTextMessage);
                results.Add($"同步发送消息: '{message}'");

                var syncConsumer = _session.CreateConsumer(
                    new ActiveMQQueue(QueueName));
                string? syncReceived = null;
                syncConsumer.Listener += msg =>
                {
                    var textMsg = msg as ITextMessage;
                    syncReceived = textMsg?.Text;
                };

                while (string.IsNullOrEmpty(syncReceived))
                {
                    await Task.Delay(100);
                }
                results.Add($"同步接收消息: '{syncReceived}'");
                results.Add($"同步测试结果: {(message == syncReceived ? "✅ 成功" : "❌ 失败")}");

                results.Add("开始监听队列...");

                // 等待接收消息
                receivedMessage = await tcs.Task;
                results.Add($"成功接收到消息: '{receivedMessage}'");
                results.Add($"\n测试结果: {(message == receivedMessage ? "✅ 成功" : "❌ 失败")}");
            }
            catch (Exception ex)
            {
                results.Add($"ActiveMQ 测试失败: {ex.Message}");
                _logger.LogError(ex, "ActiveMQ test failed");
            }
            finally
            {
                if (_session != null)
                {
                    await _session.CloseAsync();
                    _session.Dispose();
                }
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    _connection.Dispose();
                }
                results.Add("=== ActiveMQ 测试结束 ===");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}