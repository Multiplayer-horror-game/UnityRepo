using System.Collections.Generic;
using RoomGenerator.scripts.Structs;
using UnityEngine;

namespace RoomGenerator.scripts
{
    public class Room : MonoBehaviour
    {
        public List<Vector2Int> positions;
        public List<Door> doors;
        public List<Vector2Int> hallwayPositions;
    
        public Vector3 placementOffset = new Vector3(0, 0);
    }
    
}
