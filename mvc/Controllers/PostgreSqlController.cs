using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace test.Controllers
{
    public class PostgreSqlController : BaseDbController
    {
        private readonly IConfiguration _config;

        public PostgreSqlController(IConfiguration config, ILogger<PostgreSqlController> logger)
            : base(config, logger)
        {
            _config = config;
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("PostgreSqlConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("PostgreSqlConnection not found in configuration.");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestNpgsql()
        {
            var results = new List<string>();
            
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    // 连接测试
                    await connection.OpenAsync();
                    results.Add("Npgsql 连接成功");
                    
                    // 版本查询
                    results.Add("\n=== 数据库信息 ===");
                    results.Add($"服务器版本: {connection.PostgreSqlVersion}");
                    results.Add($"数据库: {connection.Database}");
                    results.Add($"数据源: {connection.DataSource}");
                    results.Add($"状态: {connection.State}");
                    results.Add($"超时: {connection.ConnectionTimeout}s");

                    // CRUD测试
                    results.Add("\n=== CRUD 操作测试 ===");
                    
                    // 创建测试表
                    var createTableSql = @"CREATE TABLE IF NOT EXISTS test_users (
                        id SERIAL PRIMARY KEY,
                        name VARCHAR(50) NOT NULL,
                        email VARCHAR(100) NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    )";
                    using (var cmd = new NpgsqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("创建测试表成功");
                    }

                    // 插入数据
                    var insertSql = "INSERT INTO test_users (name, email) VALUES (@name, @email) RETURNING id";
                    using (var cmd = new NpgsqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", "Test User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var id = await cmd.ExecuteScalarAsync();
                        results.Add($"插入数据成功，ID: {id}");
                    }

                    // 查询数据
                    var selectSql = "SELECT * FROM test_users ORDER BY id DESC LIMIT 1";
                    using (var cmd = new NpgsqlCommand(selectSql, connection))
                    {
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            results.Add($"查询到数据: ID={rdr["id"]}, Name={rdr["name"]}, Email={rdr["email"]}");
                        }
                    }

                    // 更新测试
                    var updateSql = "UPDATE test_users SET name = @newName WHERE email = @email";
                    using (var cmd = new NpgsqlCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@newName", "Updated User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var rows = await cmd.ExecuteNonQueryAsync();
                        results.Add($"更新数据成功，影响行数: {rows}");
                    }

                    // 清理测试表
                    var dropTableSql = "DROP TABLE IF EXISTS test_users";
                    using (var cmd = new NpgsqlCommand(dropTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("清理测试表成功");
                    }

                    // PostgreSQL 特有功能测试
                    results.Add("\n=== PostgreSQL 特有功能测试 ===");
                    
                    // JSON 功能测试
                    var jsonResult = await new NpgsqlCommand("SELECT json_build_object('test', 'value')::text", connection)
                        .ExecuteScalarAsync();
                    results.Add($"JSON 功能测试: {jsonResult}");

                    // 数组类型测试
                    var arrayResult = await new NpgsqlCommand("SELECT ARRAY[1,2,3]::text", connection)
                        .ExecuteScalarAsync();
                    results.Add($"数组类型测试: {arrayResult}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Npgsql 测试失败: {ex.Message}");
                _logger.LogError(ex, "Npgsql test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}