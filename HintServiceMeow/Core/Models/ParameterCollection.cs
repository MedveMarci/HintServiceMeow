using System;
using System.Collections;
using System.Collections.Generic;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models;

public class ParameterCollection : IEnumerable<Tuple<string, IParameter>>
{
    private readonly List<Tuple<string, IParameter>> list = new(4);

    internal int Version { get; private set; }

    public ParameterCollection()
    { }

    public ParameterCollection(ParameterCollection other)
    {
        list.AddRange(other.list);
    }

    public void Add(string tagName, IParameter parameter)
    {
        list.RemoveAll(x => x.Item1 == tagName);
        list.Add(Tuple.Create(tagName, parameter));
        Version++;
    }

    public void RemoveAll(Predicate<Tuple<string, IParameter>> match)
    {
        if (list.RemoveAll(match) > 0)
            Version++;
    }

    public void RemoveParameter(string tagName)
    {
        RemoveAll(x => x.Item1 == tagName);
    }

    public void RemoveParameters<T>() where T : IParameter
    {
        RemoveAll(p => p.Item2 is T);
    }

    public Tuple<string, IParameter>[] ToArray()
    {
        return [.. list];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.ToArray().GetEnumerator();
    }

    IEnumerator<Tuple<string, IParameter>> IEnumerable<Tuple<string, IParameter>>.GetEnumerator()
    {
        return ((IEnumerable<Tuple<string, IParameter>>)[.. list]).GetEnumerator();
    }
}