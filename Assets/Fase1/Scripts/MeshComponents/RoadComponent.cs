using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {
        private GameObject _referenceObject;
        
        private int _physicalSize;
        
        private List<Vector2> _mainPositions = new();

        private List<Vector2> _translatedPositions = new();

        /// RoadRules
        private int _NodeDistance = 200;

        private float _lastRotation = 0f;

        private float _maxRotation = 17f;


        
        
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            throw new NotImplementedException();
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
                _mainPositions.Add();
            }
        }
        
        //
        
        
    }
}