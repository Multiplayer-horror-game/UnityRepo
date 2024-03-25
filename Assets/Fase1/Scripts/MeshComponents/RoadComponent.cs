using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {
        private GameObject _referenceObject;
        
        private List<Vector2> _mainPositions = new();

        /// RoadRules
        private readonly int _nodeDistance = 200;

        private float _lastRotation = 0f;

        private readonly float _maxRotation = 17f;
        
        private NoiseGenerator _noiseGenerator;

        public RoadComponent(GameObject referenceObject, NoiseGenerator noiseGenerator)
        {
            _referenceObject = referenceObject;
            _mainPositions.Add(new Vector2(-100, -100));
            GenerateNewPositions(100);
            
            _noiseGenerator = noiseGenerator;
        }




        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            return null;
        }
        
        public List<Vector2> RenderSpline(List<Vector2> positions) {
            // Number of points on the curve
            List<Vector2> nodes = new List<Vector2>();

            int pos = positions.Count - 1;
            
            for (float t = 0; t <= 1f; t += 0.001f) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, positions.ToArray());
                
                nodes.Add(point);
                Debug.Log(point);
            }

            return nodes;

        }

        //based on the reference object randomly generate the road in front 
        private void GenerateNewPositions(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 lastPos = _mainPositions.Last();
                GenerateNewPosition(lastPos);
            }
            
            //the actual calculation
            void GenerateNewPosition(Vector2 lastPos)
            {
                float rotation = Random.Range(-_maxRotation, _maxRotation);
                Vector2 newPos = new Vector2(lastPos.x + _nodeDistance * Mathf.Cos(rotation + _lastRotation), lastPos.y + _nodeDistance * Mathf.Sin(rotation + _lastRotation));
                
                _lastRotation = rotation;
                _mainPositions.Add(newPos);
            }
        }
        
        
        
    }
}