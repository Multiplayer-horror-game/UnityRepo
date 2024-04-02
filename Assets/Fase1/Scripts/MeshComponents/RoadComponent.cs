using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {
        private GameObject _referenceObject;
        
        private Dictionary<Vector2,float> _mainPositions = new();

        /// RoadRules
        private readonly int _nodeDistance = 200;

        private float _lastRotation = 0f;

        private readonly float _maxRotation = 17f;
        
        private NoiseGenerator _noiseGenerator;

        public RoadComponent(GameObject referenceObject, NoiseGenerator noiseGenerator)
        {
            _referenceObject = referenceObject;
            _mainPositions.Add(new Vector2(-100,-100), 0f);
            GenerateNewPositions(100);
            
            _noiseGenerator = noiseGenerator;
        }

        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            float[,] noise = _noiseGenerator.GenerateNoiseChunk(chunkPosition.x, chunkPosition.y);

            Vector2 startChunk = new Vector2(chunkPosition.x * physicalSize, chunkPosition.y * physicalSize);
            Vector2 endChunk = new Vector2((chunkPosition.x + 1) * physicalSize, (chunkPosition.y + 1) * physicalSize);

            List<Vector2> positions = new List<Vector2>();
            
            foreach (var pos in _mainPositions.Keys)
            {
                if (pos.x > startChunk.x && pos.x < endChunk.x && pos.y > startChunk.y && pos.y < endChunk.y)
                {
                    positions.Add(pos);
                }
            }
            
            List<Vector2> spline = RenderSpline(positions);
            
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
                Vector3 a = new Vector3(x, noise[x, y], y);
                Vector3 b = new Vector3((x + 1), noise[x + 1, y], y);
                Vector3 c = new Vector3(x, noise[x, y + 1], (y + 1));
                Vector3 d = new Vector3((x + 1), noise[x + 1, y + 1], (y + 1));
                
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



            //hardcoded the material index 0-0
            Dictionary<int, List<int>> combinedTriangles = new Dictionary<int, List<int>> { { 1, triangles } };

            return new[] { new MeshComponentData(combinedTriangles, vertices, uvs) };

        }

        private void UpdateSplinePositions()
        {
            
            foreach (var pos in _mainPositions.Keys)
            {
                float distance = Vector3.Distance(new Vector3(pos.x,0,pos.y),_referenceObject.transform.position);
            }
        }
        
        public Dictionary<Vector2,float> RenderSpline(Dictionary<Vector2,float> positions) {
            // Number of points on the curve
            List<Vector2> nodes = new List<Vector2>();

            int pos = positions.Count - 1;
            
            for (float t = 0; t <= 1f; t += 0.001f) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, positions.Keys.ToArray());
                float rotation = BezierCurve.CalculateCoordinate(t,0, positions.Values.ToArray());
                
                
                nodes.Add(point);
            }

            return nodes;

        }

        //based on the reference object randomly generate the road in front 
        private void GenerateNewPositions(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 lastPos = _mainPositions.Keys.Last();
                GenerateNewPosition(lastPos);
            }
            
            //the actual calculation
            void GenerateNewPosition(Vector2 lastPos)
            {
                _lastRotation = Random.Range(-_maxRotation, _maxRotation) + _lastRotation;
                
                Vector2 newPos = new Vector2(
                    lastPos.x + _nodeDistance * Mathf.Cos(_lastRotation),
                    lastPos.y + _nodeDistance * Mathf.Sin(_lastRotation)
                    );
                
                _mainPositions.Add(newPos,_lastRotation);
            }
        }
    }
}