using ApplicationEvents.Storage;
using Google.Protobuf.WellKnownTypes;

using Grpc.Core;
using HRFlow.Contracts;

namespace ApplicationEvents.gRPC;

public class ApplicationGrpcService(
    IApplicationStorage applicationStorage) : ApplicationQuery.ApplicationQueryBase
{
    public override Task<ListApplicationsResponse> ListApplications(
        ListApplicationsRequest request, ServerCallContext context)
    {
        var response = new ListApplicationsResponse();
        response.Items.AddRange(applicationStorage.GetAll());
        return Task.FromResult(response);
    }
    
    public override Task<Application> GetApplication(
        GetApplicationRequest request, ServerCallContext context)
    {
        var application = applicationStorage.GetById(request.Id);
        
        if (application == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Заявка с ID {request.Id} не найдена."));
        }
        return Task.FromResult(application);
    }

    public override Task<Empty> DeleteApplication(
        DeleteApplicationRequest request, ServerCallContext context)
    {
        var isDeleted = applicationStorage.Delete(request.Id);
        if (!isDeleted)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Заявка с ID {request.Id} не найдена."));
        }
        return Task.FromResult(new Empty());
    }
    
    public override Task<Application> UpdateApplication(
        UpdateApplicationRequest request, ServerCallContext context)
    {
        var existingApp = applicationStorage.GetById(request.Application.Id);
        if (existingApp == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"заявка с ID {request.Application.Id} не найдена"));
        }

        if (request.UpdateMask != null)
        {
            request.UpdateMask.Merge(request.Application, existingApp);
        }
        else
        {
            existingApp.MergeFrom(request.Application);
        }

        return Task.FromResult(existingApp);
    }

}