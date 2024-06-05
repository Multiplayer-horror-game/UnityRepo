using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fase1.Scripts.Math
{
    public class HeatSortedQueue<T>
    {
        //stores all the values in a struct that contains the heat and the value
        private readonly List<HeatValue<T>> _values = new();
        
        public int Count => _values.Count;
        
        public HeatValue<T> Dequeue()
        {
            if(_values.Count == 0)
                return default;
            
            int highestHeat = 0;
            HeatValue<T> highestHeatValue = _values[0];
            foreach (var heatValue in _values)
            {
                
                if (heatValue.Heat > highestHeat)
                {
                    highestHeat = heatValue.Heat;
                    highestHeatValue = heatValue;

                    if (heatValue.Heat > 90)
                    {
                        _values.Remove(highestHeatValue);
                        return highestHeatValue;
                    }
                }
            }
            
            _values.Remove(highestHeatValue);
            return highestHeatValue;
        }
        
        
        public void Enqueue(int heat, T value)
        {
            if(heat > 92) return;
            
            _values.Add(new HeatValue<T>(heat, value));
        }

        public T Get(int index)
        {
            return _values[index].Value;
        }
        
        public void Remove(int index)
        {
            _values.RemoveAt(index);
        }
        
        public List<HeatValue<T>> GetValues()
        {
            return _values;
        }
    }
}