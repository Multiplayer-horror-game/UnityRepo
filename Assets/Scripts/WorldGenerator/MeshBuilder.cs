﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fase1
{
    public class MeshBuilder
    {
        //the calculation state the mesh is currently in
        public MeshState State { get; private set; } = MeshState.NotGenerated;
        
        //list of all the mesh components
        private static readonly List<IMeshComponent> MeshComponents = new();

        //list of all the chunk components
        private static readonly List<IChunkComponent> ChunkComponents = new();
        
        //the verts, uvs and triangles for the mesh so the game knows what to render
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector2> _uvs = new();
        private readonly Dictionary<int,List<int>> _triangles = new();
        
        //the x y position of the chunk used mostly for identification
        private readonly Vector2Int _chunkPosition;
        
        //the physical size of the chunk and the amount of vertices changed on constructor
        private readonly float physicalSize = 200;
        private readonly int verticesCount = 100;
        
        
        public MeshBuilder(int verticesCount, float physicalSize, Vector2Int chunkPosition)
        {
            //set identifier
            _chunkPosition = chunkPosition;
            
            //replace the default values
            this.verticesCount = verticesCount;
            this.physicalSize = physicalSize;
            
        }

        public void Start()
        {
            State = MeshState.Generating;
            
            /*
             * for all the mesh components that need to be generated on the mesh it will run the necessary calculations
             * Components are added through the static method AddMeshComponent
             */
            foreach (
                var componentData in MeshComponents
                    .Select(meshComponent => meshComponent.GenerateMeshData(_chunkPosition, verticesCount, physicalSize))
                    .SelectMany(meshComponentData => meshComponentData)
            )
            {
                //if the component is empty it will make a fake so we don't mess up the textures
                if (componentData.Empty)
                {
                    Dictionary<int,List<int>> fake = componentData.Triangles;
                    _triangles.Add(fake.First().Key,fake.First().Value);
                    continue;
                }
                
                _vertices.AddRange(componentData.Vertices);
                    
                _uvs.AddRange(componentData.Uvs);
                
                int totalTriangles = 0;
                
                //the triangle count is not split up in the sub meshes so that needs to be continued as number not starting from 0 every submesh
                if(_triangles.Count != 0)
                {
                    totalTriangles = _triangles.Values.Sum(triangle => triangle.Count);
                }

                //same for the triangles them selves
                foreach (var triangle in componentData.Triangles)
                {
                    List<int> triangleList = new List<int>();
                    
                    foreach (var triangleIndex in triangle.Value)
                    {
                        triangleList.Add(triangleIndex + totalTriangles);
                    }

                    if(!_triangles.TryAdd(triangle.Key, triangleList))
                    {
                        State = MeshState.Generated;
                        return;
                    }
                }
            }
            
            //state to done now it would be able to be placed as object
            State = MeshState.Generated;
        }
        

        //this function runs on the main thread instead of the background thread cus Meshes can only be created on the main thread
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
        
        //this places all the children objects that need to be placed on the chunk for example trees
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
        
        //this is used to add a mesh component to the mesh builder
        public static void AddMeshComponent(IMeshComponent meshComponent)
        {
            MeshComponents.Add(meshComponent);
        }

        //this is used to add a chunk component to the mesh builder
        public static void AddChildren(IChunkComponent component)
        {
            ChunkComponents.Add(component);
        }

    }
    
    //the state of the mesh
    public enum MeshState
    {
        NotGenerated,
        Generating,
        Generated,
        Failed
    }
}