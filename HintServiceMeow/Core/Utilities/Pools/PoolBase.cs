using System.Collections.Generic;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Utilities.Pools;

internal abstract class PoolBase<T>(int maxSize = 20) : IPool<T>
{
    private readonly Stack<T> objectBag = new();

    public T Rent()
    {
        return objectBag.Count > 0 ? objectBag.Pop() : Create();
    }

    public void Return(T? item)
    {
        if (item is null)
            return;

        if (objectBag.Count < maxSize)
        {
            Reset(item);
            objectBag.Push(item);
        }
    }

    protected abstract T Create();

    protected abstract void Reset(T item);
}