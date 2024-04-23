using System;
using System.Collections.Generic;

namespace RoomGenerator.scripts.Structs
{
    [Serializable]
    public struct Directions
    {
        public bool north;
        public bool east;
        public bool south;
        public bool west;

        private void Rotate90deg()
        {
            bool temp = north;
            north = west;
            west = south;
            south = east;
            east = temp;
        }

        private void Rotate180deg()
        {
            bool temp = north;
            north = south;
            south = temp;
            temp = east;
            east = west;
            west = temp;
        }
        
        public Directions Copy()
        {
            return new Directions
            {
                north = north,
                east = east,
                south = south,
                west = west
            };
        }

        //calculate if the directions are the same also check if rotation is needed
        public static CompareResult operator ==(Directions a, Directions b)
        {
            Directions c = a.Copy();
            int rotation = 0;

            for (int i = 0; i < 4; i++)
            {
                if (CompareDirections(c, b))
                {
                    return new CompareResult
                    {
                        result = true,
                        rotation = rotation
                    };
                }

                c.Rotate90deg();
                rotation += 90;
            }

            return new CompareResult
            {
                result = false,
                rotation = 69
            };
        }
        
        public static CompareResult operator !=(Directions a, Directions b)
        {
            CompareResult res = a == b;
            res.result = !res.result;
            return res;
        }
        
        private static bool CompareDirections(Directions a, Directions b)
        {
            return a.north == b.north && a.east == b.east && a.south == b.south && a.west == b.west;
        }
        
    }
}