using System;
using System.Collections.Generic;

namespace BrickBreak.Utility;

public class MarbleBag<T>
{
    private IEnumerable<T> fullListing;
    private List<T> current = new List<T>();
    private Random random;

    public MarbleBag(IEnumerable<T> contents)
    {
        fullListing = contents;
        random = new Random();
    }

    public void Reset()
    {
        current.Clear();
        foreach(T t in fullListing)
        {
            current.Insert(random.Next(0, current.Count + 1), t);
        }
    }

    public void Reset(IEnumerable<T> contents)
    {
        fullListing = contents;
        Reset();
    }

    public T Next
    {
        get
        {
            if(current.Count == 0)
            {
                Reset();
            }

            T result = current[current.Count - 1];
            current.RemoveAt(current.Count - 1);

            return result;
        }
    }
}