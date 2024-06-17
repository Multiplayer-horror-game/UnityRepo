using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fase1
{
    public class DubbelList<T, K> : IEnumerable<KeyValuePair<T, K>>
    {
        private readonly List<T> _firstValue;
        private readonly List<K> _secondValue;

        // Public read-only properties
        public IReadOnlyList<T> FirstValue => _firstValue;
        public IReadOnlyList<K> SecondValue => _secondValue;
        
        public int Count => _firstValue.Count;

        public DubbelList()
        {
            _firstValue = new List<T>();
            _secondValue = new List<K>();
        }
        
        public void Add(T first, K second)
        {
            _firstValue.Add(first);
            _secondValue.Add(second);
        }

        public void Add(KeyValuePair<T, K> pair)
        {
            _firstValue.Add(pair.Key);
            _secondValue.Add(pair.Value);
        }
        
        public void Remove(T first, K second)
        {
            _firstValue.Remove(first);
            _secondValue.Remove(second);
        }
        
        public void RemoveAt(int index)
        {
            _firstValue.RemoveAt(index);
            _secondValue.RemoveAt(index);
        }
        
        public KeyValuePair<T,K> GetPair(int index)
        {
            return new KeyValuePair<T, K>(_firstValue[index], _secondValue[index]);
        }

        public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
        {
            for (int i = 0; i < _firstValue.Count; i++)
            {
                yield return new KeyValuePair<T, K>(_firstValue[i], _secondValue[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}