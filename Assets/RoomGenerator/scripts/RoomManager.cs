using System;
using System.Collections.Generic;
using RoomGenerator.scripts.Structs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomGenerator.scripts
{
    public class RoomManager : MonoBehaviour
    {
        private MazeGenerator _mazeGenerator = new MazeGenerator();

        //size of the world grid
        public Vector2Int gridSize = new Vector2Int(10, 10);
    
        //room and offset
        private Dictionary<Room,Vector2> usedRooms = new ();
    

        public RoomProperties[] rooms;
    
        public HallwayProperties[] hallways;

        //final grid layout 
        //0 - empty
        //1 - room
        //2 - requested hallway
        //3 - hallway
        private int[,] bakedGrid;

        public int scale;

        void Start()
        {
            bakedGrid = new int[gridSize.x, gridSize.y];

            RoomProperties[] staticRooms = Array.FindAll(rooms, room => room.isStatic);

            RoomProperties[] mandatoryRooms = Array.FindAll(rooms, room => room.mandatory && !room.isStatic);

            foreach (var room in staticRooms)
            {
                if(!CheckPositions(room.gameObject.GetComponent<Room>(), room.staticPosition))
                {
                    Debug.LogError("je hebt iets doms gedaan. " + room.gameObject.name + " kan helemaal niet op positie " + room.staticPosition);
                    continue;
                }

                SetGridPositions(room.gameObject.GetComponent<Room>(), room.staticPosition);

                GameObject instantiatedRoom = Instantiate(room.gameObject, new Vector3(room.staticPosition.x, 0, room.staticPosition.y), new Quaternion());
            }


            foreach (var room in mandatoryRooms)
            {
                int x = Random.Range(0, gridSize.x);
                int y = Random.Range(0, gridSize.y);

                int failsafe = 0;
                
                while (!CheckPositions(room.gameObject.GetComponent<Room>(), new Vector2Int(x, y)) && failsafe < 100)
                {
                    x = Random.Range(0, gridSize.x);
                    y = Random.Range(0, gridSize.y);
                    failsafe++;
                }
                
                if (failsafe >= 100)
                {
                    Debug.LogError("Could not place mandatory room in 100 tries. Exiting.");
                    return;
                }
                
                SetGridPositions(room.gameObject.GetComponent<Room>(), new Vector2Int(x, y));
                
                GameObject instantiatedRoom = Instantiate(room.gameObject, new Vector3(x,0,y),new Quaternion());
                Room roomClass = instantiatedRoom.GetComponent<Room>();
                
                List<Vector2Int> realizedPositions = new List<Vector2Int>();
                
                foreach (Vector2Int roomClassPosition in roomClass.positions)
                {
                    realizedPositions.Add(new Vector2Int(roomClassPosition.x + x, roomClassPosition.y + y));
                }
                
                roomClass.positions = realizedPositions;
                
            }
            
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    if (bakedGrid[x,y] == 0)
                    {
                        bakedGrid[x, y] = 2;
                    }
                }
            }
            
            _mazeGenerator.GenerateHallways(bakedGrid, gridSize.x);
            
            
        }

        private bool CheckPositions(Room room, Vector2Int position)
        {
            foreach (Vector2Int pos in room.positions)
            {
                if (!(pos.x + position.x < gridSize.x && pos.y + position.y < gridSize.y) || !(pos.x + position.x >= 0 && pos.y + position.y >= 0))
                {
                    Debug.Log("Room out of bounds at " + position);
                    return false;
                }
                
                if(bakedGrid[pos.x + position.x,pos.y + position.y] != 0)
                {
                    Debug.Log("Room overlaps at " + position);
                    return false;
                }
            }

            Debug.Log("Room placed at " + position);
            
            return CheckHallwaySpace(room, position);
        }

        //checks that if the room is placed if it has space for an additional hallway
        private bool CheckHallwaySpace(Room room, Vector2Int position)
        {
            foreach (Vector2Int pos in room.hallwayPositions)
            {
                if (!(pos.x + position.x < gridSize.x && pos.y + position.y < gridSize.y) || !(pos.x + position.x >= 0 && pos.y + position.y >= 0))
                {
                    Debug.Log("Hallway out of bounds at " + position);
                    return false;
                }
                
                if (bakedGrid[pos.x + position.x, pos.y + position.y] != 0)
                {
                    Debug.Log("Hallway overlaps at " + position);
                    return false;
                }
            }
            
            return true;
        }
        
        private void SetGridPositions(Room room, Vector2Int position)
        {
            foreach (Vector2Int pos in room.positions)
            {
                bakedGrid[pos.x + position.x,pos.y + position.y] = 1;
            }
            
            SetGridHallwayPositions(room, position);
        }
        
        private void SetGridHallwayPositions(Room room, Vector2Int position)
        {
            foreach (Vector2Int pos in room.hallwayPositions)
            {
                bakedGrid[pos.x + position.x,pos.y + position.y] = 2;
            }
        }
    
        private void OnDrawGizmos()
        {
            if (bakedGrid != null)
            {
                Gizmos.color = Color.green;
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        Vector3 center = new Vector3(x * scale, 0, y * scale);
                        Gizmos.DrawWireCube(center, Vector3.one);
                        switch (bakedGrid[x,y])
                        {
                            case 1:
                                Gizmos.color = Color.red;
                                Gizmos.DrawCube(center, Vector3.one);
                                break;
                            case 2:
                                Gizmos.color = Color.blue;
                                Gizmos.DrawCube(center, Vector3.one);
                                break;
                        }
                        Gizmos.color = Color.green;
                        
                    }
                }
            }
        }
    }
}