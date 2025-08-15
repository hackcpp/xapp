using Microsoft.AspNetCore.Mvc;
using Enyim.Caching;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace test.Controllers
{
    public class MemcachedController : BaseDbController
    {
        private readonly Lazy<IMemcachedClient> _memcachedClient;
        private readonly IConfiguration _config;
        private readonly ILoggerFactory _loggerFactory;

        public MemcachedController(IConfiguration config, ILogger<MemcachedController> logger, ILoggerFactory loggerFactory)
            : base(config, logger)
        {
            _config = config;
            _loggerFactory = loggerFactory;
            _memcachedClient = new Lazy<IMemcachedClient>(() =>
            {
                var memcachedOptions = new MemcachedClientOptions();
                var memcachedConfig = new MemcachedClientConfiguration(_loggerFactory, Options.Create(memcachedOptions));
                memcachedConfig.AddServer(_connectionString.Split(':')[0], int.Parse(_connectionString.Split(':')[1]));
                return new MemcachedClient(_loggerFactory, memcachedConfig);
            });
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MemcachedConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Memcached连接字符串未配置");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestEnyimMemcachedCore()
        {
            var results = new List<string>();
            
            try
            {
                results.Add("=== Memcached 同步操作测试 ===");
                var syncKey = "sync_key_" + Guid.NewGuid();
                var syncValue = "sync_value_" + Guid.NewGuid();

                // 同步设置值
                _memcachedClient.Value.Set(syncKey, syncValue, 60);
                results.Add($"同步设置值成功: Key={syncKey}, Value={syncValue}");

                // 同步获取值
                var syncRetrieved = _memcachedClient.Value.Get<string>(syncKey);
                results.Add($"同步获取值测试: {(syncRetrieved == syncValue ? "✅ 成功" : "❌ 失败")}");

                // 同步删除
                _memcachedClient.Value.Remove(syncKey);
                results.Add($"同步删除测试: 成功删除键 {syncKey}");

                results.Add("\n=== Memcached 异步操作测试 ===");
                var testKey = "test_key_" + Guid.NewGuid();
                var testValue = "test_value_" + Guid.NewGuid();

                // 异步设置值测试
                await _memcachedClient.Value.SetAsync(testKey, testValue, 60);
                results.Add($"异步设置值成功: Key={testKey}, Value={testValue}");

                // 异步获取值测试
                var retrievedResult = await _memcachedClient.Value.GetAsync<string>(testKey);
                results.Add($"异步获取值测试: {(retrievedResult.Success && retrievedResult.Value == testValue ? "✅ 成功" : "❌ 失败")}");

                // 异步删除测试
                await _memcachedClient.Value.RemoveAsync(testKey);
                results.Add($"异步删除测试: 成功删除键 {testKey}");

            }
            catch (Exception ex)
            {
                results.Add($"Memcached 测试失败: {ex.Message}");
                _logger.LogError(ex, "Memcached test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}