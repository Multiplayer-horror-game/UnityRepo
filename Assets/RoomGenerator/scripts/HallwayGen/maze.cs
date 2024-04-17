using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class maze : MonoBehaviour
{
    public  List<Vector2Int> stack = new List<Vector2Int>();
    public int visited = 0;
    public Room[,] rooms;

    public Vector2Int size = new Vector2Int(10, 10);

    public Vector2Int currentPosition = new Vector2Int(0,0);

    // Start is called before the first frame update
    void Start()
    {
        rooms = new Room[size.x, size.y];

        visited++;
        stack.Add(currentPosition);
        rooms[currentPosition.x, currentPosition.y] = new Room();

    }

    // Update is called once per frame
    void Update()
    {
        List<Vector2Int> canVisit = new List<Vector2Int>();

        if (currentPosition.x != 0)
        {
            if (!stack.Contains(new Vector2Int(currentPosition.x - 1, currentPosition.y)))
            {
                canVisit.Add(new Vector2Int(currentPosition.x - 1, currentPosition.y));
            }
        }
        if (currentPosition.y != 0)
        {
            if (!stack.Contains(new Vector2Int(currentPosition.x, currentPosition.y - 1)))
            {
                canVisit.Add(new Vector2Int(currentPosition.x, currentPosition.y - 1));
            }
        }
        if (currentPosition.x < size.x - 1)
        {
            if (!stack.Contains(new Vector2Int(currentPosition.x + 1, currentPosition.y)))
            {
                canVisit.Add(new Vector2Int(currentPosition.x + 1, currentPosition.y));
            }
        }
        if (currentPosition.y < size.y - 1)
        {
            if (!stack.Contains(new Vector2Int(currentPosition.x, currentPosition.y + 1)))
            {
                canVisit.Add(new Vector2Int(currentPosition.x, currentPosition.y + 1));
            }
        }

        List<Vector2Int> sortedCanVisit = new List<Vector2Int>();
        foreach (var item in canVisit)
        {
            if (rooms[item.x, item.y] == null)
            {
                sortedCanVisit.Add(item);
            }
        }


        if (sortedCanVisit.Count > 0)
        {
            Vector2Int pos = sortedCanVisit[Random.Range(0, canVisit.Count)];


            stack.Add(pos);

            rooms[currentPosition.x, currentPosition.y].openDirection(currentPosition, pos);
            //rooms[pos.x, pos.y].openDirection(pos, currentPosition);

            visited++;
            currentPosition = pos;
            rooms[currentPosition.x, currentPosition.y] = new Room();

        }
        else
        {
            if (stack.Contains(currentPosition))
            {
                stack.Remove(currentPosition);
            }

            if (stack.Count != 0)
            {
                currentPosition = stack[stack.Count - 1];
            }

        }

    }

    void OnDrawGizmos()
    {
        if (rooms == null) return;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (rooms[x, y] != null)
                {
                    Gizmos.color = Color.white;
                    Vector3 position = new Vector3(x, y, 0);
                    Gizmos.DrawCube(position, Vector3.one);

                    Debug.Log(position);


                    Room room = rooms[x, y];
                    if(room.north == false)
                    {
                        Gizmos.DrawLine(position + new Vector3(-0.5f, 0.5f, 0), position + new Vector3(0.5f, 0.5f, 0));
                    }
                    if(room.south == false)
                    {
                        Gizmos.DrawLine(position + new Vector3(-0.5f, -0.5f, 0), position + new Vector3(0.5f, -0.5f, 0));
                    }
                    if(room.east == false)
                    {
                        Gizmos.DrawLine(position + new Vector3(0.5f, 0.5f, 0), position + new Vector3(0.5f, -0.5f, 0));
                    }
                    if(room.west == false)
                    {
                        Gizmos.DrawLine(position + new Vector3(0.5f, 0.5f, 0), position + new Vector3(0.5f, -0.5f, 0));
                    }
                }
            }
        }
    }
}
