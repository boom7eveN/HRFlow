using Contracts;
using GatewayAPI;
using GatewayAPI.Kafka;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using HRFlow.Contracts;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("scalable", c =>
{
    c.BaseAddress = new Uri(
        builder.Configuration["Services:ScalableService"] ?? "http://nginx:80");
});

var address = builder.Configuration["Services:ApplicationEvents"] ?? "http://application-events:8080";
var channel = GrpcChannel.ForAddress(address);
var client = new ApplicationQuery.ApplicationQueryClient(channel);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapPost("/application", async (CreateApplicationRequest req, IKafkaProducer kafka) =>
{
    var evt = new ApplicationSubmittedEvent(Guid.NewGuid(), req.CandidateName);
    await kafka.SendAsync(evt);
    return Results.Ok(new { evt.CandidateId, evt.CandidateName });
});


app.MapGet("/application", async () =>
{
    var response = await client.ListApplicationsAsync(new ListApplicationsRequest());
    return Results.Ok(new { items = response.Items });
});

app.MapGet("/application/{id}", async (string id) =>
{
    try
    {
        var response = await client.GetApplicationAsync(new GetApplicationRequest { Id = id });
        return Results.Ok(response);
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
    {
        return Results.NotFound(new { message = $"заявка с ID {id} не найдена" });
    }
});

app.MapDelete("/application/{id}", async (string id) =>
{
    try
    {
        await client.DeleteApplicationAsync(new DeleteApplicationRequest { Id = id });
        return Results.NoContent();
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
    {
        return Results.NotFound(new { message = $"заявка с ID {id} не найдена" });
    }
});

app.MapPut("/application/{id}", async (string id, UpdateApplicationApiRequest req) =>
{
    var appToUpdate = new Application { Id = id };
    var paths = new List<string>();

    if (req.CandidateName != null)
    {
        appToUpdate.CandidateName = req.CandidateName;
        paths.Add("candidate_name");
    }

    if (paths.Count == 0) return Results.BadRequest("нет полей для обновления!");

    var request = new UpdateApplicationRequest
    {
        Application = appToUpdate,
        UpdateMask = FieldMask.FromString(string.Join(",", paths))
    };

    try
    {
        var response = await client.UpdateApplicationAsync(request);
        return Results.Ok(response);
    }
    catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
    {
        return Results.NotFound(new { message = $"заявка с ID {id} не найдена" });
    }
});

app.MapGet("/scalable/health", async (IHttpClientFactory httpFactory) =>
{
    var http = httpFactory.CreateClient("scalable");
    var resp = await http.GetAsync("/health");
    var body = await resp.Content.ReadAsStringAsync();
    return Results.Content(body, "application/json", statusCode: (int)resp.StatusCode);
});

app.Run();

public record CreateApplicationRequest(string CandidateName);
public record UpdateApplicationApiRequest(string? CandidateName);