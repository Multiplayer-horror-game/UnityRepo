namespace Fase1.Scripts.Math
{
    public struct HeatValue<T>
    {
        private readonly int _heat;
        private readonly T _value;
        
        public int Heat => _heat;
        public T Value => _value;
        
        public HeatValue(int heat, T value)
        {
            _heat = heat;
            this._value = value;
        }
        
    }
}