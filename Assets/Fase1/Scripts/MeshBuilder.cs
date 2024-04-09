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

        private static readonly List<IChunkComponent> ChunkComponents = new();
        
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector2> _uvs = new();

        private readonly Dictionary<int,List<int>> _triangles = new();
        
        private readonly Vector2Int _chunkPosition;
        
        
        public MeshBuilder(int verticesCount, float physicalSize, Vector2Int chunkPosition)
        {
            _chunkPosition = chunkPosition;
            
            State = MeshState.Generating;
            foreach (
                var data in MeshComponents
                    .Select(meshComponent => meshComponent.GenerateMeshData(_chunkPosition, verticesCount, physicalSize))
                    .SelectMany(meshComponentData => meshComponentData)
                )
            {
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
        
        public void ChildOperation(WorldGenerator worldGenerator, GameObject gameObject)
        {
            foreach (var component in ChunkComponents)
            {
                component.ImplementChildren(worldGenerator, gameObject,_chunkPosition.x,_chunkPosition.y);
            }
        }
        
        public Vector2Int GetChunkPosition()
        {
            return _chunkPosition;
        }
        
        public static void AddMeshComponent(IMeshComponent meshComponent)
        {
            MeshComponents.Add(meshComponent);
        }

        public static void AddChildren(IChunkComponent component)
        {
            ChunkComponents.Add(component);
        }

    }
    
    
    public enum MeshState
    {
        NotGenerated,
        Generating,
        Generated
    }
}