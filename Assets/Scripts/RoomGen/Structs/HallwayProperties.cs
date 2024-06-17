using System;
using UnityEngine;

namespace RoomGenerator.scripts.Structs
{
    [Serializable]
    public struct HallwayProperties
    {
        public GameObject hallway;
        
        //north east south west
        public Directions directions;
    }
}