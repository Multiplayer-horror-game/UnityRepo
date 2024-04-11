using System.Collections.Generic;
using UnityEngine;

namespace RoomGenerator.scripts
{
    public class Room : MonoBehaviour
    {
        public List<Vector2Int> positions;
        public List<Door> doors;
        public List<Vector2Int> hallwayPositions;
    
    }
    
    public struct Door
    {
        public Vector2Int position;
        public Vector2Int direction;
    }
}
