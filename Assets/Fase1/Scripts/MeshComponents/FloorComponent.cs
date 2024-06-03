using System.Collections.Generic;
using UnityEngine;

namespace Fase1.MeshComponents
{
    public class FloorComponent : IMeshComponent
    {
        private NoiseGenerator _noiseGenerator;
        
        public FloorComponent(NoiseGenerator noiseGenerator)
        {
            _noiseGenerator = noiseGenerator;
        }
        
        
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition,int verticesCount,float physicalSize)
        {
            float[,] noise = _noiseGenerator.GenerateNoiseChunk(chunkPosition.x, chunkPosition.y);
            
            //distance between each vertex
            float vDistance = physicalSize / (verticesCount - 1);
            
            List<int> triangles = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            
            
            //calculate the vertices ,triangles and uvs
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
            
            Vector3[] GetVertices(int x, int y)
            {
                
                //each corner of the quad
                Vector3 a = new Vector3(x * vDistance, noise[x, y], y * vDistance);
                Vector3 b = new Vector3((x + 1) * vDistance, noise[x + 1, y], y * vDistance);
                Vector3 c = new Vector3(x * vDistance, noise[x, y + 1], (y + 1) * vDistance);
                Vector3 d = new Vector3((x + 1) * vDistance, noise[x + 1, y + 1], (y + 1) * vDistance);
                
                return new[] { a, c, d, a, d, b };
            }

            Vector2[] GetUvs()
            {
                //corners of the UVs
                Vector2 uv00 = new Vector2(0f, 0f);
                Vector2 uv10 = new Vector2(1f, 0f);
                Vector2 uv01 = new Vector2(0f, 1f);
                Vector2 uv11 = new Vector2(1f, 1f);
                
                return new[] { uv00, uv10, uv11, uv00, uv11, uv01 };
            }
            
            //hardcoded the material index 0-0
            Dictionary<int,List<int>> combinedTriangles = new Dictionary<int, List<int>> {{0,triangles}};
            
            return new[] {new MeshComponentData(combinedTriangles, vertices, uvs)};
        }
    }
}