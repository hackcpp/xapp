using Microsoft.AspNetCore.Mvc;

namespace test.Controllers
{
    public class MySqlController : BaseDbController
    {
        private readonly IConfiguration _config;

        public MySqlController(IConfiguration config, ILogger<MySqlController> logger)
            : base(config, logger)
        {
            _config = config;
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("MySqlConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MySQL connection string is not configured");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestMySqlData()
        {
            var results = new List<string>();
            
            try
            {
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
                {
                    // 连接测试
                    await connection.OpenAsync();
                    results.Add("MySql.Data连接成功");
                    
                    // 版本查询
                    using var versionCmd = new MySql.Data.MySqlClient.MySqlCommand("SELECT VERSION()", connection);
                    var version = await versionCmd.ExecuteScalarAsync();
                    results.Add($"MySQL版本: {version}");
                    
                    // 数据库列表
                    results.Add("\n数据库列表:");
                    using (var dbCmd = new MySql.Data.MySqlClient.MySqlCommand("SHOW DATABASES", connection))
                    {
                        using (var reader = await dbCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(reader.GetString(0));
                            }
                        }
                    }

                    // CRUD测试
                    results.Add("\nCRUD操作测试:");
                    
                    // 创建测试表
                    var createTableSql = @"CREATE TABLE IF NOT EXISTS test_users (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        name VARCHAR(50) NOT NULL,
                        email VARCHAR(100) NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("创建测试表成功");
                    }

                    // 插入数据
                    var insertSql = "INSERT INTO test_users (name, email) VALUES (@name, @email)";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", "Test User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var rows = await cmd.ExecuteNonQueryAsync();
                        results.Add($"插入数据成功，影响行数: {rows}");
                    }

                    // 查询数据
                    var selectSql = "SELECT * FROM test_users ORDER BY id DESC LIMIT 1";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(selectSql, connection))
                    {
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            results.Add($"查询到数据: ID={rdr["id"]}, Name={rdr["name"]}, Email={rdr["email"]}");
                        }
                    }

                    // 清理测试表
                    var dropTableSql = "DROP TABLE IF EXISTS test_users";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(dropTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("清理测试表成功");
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"MySql.Data测试失败: {ex.Message}");
                _logger.LogError(ex, "MySql.Data test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }

        public async Task<IActionResult> TestMySqlConnector()
        {
            var results = new List<string>();
            
            try
            {
                using (var connection = new MySqlConnector.MySqlConnection(_connectionString))
                {
                    // 连接测试
                    await connection.OpenAsync();
                    results.Add("MySqlConnector连接成功");
                    
                    // 版本查询
                    // 版本查询
                    using (var versionCmd = new MySqlConnector.MySqlCommand("SELECT VERSION()", connection))
                    {
                        var version = await versionCmd.ExecuteScalarAsync();
                        results.Add($"MySQL版本: {version}");
                    }
                    
                    // 数据库列表
                    results.Add("\n数据库列表:");
                    using (var dbCmd = new MySqlConnector.MySqlCommand("SHOW DATABASES", connection))
                    {
                        using (var reader = await dbCmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(reader.GetString(0));
                            }
                        }
                    }

                    // CRUD测试
                    results.Add("\nCRUD操作测试:");
                    
                    // 创建测试表
                    var createTableSql = @"CREATE TABLE IF NOT EXISTS test_users (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        name VARCHAR(50) NOT NULL,
                        email VARCHAR(100) NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";
                    using (var cmd = new MySqlConnector.MySqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("创建测试表成功");
                    }

                    // 插入数据
                    var insertSql = "INSERT INTO test_users (name, email) VALUES (@name, @email)";
                    using (var cmd = new MySqlConnector.MySqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", "Test User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var rows = await cmd.ExecuteNonQueryAsync();
                        results.Add($"插入数据成功，影响行数: {rows}");
                    }

                    // 查询数据
                    var selectSql = "SELECT * FROM test_users ORDER BY id DESC LIMIT 1";
                    using (var cmd = new MySqlConnector.MySqlCommand(selectSql, connection))
                    {
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            results.Add($"查询到数据: ID={rdr["id"]}, Name={rdr["name"]}, Email={rdr["email"]}");
                        }
                    }

                    // 清理测试表
                    var dropTableSql = "DROP TABLE IF EXISTS test_users";
                    using (var cmd = new MySqlConnector.MySqlCommand(dropTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("清理测试表成功");
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"MySqlConnector测试失败: {ex.Message}");
                _logger.LogError(ex, "MySqlConnector test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    
    }
}
