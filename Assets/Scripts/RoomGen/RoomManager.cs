using System;
using System.Collections.Generic;
using RoomGenerator.scripts.Structs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RoomGenerator.scripts
{
    public class RoomManager : MonoBehaviour
    {
        private MazeGenerator _mazeGenerator;

        //_size of the world grid
        public Vector2Int gridSize = new Vector2Int(10, 10);
    
        //room and offset
        private Dictionary<Room,Vector2Int> usedRooms = new ();
    

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
            _mazeGenerator = new MazeGenerator(hallways,this);
                
                
            bakedGrid = new int[gridSize.x, gridSize.y];

            RoomProperties[] staticRooms = Array.FindAll(rooms, room => room.isStatic);

            RoomProperties[] mandatoryRooms = Array.FindAll(rooms, room => room.mandatory && !room.isStatic);
            
            RoomProperties[] randomRooms = Array.FindAll(rooms, room => !room.mandatory && !room.isStatic);

            foreach (var room in staticRooms)
            {
                if(!CheckPositions(room.gameObject.GetComponent<Room>(), room.staticPosition))
                {
                    Debug.LogError("je hebt iets doms gedaan. " + room.gameObject.name + " kan helemaal niet op positie " + room.staticPosition);
                    continue;
                }
                
                SetGridPositions(room.gameObject.GetComponent<Room>(), room.staticPosition);
                
                Vector3 offset = room.gameObject.GetComponent<Room>().placementOffset;

                GameObject instantiatedRoom = Instantiate(room.gameObject, new Vector3(room.staticPosition.x * scale + offset.x, 0 + offset.y, room.staticPosition.y * scale + offset.z), new Quaternion());
                Room roomClass = instantiatedRoom.GetComponent<Room>();
                
                List<Vector2Int> realizedPositions = new List<Vector2Int>();
                
                foreach (Vector2Int roomClassPosition in roomClass.positions)
                {
                    realizedPositions.Add(new Vector2Int(roomClassPosition.x + room.staticPosition.x, roomClassPosition.y + room.staticPosition.y));
                }
                
                roomClass.positions = realizedPositions;
                
                usedRooms.Add(roomClass, new Vector2Int(room.staticPosition.x, room.staticPosition.y));
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

                Vector3 offset = room.gameObject.GetComponent<Room>().placementOffset;
                
                GameObject instantiatedRoom = Instantiate(room.gameObject, new Vector3((x * scale) + offset.x,0 + offset.y,(y * scale) + offset.z),new Quaternion());
                Room roomClass = instantiatedRoom.GetComponent<Room>();
                
                List<Vector2Int> realizedPositions = new List<Vector2Int>();
                
                foreach (Vector2Int roomClassPosition in roomClass.positions)
                {
                    realizedPositions.Add(new Vector2Int(roomClassPosition.x + x, roomClassPosition.y + y));
                }
                
                roomClass.positions = realizedPositions;
                
                usedRooms.Add(roomClass, new Vector2Int(x, y));
            }

            foreach (var room in randomRooms)
            {
                int count = Random.Range(1, room.maxAmount);
                
                int x = Random.Range(0, gridSize.x);
                int y = Random.Range(0, gridSize.y);

                int failsafe = 0;

                for (int i = 0; i < count; i++)
                {
                    while (!CheckPositions(room.gameObject.GetComponent<Room>(), new Vector2Int(x, y)) && failsafe < 100)
                    {
                        x = Random.Range(0, gridSize.x);
                        y = Random.Range(0, gridSize.y);
                        failsafe++;
                    }
                
                    if (failsafe >= 100)
                    {
                        Debug.LogError("Could not place random room in 100 tries. Exiting.");
                        break;
                    }
                
                    SetGridPositions(room.gameObject.GetComponent<Room>(), new Vector2Int(x, y));

                    Vector3 offset = room.gameObject.GetComponent<Room>().placementOffset;
                
                    GameObject instantiatedRoom = Instantiate(room.gameObject, new Vector3((x * scale) + offset.x,0 + offset.y,(y * scale) + offset.z),new Quaternion());
                    Room roomClass = instantiatedRoom.GetComponent<Room>();
                
                    List<Vector2Int> realizedPositions = new List<Vector2Int>();
                
                    foreach (Vector2Int roomClassPosition in roomClass.positions)
                    {
                        realizedPositions.Add(new Vector2Int(roomClassPosition.x + x, roomClassPosition.y + y));
                    }
                
                    roomClass.positions = realizedPositions;
                
                    usedRooms.Add(roomClass, new Vector2Int(x, y));
                    
                    failsafe = 0;
                }
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
            
            ModifyHallways(); 
            
            _mazeGenerator.InitializeRooms();
        }

        private void ModifyHallways()
        {
            foreach (var keyValuePair in usedRooms)
            {
                Room room = keyValuePair.Key;
                Vector2Int position = keyValuePair.Value;

                foreach (var door in room.doors)
                {
                    Debug.Log(door.directions);
                    if(door.directions.north)
                    {
                        Debug.Log("north" + position);
                        
                        Vector3 doorpos = new Vector3((position.x + door.position.x) * scale, 0, (position.y + door.position.y) * scale);
                        Debug.DrawLine(doorpos, doorpos + new Vector3(0,0,1 * scale), Color.red, 1000f);
                        
                        Directions dir = _mazeGenerator.GetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(0,1));
                        
                        dir.south = true;
                        
                        _mazeGenerator.SetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(0,1), dir);
                    }
                    
                    if(door.directions.east)
                    {
                        Debug.Log("east" + position);
                        
                        Vector3 doorpos = new Vector3((position.x + door.position.x) * scale, 0, (position.y + door.position.y) * scale);
                        Debug.DrawLine(doorpos, doorpos + new Vector3(1 * scale,0,0), Color.red, 1000f);
                        
                        Directions dir = _mazeGenerator.GetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(1,0));
                        
                        dir.west = true;
                        
                        _mazeGenerator.SetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(1,0), dir);
                    }
                    
                    if(door.directions.south)
                    {
                        Debug.Log("south" + position);
                        
                        Vector3 doorpos = new Vector3((position.x + door.position.x) * scale, 0, (position.y + door.position.y) * scale);
                        Debug.DrawLine(doorpos, doorpos + new Vector3(0,0,-1 * scale), Color.red, 1000f);
                        
                        Directions dir = _mazeGenerator.GetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(0,-1));
                        
                        dir.north = true;
                        
                        _mazeGenerator.SetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(0,-1), dir);
                    }
                    
                    if(door.directions.west)
                    {
                        Debug.Log("west" + position);
                        
                        Vector3 doorpos = new Vector3((position.x + door.position.x) * scale, 0, (position.y + door.position.y) * scale);
                        Debug.DrawLine(doorpos, doorpos + new Vector3(-1 * scale,0,0), Color.red, 1000f);
                        
                        Directions dir = _mazeGenerator.GetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(-1,0));
                        
                        dir.east = true;
                        
                        _mazeGenerator.SetHallway(position + new Vector2Int(door.position.x, door.position.y) + new Vector2Int(-1,0), dir);
                    }
                }
            }
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

                    return false;
                }
            }
            
            
            return CheckHallwaySpace(room, position);
        }

        //checks that if the room is placed if it has space for an additional hallway
        private bool CheckHallwaySpace(Room room, Vector2Int position)
        {
            foreach (Vector2Int pos in room.hallwayPositions)
            {
                if (!(pos.x + position.x < gridSize.x && pos.y + position.y < gridSize.y) || !(pos.x + position.x >= 0 && pos.y + position.y >= 0))
                {

                    return false;
                }
                
                if (bakedGrid[pos.x + position.x, pos.y + position.y] != 0)
                {

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

        public void AddHallway(GameObject hallwayObject, Vector2Int pos, Quaternion dir)
        {
            GameObject hallway = Instantiate(hallwayObject, new Vector3(pos.x * scale, 0, pos.y * scale), dir);
            hallway.transform.localScale = new Vector3(scale, scale, scale);
        }

    }
}