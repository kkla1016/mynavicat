using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using MyNavicat.Api.Data;
using MyNavicat.Api.Hubs;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 設定預設監聽位址 (5050)
builder.WebHost.UseUrls("http://localhost:5050");

// 設定 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 設定 SQLite 內容
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=app.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// 註冊基礎應用服務
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddSingleton<IToolDetectionService, ToolDetectionService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

// 註冊 6 大資料庫 Provider
builder.Services.AddTransient<IDbProvider, MySqlProvider>();
builder.Services.AddTransient<IDbProvider, PostgreSqlProvider>();
builder.Services.AddTransient<IDbProvider, SqlServerProvider>();
builder.Services.AddTransient<IDbProvider, MariaDbProvider>();
builder.Services.AddTransient<IDbProvider, SqliteProvider>();
builder.Services.AddTransient<IDbProvider, OracleProvider>();

// 註冊 Factory
builder.Services.AddTransient<IDbProviderFactory, DbProviderFactory>();

// 註冊 Domain Services
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddScoped<IDatabaseBrowseService, DatabaseBrowseService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IExportImportService, ExportImportService>();

// 設定 Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(connectionString));

builder.Services.AddHangfireServer();

// 設定 SignalR
builder.Services.AddSignalR();

// 設定 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:5050")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 自動遷移 SQLite 資料庫
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyNavicat API v1");
});

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthorization();

// 根目錄重導向至 Swagger 頁面
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// 設定 Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.MapControllers();

// 設定 SignalR Hub 端點
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
