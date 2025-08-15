using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace test.Controllers
{
    public class SqlServerController : BaseDbController
    {
        private readonly IConfiguration _config;

        public SqlServerController(IConfiguration config, ILogger<SqlServerController> logger)
            : base(config, logger)
        {
            _config = config;
        }

        protected override string GetConnectionString(IConfiguration config)
        {
            var connectionString = config.GetConnectionString("SqlServerConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("SQL Server connection string is not configured");
            }
            return connectionString;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> TestSystemDataSqlClient()
        {
            var results = new List<string>();
            
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(_connectionString))
                {
                    // 连接测试
                    await connection.OpenAsync();
                    results.Add("System.Data.SqlClient 连接成功");
                    
                    // 版本查询
                    using var versionCmd = new System.Data.SqlClient.SqlCommand("SELECT @@VERSION", connection);
                    var version = await versionCmd.ExecuteScalarAsync();
                    results.Add($"SQL Server 版本: {(version?.ToString() ?? "N/A").Split('\n')[0]}");
                    
                    // 数据库列表
                    results.Add("\n数据库列表:");
                    using var dbCmd = new System.Data.SqlClient.SqlCommand("SELECT name FROM sys.databases", connection);
                    using var reader = await dbCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        results.Add(reader.GetString(0));
                    }
                    await reader.CloseAsync();

                    // CRUD测试
                    results.Add("\nCRUD操作测试:");
                    
                    // 创建测试表
                    var createTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='test_users' and xtype='U')
                    CREATE TABLE test_users (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        name NVARCHAR(50) NOT NULL,
                        email NVARCHAR(100) NOT NULL,
                        created_at DATETIME DEFAULT GETDATE()
                    )";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("创建测试表成功");
                    }

                    // 插入数据
                    var insertSql = "INSERT INTO test_users (name, email) VALUES (@name, @email)";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", "Test User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var rows = await cmd.ExecuteNonQueryAsync();
                        results.Add($"插入数据成功，影响行数: {rows}");
                    }

                    // 查询数据
                    var selectSql = "SELECT TOP 1 * FROM test_users ORDER BY id DESC";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(selectSql, connection))
                    {
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            results.Add($"查询到数据: ID={rdr["id"]}, Name={rdr["name"]}, Email={rdr["email"]}");
                        }
                    }

                    // 清理测试表
                    var dropTableSql = "DROP TABLE IF EXISTS test_users";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(dropTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("清理测试表成功");
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"System.Data.SqlClient 测试失败: {ex.Message}");
                _logger.LogError(ex, "System.Data.SqlClient test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }

        public async Task<IActionResult> TestMicrosoftDataSqlClient()
        {
            var results = new List<string>();
            
            try
            {
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                {
                    // 连接测试
                    await connection.OpenAsync();
                    results.Add("Microsoft.Data.SqlClient 连接成功");
                    
                    // 版本查询
                    using var versionCmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT @@VERSION", connection);
                    var version = await versionCmd.ExecuteScalarAsync();
                    results.Add($"SQL Server 版本: {(version?.ToString() ?? "N/A").Split('\n')[0]}");
                    
                    // 数据库列表
                    results.Add("\n数据库列表:");
                    using var dbCmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT name FROM sys.databases", connection);
                    using var reader = await dbCmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        results.Add(reader.GetString(0));
                    }
                    await reader.CloseAsync();

                    // CRUD测试
                    results.Add("\nCRUD操作测试:");
                    
                    // 创建测试表
                    var createTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='test_users' and xtype='U')
                    CREATE TABLE test_users (
                        id INT IDENTITY(1,1) PRIMARY KEY,
                        name NVARCHAR(50) NOT NULL,
                        email NVARCHAR(100) NOT NULL,
                        created_at DATETIME DEFAULT GETDATE()
                    )";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(createTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("创建测试表成功");
                    }

                    // 插入数据
                    var insertSql = "INSERT INTO test_users (name, email) VALUES (@name, @email)";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(insertSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", "Test User");
                        cmd.Parameters.AddWithValue("@email", "test@example.com");
                        var rows = await cmd.ExecuteNonQueryAsync();
                        results.Add($"插入数据成功，影响行数: {rows}");
                    }

                    // 查询数据
                    var selectSql = "SELECT TOP 1 * FROM test_users ORDER BY id DESC";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(selectSql, connection))
                    {
                        using var rdr = await cmd.ExecuteReaderAsync();
                        while (await rdr.ReadAsync())
                        {
                            results.Add($"查询到数据: ID={rdr["id"]}, Name={rdr["name"]}, Email={rdr["email"]}");
                        }
                    }

                    // 清理测试表
                    var dropTableSql = "DROP TABLE IF EXISTS test_users";
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(dropTableSql, connection))
                    {
                        await cmd.ExecuteNonQueryAsync();
                        results.Add("清理测试表成功");
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"Microsoft.Data.SqlClient 测试失败: {ex.Message}");
                _logger.LogError(ex, "Microsoft.Data.SqlClient test failed");
            }

            ViewBag.Results = results;
            return View("TestResults");
        }
    }
}
