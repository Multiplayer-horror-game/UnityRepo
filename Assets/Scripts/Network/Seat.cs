using System;
using UnityEngine;

namespace Network
{
    [Serializable]
    public struct Seat
    {
        public Vector3 position;
        public Quaternion rotation;
        private bool claimed;

        public void AttachPlayer()
        {
            claimed = true;
        }
        
        public void DetachPlayer()
        {
            claimed = false;
        }
        
        public bool IsOccupied()
        {
            return claimed;
        }
    }
}