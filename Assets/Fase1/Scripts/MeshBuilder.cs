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
                var componentData in MeshComponents
                    .Select(meshComponent => meshComponent.GenerateMeshData(_chunkPosition, verticesCount, physicalSize))
                    .SelectMany(meshComponentData => meshComponentData)
            )
            {
                if (componentData.Empty)
                {
                    Dictionary<int,List<int>> fake = componentData.Triangles;
                    _triangles.Add(fake.First().Key,fake.First().Value);
                    continue;
                }
                
                _vertices.AddRange(componentData.Vertices);
                    
                _uvs.AddRange(componentData.Uvs);
                
                int totalTriangles = 0;
                if(_triangles.Count != 0)
                {
                    totalTriangles = _triangles.Values.Sum(triangle => triangle.Count);
                }

                foreach (var triangle in componentData.Triangles)
                {
                    List<int> triangleList = new List<int>();
                    
                    foreach (var triangleIndex in triangle.Value)
                    {
                        triangleList.Add(triangleIndex + totalTriangles);
                    }

                    if (!_triangles.TryAdd(triangle.Key, triangleList))
                    {
                        State = MeshState.Failed;
                        return;
                    }
                }
            }
            
            State = MeshState.Generated;
        }
        

        public Mesh BuildMesh()
        {
            Mesh mesh = new Mesh();
            
            mesh.subMeshCount = _triangles.Count;
            
            mesh.SetVertices(_vertices);
            mesh.SetUVs(0,_uvs);
            
            foreach (KeyValuePair<int, List<int>> entry in _triangles)
            {
                mesh.SetTriangles(entry.Value.ToArray(),entry.Key, true);  
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
        Generated,
        Failed
    }
}