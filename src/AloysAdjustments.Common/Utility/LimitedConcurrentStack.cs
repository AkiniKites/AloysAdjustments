using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AloysAdjustments.Common.Utility
{
    public class LimitedConcurrentStack<T> : IEnumerable<T>, IEnumerable
    {
        private readonly object _lock = new object();
        
        private readonly T[] _items;
        private readonly int _capacity;
        private readonly SemaphoreSlim _waitingCount;

        private int _count;
        private int _head;
        private bool _complete;

        public LimitedConcurrentStack(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _items = new T[capacity];
            _waitingCount = new SemaphoreSlim(0);
        }

        public void CompleteAdding()
        {
            lock (_lock)
            {
                if (!_complete)
                {
                    _waitingCount.Release();
                    _complete = true;
                }
            }
        }

        public void Push(T item)
        {
            lock (_lock)
            {
                if (_complete)
                    throw new InvalidOperationException("Collection is complete adding.");

                _items[_head] = item;
                _head = (_head + 1) % _items.Length;
                if (_waitingCount.CurrentCount < 10)
                {
                    _waitingCount.Release();
                    _count++;
                }
                else
                    Console.WriteLine("Drop");
            }
        }
        
        public bool TryPopWait(out T val)
        {
            _waitingCount.Wait();
            if (_complete && _count == 0)
            {
                val = default;
                return false;
            }

            val = PopInternal();
            return true;
        }

        public bool TryPop(out T val)
        {
            if (_count == 0)
            {
                val = default;
                return false;
            }

            _waitingCount.Wait();

            val = PopInternal();
            return true;
        }
        private T PopInternal()
        {
            lock (_lock)
            {
                _head = (_items.Length + _head - 1) % _capacity;
                _count--;

                var val = _items[_head];
                _items[_head] = default;
                return val;
            }
        }

        public int Count => _count;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                var head = (_items.Length + _head - 1 + i) % _capacity;
                yield return _items[head];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}