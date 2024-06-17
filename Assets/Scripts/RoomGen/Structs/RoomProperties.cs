using System;
using UnityEngine;

namespace RoomGenerator.scripts.Structs
{
    [Serializable]
    public struct RoomProperties
    {
        public bool mandatory;
        public int maxAmount;
        public GameObject gameObject;

        public bool isStatic;
        public Vector2Int staticPosition;
    }
}