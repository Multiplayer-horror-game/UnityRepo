using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {

        private List<Vector2> _positions = new();
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            throw new NotImplementedException();
        }
        
        public List<Vector2> RenderSpline(List<Vector2> positions) {
            // Number of points on the curve
            List<Vector2> nodes = new List<Vector2>();

            int pos = positions.Count - 1;
            
            for (float t = 0; t <= 1.05f; t += 0.001f) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, positions.ToArray());
                
                nodes.Add(point);
                Debug.Log(point);
            }

            return nodes;

        }
    }
}