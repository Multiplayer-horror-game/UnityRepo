using System.Collections.Generic;
using System.Linq;
using Fase1.MeshComponents;
using UnityEngine;

namespace Fase1
{
    public class MeshBuilder
    {
        public MeshState State { get; private set; } = MeshState.NotGenerated;
        
        private static readonly List<IMeshComponent> MeshComponents = new();
        
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector2> _uvs = new();

        private readonly Dictionary<int,List<int>> _triangles = new();
        
        private readonly Vector2Int _chunkPosition;
        
        private readonly float physicalSize = 200;
        private readonly int verticesCount = 100;
        
        
        public MeshBuilder(int verticesCount, float physicalSize, Vector2Int chunkPosition)
        {
            _chunkPosition = chunkPosition;
            
            this.verticesCount = verticesCount;
            this.physicalSize = physicalSize;
            
        }

        public void Start()
        {
            State = MeshState.Generating;
            
            foreach (
                var data in MeshComponents
                    .Select(meshComponent => meshComponent.GenerateMeshData(_chunkPosition, verticesCount, physicalSize))
                    .SelectMany(meshComponentData => meshComponentData)
            )
            {
                if(data.Empty) continue;
                
                _vertices.AddRange(data.Vertices);
                    
                _uvs.AddRange(data.Uvs);

                foreach (var triangle in data.Triangles)
                {
                    _triangles.Add(triangle.Key,triangle.Value);
                }
            }
            
            State = MeshState.Generated;
        }
        

        public Mesh BuildMesh()
        {
            Mesh mesh = new Mesh();
            
            
            mesh.SetVertices(_vertices);
            mesh.SetUVs(0,_uvs);
            
            foreach (KeyValuePair<int, List<int>> entry in _triangles)
            {
                mesh.SetTriangles(entry.Value.ToArray(),entry.Key, true, 0);  
            }

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            
            return mesh;
        }
        
        public Vector2Int GetChunkPosition()
        {
            return _chunkPosition;
        }
        
        public static void AddMeshComponent(IMeshComponent meshComponent)
        {
            MeshComponents.Add(meshComponent);
        }
        
    }
    
    public enum MeshState
    {
        NotGenerated,
        Generating,
        Generated
    }
}