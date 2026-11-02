using HRFlow.Contracts;

namespace ApplicationEvents.Storage;

public interface IApplicationStorage
{
    void Add(Application application);
    Application? GetById(string id);
    public bool Delete(string id);
    IReadOnlyCollection<Application> GetAll();
}