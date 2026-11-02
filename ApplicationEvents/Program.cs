using ApplicationEvents.gRPC;
using ApplicationEvents.Kafka;
using ApplicationEvents.Storage;
using HRFlow.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IApplicationStorage, ApplicationStorage>();
builder.Services.AddHostedService<KafkaBackgroundConsumer>();

var app = builder.Build();

var storage = app.Services.GetRequiredService<IApplicationStorage>();
storage.Add(new Application { Id = "TEST", CandidateName = "TEST" });

app.MapGrpcService<ApplicationGrpcService>();
app.Run();