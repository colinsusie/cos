// Written by Colin on 2023-11-25

using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using CoLib.Common;

namespace CoLib.Container;

/// <summary>
/// 本地列表，列表中的元素从ArrayPool分配，在函数中作为局部变量使用，加上using确保Dispose被调用
/// </summary>
public struct LocalList<T> : IDisposable, IEnumerable<T>
{
    private const int DefaultCapacity = 16;
    private static readonly T[] SEmptyArray = Array.Empty<T>();

    private T[] _items = SEmptyArray;
    private int _size = 0;

    public LocalList(): this(0)
    {
    }

    public LocalList(int capacity)
    {
        if (capacity < 0)
            ThrowHelper.ThrowArgumentOutOfRangeException("capacity can not less than 0");
        _items = RentFromPool(capacity);
    }

    public int Count => _size;
    public int Capacity => _items.Length;

    public T this[int index] => Get(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index)
    {
        if ((uint) index >= (uint) _size)
            ThrowHelper.ThrowIndexOutOfRangeException($"index out of range, _size:{_size}, index:{index}");
        return _items[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int index, T value)
    {
        if ((uint) index >= (uint) _size)
            ThrowHelper.ThrowIndexOutOfRangeException($"index out of range, _size:{_size}, index:{index}");
        _items[index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        T[] array = _items;
        int size = _size;
        if ((uint) size < (uint) array.Length)
        {
            _size = size + 1;
            array[size] = item;
        }
        else
        {
            AddWithResize(item);
        }
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        if ((uint) index >= (uint) _size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException($"index out of range, index:{index}, size:{_size}");
        }

        _size--;
        if (index < _size)
        {
            Array.Copy(_items, index + 1, _items, index, _size - index);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_size] = default!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            int size = _size;
            _size = 0;
            if (size > 0)
            {
                Array.Clear(_items, 0, size); // Clear the elements so that the gc can reclaim the references.
            }
        }
        else
        {
            _size = 0;
        }
    }

    public bool Contains(T item) => _size != 0 && IndexOf(item) != -1;

    public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _size);

    public T[] ToArray()
    {
        if (_size == 0)
        {
            return SEmptyArray;
        }

        T[] array = new T[_size];
        Array.Copy(_items, array, _size);
        return array;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        int size = _size;
        Grow(size + 1);
        _size = size + 1;
        _items[size] = item;
    }

    private void Grow(int capacity)
    {
        int num = _items.Length == 0 ? DefaultCapacity : 2 * _items.Length;
        if ((uint) num > Array.MaxLength)
            num = Array.MaxLength;
        if (num < capacity)
            num = capacity;

        if (num < _size)
            ThrowHelper.ThrowArgumentOutOfRangeException($"capacity can not less than size, capacity:{num}, size:{_size}");
        if (num == _items.Length)
            return;

        var distArray = RentFromPool(num);
        if (_size > 0)
            Array.Copy(_items, distArray, _size);

        ReturnToPool(_items, _size);
        _items = distArray;
    }

    public void Dispose()
    {
        ReturnToPool(_items, _size);
        _items = SEmptyArray;
        _size = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T[] RentFromPool(int num)
    {
        return num == 0 ? SEmptyArray : ArrayPool<T>.Shared.Rent(num);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ReturnToPool(T [] items, int size)
    {
        if (items.Length > 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                Array.Clear(items, 0, size);
            ArrayPool<T>.Shared.Return(items);
        }
    }

    public Enumerator GetEnumerator() => new(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>) this).GetEnumerator();

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _items;
        private readonly int _end;
        private int _current;

        internal Enumerator(in LocalList<T> list)
        {
            _items = list._items;
            _end = list._size - 1;
            _current = -1;
        }

        public bool MoveNext()
        {
            if (_current < _end)
            {
                _current++;
                return true;
            }
            return false;
        }

        public T Current
        {
            get
            {
                if (_current < 0 || _current > _end)
                    ThrowHelper.ThrowInvalidOperationException($"_current not must be in [0, {_end}]");
                return _items[_current];
            }
        }

        object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            _current = -1;
        }
        
        public void Dispose()
        {
        }
    }
}