using System.Collections.Generic;
using UnityEngine;

namespace Fase1
{
    public class DrawMesh
    {
        public Mesh CreateMeshChunk(float[,] chunkNoise, Material material, int verticesCount)
        {
            Mesh mesh = new Mesh();
            
            List<int> Triangles = new();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            for (int x = 0; x < verticesCount; x++)
            {
                for (int y = 0; y < verticesCount; y++)
                {
                    Vector3[] v = quad.GetVerts(cell, x, z);
                    Vector2[] uv = quad.GetUVs(cell);
                    
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        Wtriangles.Add(vertices.Count - 1);
                        uvs.Add(uv[k]);
                    }

                }
            }
            
            return mesh;

            Vector3[] GetVertices()
            {

                return null;
            }

            Vector2[] GetUvs()
            {


                return null;
            }
        }
        
        
        
    }
}