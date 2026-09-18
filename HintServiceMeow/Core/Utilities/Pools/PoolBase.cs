using System.Collections.Concurrent;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Pools;

internal abstract class PoolBase<T>(int maxSize = 20) : IPool<T>
{
    private readonly ConcurrentBag<T> objectBag = [];

    public T Rent()
    {
        return objectBag.TryTake(out T item) ? item : Create();
    }

    public void Return(T? item)
    {
        if (item is null)
            return;

        if (objectBag.Count < maxSize)
        {
            Reset(item);
            objectBag.Add(item);
        }
    }

    protected abstract T Create();

    protected abstract void Reset(T item);
}