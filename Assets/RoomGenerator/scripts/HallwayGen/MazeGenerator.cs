using System;
using System.Collections;
using System.Collections.Generic;
using RoomGenerator.scripts;
using RoomGenerator.scripts.Structs;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator
{
    
    private Dictionary<Vector2Int, Directions> _hallways = new();
    private List<Vector2Int> stack = new();
    private int visited = 0;
    
    public Vector2Int size = new Vector2Int(10, 10);

    public Vector2Int _currentPosition = new Vector2Int(0,0);
    
    //list with the objects that can be used as hallways
    private HallwayProperties[] _hallwayOptions;
    
    private RoomManager _roomManager;
    
    //constructor
    public MazeGenerator(HallwayProperties[] hallwayProperties, RoomManager roomManager)
    {
        _hallwayOptions = hallwayProperties;
        _roomManager = roomManager;
    }
    
    //Generate the hallways using the data from a roomManager
    public void GenerateHallways(int[,] bakedGrid, int size)
    {
        
        bool done = false;
        while (!done)
        {
            //if it can't find a new starting point, it's done
            if (!GetNewStartingPoint(bakedGrid, size)) done = true;
            
            bakedGrid[_currentPosition.x, _currentPosition.y] = 3;
            try
            {
                _hallways.Add(_currentPosition, new Directions()); //err here
            }
            catch (Exception ignored)
            {
                // ignored
            }

            bool canContinue = true;
            while (canContinue)
            {
                //get all possible room that can theoretically be visited 
                List<Vector2Int> canVisit = VisitableRooms();
                
                //check if they are not already done
                List<Vector2Int> sortedCanVisit = new List<Vector2Int>();
                foreach (var position in canVisit)
                {
                    if (bakedGrid[position.x, position.y] == 2)
                    {
                        sortedCanVisit.Add(position);
                    }
                }
                
                if (sortedCanVisit.Count > 0)
                {
                    //get a random room from the list of possible rooms
                    int numb = Random.Range(0, canVisit.Count - 1);

                    Vector2Int pos;

                    //try to get the position from the list, if it fails, get the first position
                    try
                    {
                        pos = sortedCanVisit[numb];
                    }
                    catch
                    {
                        pos = sortedCanVisit[0];
                    }
                    
                    //add the position to the stack history
                    stack.Add(pos);
                    
                    //update the last hallway with the new direction
                    _hallways[_currentPosition] = OpenDirection(_hallways[_currentPosition], pos);

                    //add a visited count to keep track of how many rooms have been visited
                    visited++;
                    
                    //update the grid position to remove it from the possible visitable rooms
                    bakedGrid[pos.x, pos.y] = 3;
                    
                    //set the new position and keep old position to update the new hallway
                    Vector2Int oldPos = _currentPosition;
                    _currentPosition = pos;
                    
                    Directions dir = new Directions();
                    
                    //add the new hallway to the dictionary
                    _hallways.Add(_currentPosition, OpenDirection(dir, oldPos));

                }
                else
                {
                    if (stack.Contains(_currentPosition))
                    {
                        stack.Remove(_currentPosition);
                    }

                    if (stack.Count != 0)
                    {
                        _currentPosition = stack[stack.Count - 1];
                    }
                    else
                    {
                        canContinue = false;
                    }
                }
            }
        }
        
        Debug.Log(_hallways.Count);
        
        //so after doing all the calculations
        //it will loop through all the hallways and add gameobjects to the scene
        foreach (var keyValuePair in _hallways)
        {
            
            Directions dir = keyValuePair.Value;
            Vector2Int pos = keyValuePair.Key;

            //loop through all the hallway options to get the fitting one
            foreach (var hallwayOption in _hallwayOptions)
            {
                //dami cracky way of operating with custom structs check the Directions.cs for more info XD
                Debug.Log(pos + " , " + dir);
                CompareResult compareResult = Directions.Compare(dir, hallwayOption.directions);
                if (compareResult.result)
                {
                    _roomManager.AddHallway(hallwayOption.hallway, pos, Quaternion.Euler(0, compareResult.rotation, 0));
                    break;
                }
            }
        }
    }
    

    //open the direction to the next room
    private Directions OpenDirection(Directions dir, Vector2Int pos)
    {
        //there can only be a change of 1 in the x or y axis
        Vector2Int change = pos - _currentPosition;
        
        //so one of these if statements will always be true
        if (change.x == 1) dir.east = true;
        
        if (change.x == -1) dir.west = true;
        
        if (change.y == 1) dir.north = true;
        
        if (change.y == -1) dir.south = true;
        
        return dir;
    }

    //get a new starting point for the generator
    private bool GetNewStartingPoint(int[,] bakedGrid, int size)
    {
        bool foundPos = false;
        //loop through the 2 dimensions of the baked grid array
        for (int x = 0; x < size && !foundPos; x++)
        {
            for (int y = 0; y < size && !foundPos; y++)
            {
                //if it finds a room that hasn't been visited yet, it sets it as the new starting point
                if(bakedGrid[x, y] == 2)
                {
                    _currentPosition = new Vector2Int(x, y);
                    foundPos = true;
                }
            }
        }
        
        //this will always return false if it can't find a new starting point
        return foundPos;
    }

    //check for all possible rooms that can be visited
    private List<Vector2Int> VisitableRooms()
    {
        List<Vector2Int> canVisit = new List<Vector2Int>();

        if (_currentPosition.x != 0)
        {
            if (!stack.Contains(new Vector2Int(_currentPosition.x - 1, _currentPosition.y)))
            {
                canVisit.Add(new Vector2Int(_currentPosition.x - 1, _currentPosition.y));
            }
        }
        if (_currentPosition.y != 0)
        {
            if (!stack.Contains(new Vector2Int(_currentPosition.x, _currentPosition.y - 1)))
            {
                canVisit.Add(new Vector2Int(_currentPosition.x, _currentPosition.y - 1));
            }
        }
        if (_currentPosition.x < size.x - 1)
        {
            if (!stack.Contains(new Vector2Int(_currentPosition.x + 1, _currentPosition.y)))
            {
                canVisit.Add(new Vector2Int(_currentPosition.x + 1, _currentPosition.y));
            }
        }
        if (_currentPosition.y < size.y - 1)
        {
            if (!stack.Contains(new Vector2Int(_currentPosition.x, _currentPosition.y + 1)))
            {
                canVisit.Add(new Vector2Int(_currentPosition.x, _currentPosition.y + 1));
            }
        }
        

        return canVisit;
    }
    
}
