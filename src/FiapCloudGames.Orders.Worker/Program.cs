using FiapCloudGames.BuildingBlocks.Hosting;
using FiapCloudGames.Orders.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddFiapServiceDefaults();
builder.Services.AddOrdersWorker(builder.Configuration);

var host = builder.Build();
await host.RunAsync();
