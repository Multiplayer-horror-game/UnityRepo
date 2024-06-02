using System.Collections.Generic;
using System.Linq;
using Fase1.Math;
using Fase1.ScriptableObjects;
using UnityEngine;

namespace Fase1.MeshComponents
{
    public class NatureComponent : IChunkComponent
    {
        private NatureNoiseGenerator _generator;

        private NatureObjects _natureObjects;

        private Dictionary<GameObject, float> _processedObjects;
        
        public void ImplementChildren(WorldGenerator worldGenerator, GameObject parent, int xChunk, int yChunk)
        {
            Dictionary<Vector3,float> objectPositions = _generator.GenerateObjectOffsets(xChunk,yChunk,worldGenerator.physicalSize);
            
            foreach (var position in objectPositions)
            {
                worldGenerator.RequestNatureObject(new NatureObject(parent,position.Key,position.Value,ReturnRandomObject()));
            }
            
        }

        public NatureComponent(NoiseGenerator noiseGenerator, NatureObjects natureObjects)
        {
            _generator = noiseGenerator.ConvertToNatureNoiseGenerator();
            _natureObjects = natureObjects;

            _processedObjects = ProcessChances();
        }

        private Dictionary<GameObject,float> ProcessChances()
        {
            Dictionary<GameObject, int> objects = _natureObjects.GetDictionary();

            Dictionary<GameObject, float> result = new Dictionary<GameObject, float>();

            int total = 0;
            foreach (var value in objects.Values)
            {
                total += value;
            }

            foreach (var keyValuePair in objects)
            {
                result.Add(keyValuePair.Key,total / keyValuePair.Value);
            }

            return result;
        }

        private GameObject ReturnRandomObject()
        {
            foreach (var keyValuePair in _processedObjects)
            {
                float res = Random.Range(0, 100);

                if (keyValuePair.Value > res)
                {
                    return keyValuePair.Key;
                }
            }

            return _processedObjects.First().Key;
        }
        
    }
}