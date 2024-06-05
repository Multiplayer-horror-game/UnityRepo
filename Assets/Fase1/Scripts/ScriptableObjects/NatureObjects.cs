using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fase1.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NatureObjects", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
    public class NatureObjects : ScriptableObject
    {

        public List<GameObject> objects = new();

        public List<int> chances = new();

        public float distance = 10;

        public Dictionary<GameObject, int> GetDictionary()
        {
            Dictionary<GameObject, int> result = new Dictionary<GameObject, int>();
            

            for (int i = 0; i < objects.Count; i++)
            {
                result.Add(objects[i],chances[i]);
            }

            return result;
        }
    }
}