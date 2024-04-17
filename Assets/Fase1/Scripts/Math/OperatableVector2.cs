using UnityEngine;

namespace Fase1
{
    public class OperatableVector2
    {
        private float _x;
        private float _y;
        
        public OperatableVector2(float x, float y)
        {
            _x = x;
            _y = y;
        }
        
        public OperatableVector2(Vector2 vector)
        {
            _x = vector.x;
            _y = vector.y;
        }
        
        public static OperatableVector2 operator +(OperatableVector2 a, OperatableVector2 b)
        {
            return new OperatableVector2(a._x + b._x, a._y + b._y);
        }
        
        public static OperatableVector2 operator -(OperatableVector2 a, OperatableVector2 b)
        {
            return new OperatableVector2(a._x - b._x, a._y - b._y);
        }
        
        public static OperatableVector2 operator *(OperatableVector2 a, float b)
        {
            return new OperatableVector2(a._x * b, a._y * b);
        }
        
        public static OperatableVector2 operator /(OperatableVector2 a, float b)
        {
            return new OperatableVector2(a._x / b, a._y / b);
        }
        
        public static bool operator >(OperatableVector2 a, OperatableVector2 b)
        {
            return a._x > b._x && a._y > b._y;
        }
        
        public static bool operator <(OperatableVector2 a, OperatableVector2 b)
        {
            return a._x < b._x && a._y < b._y;
        }
        
        public static bool operator >=(OperatableVector2 a, OperatableVector2 b)
        {
            return a._x >= b._x && a._y >= b._y;
        }
        
        public static bool operator <=(OperatableVector2 a, OperatableVector2 b)
        {
            return a._x <= b._x && a._y <= b._y;
        }
        
        public static implicit operator Vector2(OperatableVector2 a)
        {
            return new Vector2(a._x, a._y);
        }
        
        public static implicit operator OperatableVector2(Vector2 a)
        {
            return new OperatableVector2(a);
        }
        
        public static implicit operator Vector3(OperatableVector2 a)
        {
            return new Vector3(a._x, a._y);
        }
        
        public static implicit operator string(OperatableVector2 a)
        {
            return $"({a._x},{a._y})";
        }
        
        
    }
}