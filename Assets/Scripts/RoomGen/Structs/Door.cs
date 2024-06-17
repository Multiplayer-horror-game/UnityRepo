using System;
using UnityEngine;

namespace RoomGenerator.scripts.Structs
{
    [Serializable]
    public struct Door
    {
        public Vector2Int position;
        public Directions directions;
    }
}