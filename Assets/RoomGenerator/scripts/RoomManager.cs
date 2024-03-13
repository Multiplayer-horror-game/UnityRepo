using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    //posible rooms to spawn
    public List<GameObject> rooms;

    //size of the world grid
    public Vector2Int gridSize = new Vector2Int(10, 10);

    //the amount of rooms you want to generate
    public int preferdRooms = 5;
    
    //room and offset
    private Dictionary<Room,Vector2> usedRooms = new ();

    //final grid layout (not needed for anything rn)
    private bool[,] bakedGrid;
    void Start()
    {
        bakedGrid = new bool[gridSize.x,gridSize.y];

        bool noSpace = false;
        int roomsGenerated = 0;
        while (!noSpace && roomsGenerated != preferdRooms)
        {
            int x = Random.Range(0, gridSize.x);
            int y = Random.Range(0, gridSize.y);
            

            if (!bakedGrid[x, y])
            {

                int roomNumber = Random.Range(0,rooms.Count);
                
                GameObject instantiatedRoom = Instantiate(rooms[roomNumber], new Vector3(x,0,y),new Quaternion());
                Room roomClass = instantiatedRoom.GetComponent<Room>();
                
                foreach (Vector2Int pos in roomClass.positions)
                {
                    if (!(pos.x + x < gridSize.x && pos.y + y < gridSize.y))
                    {
                        Destroy(instantiatedRoom);

                        foreach (Vector2Int pos2 in roomClass.positions)
                        {
                            if (pos2.x + x < gridSize.x && pos2.y + y < gridSize.y)
                            {
                                bakedGrid[pos2.x + x, pos2.y + y] = false;
                            }
                        }
                        
                        continue;
                    }
                    bakedGrid[pos.x + x, pos.y + y] = true;
                }

                usedRooms.Add(roomClass, new Vector2(x,y));
                roomsGenerated++;
            }
        }
        
        foreach (var room in usedRooms)
        {
             Debug.Log(new Vector3(room.Value.x,0,room.Value.y));
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
                    Vector3 center = new Vector3(x, 0, y);
                    Gizmos.DrawWireCube(center, Vector3.one);
                    if (bakedGrid[x, y])
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(center, Vector3.one * 0.8f);
                        Gizmos.color = Color.green;
                    }
                }
            }
        }
    }
    
    void Update()
    {
        
    }
}
