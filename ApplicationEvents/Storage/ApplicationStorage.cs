using System.Collections.Concurrent;
using HRFlow.Contracts;

namespace ApplicationEvents.Storage;

public class ApplicationStorage : IApplicationStorage
{
    private readonly ConcurrentDictionary<string, Application> _items = new();

    public void Add(Application application) 
    {
        _items.TryAdd(application.Id, application);
    }

    public Application? GetById(string id)
    {
        _items.TryGetValue(id, out var application);
        return application;
    }

    public bool Delete(string id)
    {
        return _items.TryRemove(id, out _);
    }

    public IReadOnlyCollection<Application> GetAll() => _items.Values.ToArray();
}