var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

//using KuguaHost;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddMediatR(cfg =>
//{
//    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
//    // 动态加载插件程序集（保持之前 PluginManager 逻辑）
//});

//builder.Services.AddGrpc() // gRPC 服务
//    .AddJsonTranscoding();
//var app = builder.Build();

//// 映射 gRPC 服务（同时支持 gRPC 二进制 + JSON/REST）
//app.MapGrpcService<CommandBusService>();

//// 可选：启用 gRPC-Web（浏览器调用）
//app.MapGrpcService<CommandBusService>().EnableGrpcWeb();

//app.Run("http://localhost:5100");