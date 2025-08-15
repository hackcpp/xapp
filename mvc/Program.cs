using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using ServiceStack.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 添加 HttpClient 工厂
builder.Services.AddHttpClient();

// 添加 Redis 配置
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// 添加 ServiceStack.Redis 配置
builder.Services.AddSingleton<IRedisClientsManager>(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("RedisConnection") ?? "localhost:6379";
    
    // 设置全局 Redis 配置
    RedisConfig.DefaultConnectTimeout = 30;
    RedisConfig.DefaultIdleTimeOutSecs = 240;
    RedisConfig.VerifyMasterConnections = false;
    
    // 解析连接字符串
    var parts = connectionString.Split(',');
    var host = parts[0];
    var password = parts.Length > 1 ? parts[1].Split('=')[1] : null;
    
    // 创建 RedisManagerPool 并设置密码
    var manager = new RedisManagerPool(host);
    
    // 设置密码(通过全局配置)
    if (!string.IsNullOrEmpty(password)) {
        RedisClient client = (RedisClient)manager.GetClient();
        client.Password = password;
        client.Dispose();
    }
    
    return manager;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
