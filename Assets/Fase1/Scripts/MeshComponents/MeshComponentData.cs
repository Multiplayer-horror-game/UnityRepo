using System.Collections.Generic;
using UnityEngine;

namespace Fase1.MeshComponents
{
    public class MeshComponentData
    {

        public readonly bool Empty;
        public readonly Dictionary<int,List<int>> Triangles;
        public readonly List<Vector3> Vertices;
        public readonly List<Vector2> Uvs;
        
        public MeshComponentData(Dictionary<int,List<int>> triangles, List<Vector3> vertices, List<Vector2> uvs)
        {
            Triangles = triangles;
            Vertices = vertices;
            Uvs = uvs;
        }
        
        public MeshComponentData(Dictionary<int,List<int>> triangles, List<Vector3> vertices, List<Vector2> uvs, bool empty)
        {
            Triangles = triangles;
            Vertices = vertices;
            Uvs = uvs;
            this.Empty = empty;
        }
    }
}