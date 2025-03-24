using System.Collections;
using System.Collections.Generic;

public class ConcurrentList<T> : IList<T>
{
    private readonly IList<T> _list = new List<T>();
    private readonly object _synchObj = new object();

    public T this[int index]
    {
        get
        {
            T returnValue=_list[index];

            lock(_synchObj)
            {
                returnValue = _list[index];
            }

            return returnValue;
        }
        set
        {
            lock (_synchObj)
            {
                _list[index] = value;
            }
        }
    }

    public int Count {
        get
        {
            int count = 0;
            lock (_synchObj)
            {
                count= _list.Count;
            }
            return count;
        }
    }

    public bool IsReadOnly => throw new System.NotImplementedException();

    public void Add(T item)
    {
        lock (_synchObj)
        {
            _list.Add(item);
        }
    }

    public void Clear()
    {
        lock (_synchObj)
        {
            _list.Clear();
        }
    }

    public bool Contains(T item)
    {
        bool returnValue = false;
        lock (_synchObj)
        {
            returnValue=_list.Contains(item);
        }
        return returnValue;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (_synchObj)
        {
            _list.CopyTo(array, arrayIndex);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        IEnumerator<T> returnValue = null;

        lock (_synchObj)
        {
            returnValue=_list.GetEnumerator();
        }

        return returnValue;
    }

    public int IndexOf(T item)
    {
        int returnValue = 0;
        lock (_synchObj)
        {
            returnValue = _list.IndexOf(item);
        }
        return returnValue;
    }

    public void Insert(int index, T item)
    {
        lock (_synchObj)
        {
            _list.Insert(index, item);
        }
    }

    public bool Remove(T item)
    {
        bool returnValue = false;
        lock (_synchObj)
        {
            returnValue=_list.Remove(item);
        }
        return returnValue;
    }

    public void RemoveAt(int index)
    {
        lock (_synchObj)
        {
            _list.RemoveAt(index);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
