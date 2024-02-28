using System.Collections.Generic;
using UnityEngine;

namespace Fase1
{
    public class DrawMesh
    {
        public Mesh CreateMeshChunk(float[,] chunkNoise, Material material, int verticesCount, float physicalSize)
        {
            Mesh mesh = new Mesh();
            
            List<int> triangles = new();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            
            float uvDistance = physicalSize / verticesCount;

            for (int x = 0; x < verticesCount - 1; x++)
            {
                for (int y = 0; y < verticesCount - 1 ; y++)
                {
                    Vector3[] v = GetVertices(x,y);
                    Vector2[] uv = GetUvs();
                    
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(vertices.Count - 1);
                        uvs.Add(uv[k]);
                    }

                }
            }
            
            mesh.SetVertices(vertices);
            mesh.SetUVs(0,uvs);

            mesh.SetTriangles(triangles.ToArray(),0, true, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            
            return mesh;

            Vector3[] GetVertices(int x, int y)
            {
                
                //each corner of the quad
                Vector3 a = new Vector3(x * uvDistance, chunkNoise[x, y], y * uvDistance);
                Vector3 b = new Vector3((x + 1) * uvDistance, chunkNoise[x + 1, y], y * uvDistance);
                Vector3 c = new Vector3(x * uvDistance, chunkNoise[x, y + 1], (y + 1) * uvDistance);
                Vector3 d = new Vector3((x + 1) * uvDistance, chunkNoise[x + 1, y + 1], (y + 1) * uvDistance);
                
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
        
        
        
    }
}