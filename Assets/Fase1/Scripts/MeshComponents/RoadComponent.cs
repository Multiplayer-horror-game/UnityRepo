using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {

        private List<Vector2> positions = new List<Vector2>();
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            throw new NotImplementedException();
        }
        
        public void RenderSpline(int index) {
            // Number of points on the curve
            List<Vector2> nodes = new List<Vector2>();

            int pos = positions.Count;

            for (float t = 0; t <= 1; t += 1 / 3) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, new []{positions[pos - 2],positions[pos - 1], positions[pos]});
                nodes.Add(point);
            }
            
        }
    }
}