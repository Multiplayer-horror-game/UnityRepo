using System.Collections.Generic;
using UnityEngine;

namespace Fase1
{
    public class MeshBuilder
    {
        private float[,] _chunkNoise;
        
        private readonly List<int> _triangles = new();
        private readonly List<Vector3> _vertices = new();
        private readonly List<Vector2> _uvs = new();
        
        private readonly Vector2Int _chunkPosition;

        
        public MeshBuilder(float[,] chunkNoise, int verticesCount, float physicalSize, Vector2Int chunkPosition)
        {
            _chunkNoise = chunkNoise;
            _chunkPosition = chunkPosition;
            
            
            float uvDistance = physicalSize / verticesCount;
            
            for (int x = 0; x < verticesCount - 1; x++)
            {
                for (int y = 0; y < verticesCount - 1 ; y++)
                {
                    Vector3[] v = GetVertices(x,y);
                    Vector2[] uv = GetUvs();
                    
                    for (int k = 0; k < 6; k++)
                    {
                        _vertices.Add(v[k]);
                        _triangles.Add(_vertices.Count - 1);
                        _uvs.Add(uv[k]);
                    }

                }
            }
            
            Vector3[] GetVertices(int x, int y)
            {
                
                //each corner of the quad
                Vector3 a = new Vector3(x * uvDistance, _chunkNoise[x, y], y * uvDistance);
                Vector3 b = new Vector3((x + 1) * uvDistance, _chunkNoise[x + 1, y], y * uvDistance);
                Vector3 c = new Vector3(x * uvDistance, _chunkNoise[x, y + 1], (y + 1) * uvDistance);
                Vector3 d = new Vector3((x + 1) * uvDistance, _chunkNoise[x + 1, y + 1], (y + 1) * uvDistance);
                
                return new[] { a, c, d, a, d, b };
            }

            Vector2[] GetUvs()
            {
                //corners of the UVs
                Vector2 uv00 = new Vector2(0f, 0f);
                Vector2 uv10 = new Vector2(1f, 0f);
                Vector2 uv01 = new Vector2(1f, 1f);
                Vector2 uv11 = new Vector2(1f, 1f);
                
                return new[] { uv00, uv10, uv01, uv10, uv11, uv01 };
            }
        }
        

        public Mesh BuildMesh()
        {
            Mesh mesh = new Mesh();
            
            mesh.SetVertices(_vertices);
            mesh.SetUVs(0,_uvs);

            mesh.SetTriangles(_triangles.ToArray(),0, true, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            
            return mesh;
        }

        public float[,] GetChunkNoise()
        {
            return _chunkNoise;
        }
        
        public Vector2Int GetChunkPosition()
        {
            return _chunkPosition;
        }
        
        
    }
}