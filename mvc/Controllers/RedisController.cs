using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using ServiceStack.Redis;

namespace test.Controllers
{
    public class RedisController : BaseDbController
    {
        private readonly Lazy<IConnectionMultiplexer> _stackExchangeRedis;
        private readonly Lazy<IRedisClientsManager> _serviceStackRedisManager;
        private readonly ILoggerFactory _loggerFactory;

        public RedisController(ILogger<RedisController> logger,
                             IConfiguration config,
                             ILoggerFactory loggerFactory) : base(config, logger)
        {
            _loggerFactory = loggerFactory;
            _stackExchangeRedis = new Lazy<IConnectionMultiplexer>(() =>
                ConnectionMultiplexer.Connect(_connectionString));
            
            _serviceStackRedisManager = new Lazy<IRedisClientsManager>(() => {
                var parts = _connectionString.Split(',');
                var hostPort = parts[0].Split(':');
                var password = parts.FirstOrDefault(x => x.StartsWith("password="))?.Split('=')[1];
                
                var sb = new System.Text.StringBuilder();
                sb.Append($"{hostPort[0]}:{hostPort[1]}");
                if (!string.IsNullOrEmpty(password))
                {
                    sb.Append($"?password={Uri.EscapeDataString(password)}");
                }
                var connStr = sb.ToString();
                return new RedisManagerPool(connStr);
            });
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("RedisConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Redis连接字符串未配置");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestStackExchangeRedis()
        {
            var results = new List<string>();
            try
            {
                var db = _stackExchangeRedis.Value.GetDatabase();
                var server = _stackExchangeRedis.Value.GetServer(_stackExchangeRedis.Value.GetEndPoints().First());
                
                results.Add("StackExchange.Redis连接成功");
                results.Add($"Redis版本: {server.Version}");
                results.Add($"运行状态: {(server.IsConnected ? "正常" : "异常")}");

                // 字符串CRUD测试
                results.Add("\n字符串CRUD测试:");
                await db.StringSetAsync("test:string", "Hello StackExchange.Redis", TimeSpan.FromMinutes(1));
                var stringValue = await db.StringGetAsync("test:string");
                results.Add($"读取字符串值: {stringValue}");
                await db.KeyDeleteAsync("test:string");

                // 哈希表CRUD测试
                results.Add("\n哈希表CRUD测试:");
                await db.HashSetAsync("test:hash", new HashEntry[] {
                    new HashEntry("field1", "value1"),
                    new HashEntry("field2", "value2")
                });
                var hashValue = await db.HashGetAsync("test:hash", "field1");
                results.Add($"读取哈希字段值: {hashValue}");
                await db.KeyDeleteAsync("test:hash");

                // 列表CRUD测试
                results.Add("\n列表CRUD测试:");
                await db.ListRightPushAsync("test:list", "item1");
                await db.ListRightPushAsync("test:list", "item2");
                var listLength = await db.ListLengthAsync("test:list");
                var listItem = await db.ListGetByIndexAsync("test:list", 0);
                results.Add($"列表长度: {listLength}, 第一个元素: {listItem}");
                await db.KeyDeleteAsync("test:list");

                // 集合CRUD测试
                results.Add("\n集合CRUD测试:");
                await db.SetAddAsync("test:set", "member1");
                await db.SetAddAsync("test:set", "member2");
                var setMembers = await db.SetMembersAsync("test:set");
                results.Add($"集合成员数: {setMembers.Length}");
                await db.KeyDeleteAsync("test:set");

                // 有序集合CRUD测试
                results.Add("\n有序集合CRUD测试:");
                await db.SortedSetAddAsync("test:sortedset", "member1", 1);
                await db.SortedSetAddAsync("test:sortedset", "member2", 2);
                var sortedSetScore = await db.SortedSetScoreAsync("test:sortedset", "member1");
                results.Add($"成员1的分数: {sortedSetScore}");
                await db.KeyDeleteAsync("test:sortedset");

                // 键操作测试
                results.Add("\n键操作测试:");
                await db.StringSetAsync("test:expire", "expiring value", TimeSpan.FromSeconds(5));
                var existsBefore = await db.KeyExistsAsync("test:expire");
                await Task.Delay(6000);
                var existsAfter = await db.KeyExistsAsync("test:expire");
                results.Add($"键过期测试: 过期前存在={existsBefore}, 过期后存在={existsAfter}");
            }
            catch (Exception ex)
            {
                results.Add($"StackExchange.Redis测试失败: {ex.Message}");
                _logger.LogError(ex, "StackExchange.Redis test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }

        public IActionResult TestServiceStackRedis()
        {
            var results = new List<string>();
            try
            {
                var redisPassword = _connectionString
                    .Split(',')
                    .FirstOrDefault(x => x.StartsWith("password="))
                    ?.Split('=')[1];

                using var redis = _serviceStackRedisManager.Value.GetClient();
                if (!string.IsNullOrEmpty(redisPassword))
                {
                    redis.Password = redisPassword;
                }
                
                results.Add("ServiceStack.Redis连接成功");
                var redisInfo = redis.Info.FirstOrDefault(x => x.Key == "redis_version");
                results.Add($"Redis版本: {(redisInfo.Equals(default(KeyValuePair<string, string>)) ? "未知" : redisInfo.Value)}");

                // 字符串CRUD测试
                results.Add("\n字符串CRUD测试:");
                redis.Set("test:string", "Hello ServiceStack.Redis", TimeSpan.FromMinutes(1));
                var stringValue = redis.Get<string>("test:string");
                results.Add($"读取字符串值: {stringValue}");
                redis.Remove("test:string");

                // 哈希表CRUD测试
                results.Add("\n哈希表CRUD测试:");
                redis.SetEntryInHash("test:hash", "field1", "value1");
                redis.SetEntryInHash("test:hash", "field2", "value2");
                var hashValue = redis.GetValueFromHash("test:hash", "field1");
                results.Add($"读取哈希字段值: {hashValue}");
                redis.Remove("test:hash");

                // 列表CRUD测试
                results.Add("\n列表CRUD测试:");
                redis.AddItemToList("test:list", "item1");
                redis.AddItemToList("test:list", "item2");
                var listLength = redis.GetListCount("test:list");
                var listItem = redis.GetItemFromList("test:list", 0);
                results.Add($"列表长度: {listLength}, 第一个元素: {listItem}");
                redis.Remove("test:list");

                // 集合CRUD测试
                results.Add("\n集合CRUD测试:");
                redis.AddItemToSet("test:set", "member1");
                redis.AddItemToSet("test:set", "member2");
                var setMembers = redis.GetAllItemsFromSet("test:set");
                results.Add($"集合成员数: {setMembers.Count}");
                redis.Remove("test:set");

                // 有序集合CRUD测试
                results.Add("\n有序集合CRUD测试:");
                redis.AddItemToSortedSet("test:sortedset", "member1", 1);
                redis.AddItemToSortedSet("test:sortedset", "member2", 2);
                var sortedSetScore = redis.GetItemScoreInSortedSet("test:sortedset", "member1");
                results.Add($"成员1的分数: {sortedSetScore}");
                redis.Remove("test:sortedset");

                // 键操作测试
                results.Add("\n键操作测试:");
                redis.Set("test:expire", "expiring value", TimeSpan.FromSeconds(5));
                var existsBefore = redis.ContainsKey("test:expire");
                Thread.Sleep(6000);
                var existsAfter = redis.ContainsKey("test:expire");
                results.Add($"键过期测试: 过期前存在={existsBefore}, 过期后存在={existsAfter}");
            }
            catch (Exception ex)
            {
                results.Add($"ServiceStack.Redis测试失败: {ex.Message}");
                _logger.LogError(ex, "ServiceStack.Redis test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}