using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fase1.MeshComponents
{
    public class RoadComponent : IMeshComponent
    {
        private GameObject _referenceObject;
        
        private Dictionary<Vector2,float> _mainPositions = new();
        public IReadOnlyDictionary<Vector2,float> Nodes => _mainPositions;
        
        private Dictionary<Vector2,float> _renderedPositions = new();
        
        public IReadOnlyDictionary<Vector2,float> RenderedNodes => _renderedPositions;

        /// RoadRules
        private readonly int _nodeDistance = 200;

        private float _lastRotation = 0f;

        private readonly float _maxRotation = 0.4f;
        
        private readonly float _smoothness = 60f;
        
        
        private NoiseGenerator _noiseGenerator;

        private List<Color> _colors = new List<Color>();
        
        private int lastColorIndex = 0;
        
        private float _physicalSize = 1000f;

        public RoadComponent(GameObject referenceObject, NoiseGenerator noiseGenerator)
        {
            _referenceObject = referenceObject;
            
            _colors.Add(Color.red);
            _colors.Add(Color.blue);
            _colors.Add(Color.green);
            _colors.Add(Color.yellow);
            _colors.Add(Color.cyan);
            _colors.Add(Color.magenta);
            _colors.Add(Color.black);
            _colors.Add(Color.white);
            _colors.Add(Color.gray);
            
            _mainPositions.Add(new Vector2(1,1),0);
            
            GenerateNewPositions(50);

            for (int i = 0; i < _mainPositions.Count - 1; i++)
            {
                RenderRoadPiece(_mainPositions.ElementAt(i),_mainPositions.ElementAt(i + 1));
            }
            
            _noiseGenerator = noiseGenerator;
            
        }
        
        public MeshComponentData[] GenerateMeshData(Vector2Int chunkPosition, int verticesCount, float physicalSize)
        {
            _physicalSize = physicalSize;
            
            //get the corners of the chunk
            OperatableVector2 chunkCorner0 = new Vector2(chunkPosition.x * physicalSize, chunkPosition.y * physicalSize);
            OperatableVector2 chunkCorner1 = new Vector2(chunkPosition.x * physicalSize + physicalSize, chunkPosition.y * physicalSize + physicalSize);
            
            //check all the road nodes in the chunk
            DubbelList<Vector2,float> nodes = GetRenderedRoadNodes(chunkCorner0,chunkCorner1);
            
            //if none return empty mesh
            if(nodes.Count == 0) return new[] { new MeshComponentData(new Dictionary<int, List<int>> {{1,new List<int>()}},null,null,true) };
            
            Color randomColor = _colors[lastColorIndex];
            lastColorIndex = (lastColorIndex + 1) % _colors.Count;

            //float[,] chunk = _noiseGenerator.GenerateNoiseChunk(chunkPosition.x,chunkPosition.y);
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Debug.DrawLine(
                    new Vector3(nodes.FirstValue[i].x, _noiseGenerator.GetNoiseValue(nodes.FirstValue[i].x,nodes.FirstValue[i].y), nodes.FirstValue[i].y),
                    new Vector3(nodes.FirstValue[i + 1].x, _noiseGenerator.GetNoiseValue(nodes.FirstValue[i + 1].x,nodes.FirstValue[i + 1].y), nodes.FirstValue[i + 1].y),
                    randomColor,
                    1000f
                    );
            }
            
            //generate the mesh
            List<int> triangles = new List<int>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                Vector3[] v;
                if (i == 0)
                {
                     v = GetVertices(nodes.GetPair(i),nodes.GetPair(i + 1), chunkPosition);
                }
                else
                {
                    v = GetVertices(nodes.GetPair(i - 1), nodes.GetPair(i), nodes.GetPair(i + 1), chunkPosition);
                }

                Vector2[] uv = GetUvs();
                
                for (int k = 0; k < 6; k++)
                {
                    vertices.Add(v[k]);
                    triangles.Add(vertices.Count - 1);
                    uvs.Add(uv[k]);
                }
            }
            
            Dictionary<int,List<int>> combinedTriangles = new Dictionary<int, List<int>> {{1,triangles}};

            return new[] { new MeshComponentData(combinedTriangles,vertices,uvs) };
        }



        private void RenderRoadPiece(KeyValuePair<Vector2,float> a, KeyValuePair<Vector2,float> b)
        {
            Vector2[] aTranslated = TranslateNode(a);
            Vector2[] bTranslated = TranslateNode(b);
            
            List<Vector2> nodes = new() {aTranslated[1],aTranslated[2],bTranslated[0],bTranslated[1]};
            
            for (float t = 0; t <= 1f; t += 0.01f) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, nodes.ToArray());
                float rotation = BezierCurve.CalculateCoordinate1D(t, new []{a.Value,b.Value});
                
                if (!_renderedPositions.ContainsKey(point))
                {
                    _renderedPositions.Add(point,rotation);
                }
            }
            
            
        }
        
        //translate to renderable spline positions
        private Vector2[] TranslateNode(KeyValuePair<Vector2,float> node)
        {
            Vector2[] nodes = new Vector2[3];
            nodes[1] = node.Key;
            
            nodes[0] = new Vector2(
                node.Key.x - _smoothness * Mathf.Cos(node.Value),
                node.Key.y - _smoothness * Mathf.Sin(node.Value)
            );
            
            nodes[2] = new Vector2(
                node.Key.x + _smoothness * Mathf.Cos(node.Value),
                node.Key.y + _smoothness * Mathf.Sin(node.Value)
            );
            
            return nodes;
        }
        
        private Vector3[] GetVertices(KeyValuePair<Vector2,float> a, KeyValuePair<Vector2,float> b, KeyValuePair<Vector2,float> c, Vector2Int chunk)
        {
            //each corner of the quad
            Vector2 middleAB = new Vector2((a.Key.x + b.Key.x) / 2, (a.Key.y + b.Key.y) / 2) - new Vector2(chunk.x * _physicalSize, chunk.y * _physicalSize);
            Vector2 middleBC = new Vector2((c.Key.x + b.Key.x) / 2, (c.Key.y + b.Key.y) / 2) - new Vector2(chunk.x * _physicalSize, chunk.y * _physicalSize);
            
            //x and z position
            float a1X = middleAB.x + 10 * Mathf.Cos(b.Value);
            float a1Z = middleAB.y + 10 * Mathf.Sin(b.Value);
            // y position
            float a1Y = _noiseGenerator.GetNoiseValue(a1X + (chunk.x * _physicalSize),a1Z + (chunk.y * _physicalSize));
            //combine
            Vector3 a1 = new Vector3(a1X, a1Y + 1f, a1Z);
            
            float b1X = middleAB.x - 10 * Mathf.Cos(b.Value);
            float b1Z = middleAB.y - 10 * Mathf.Sin(b.Value);
            float b1Y = _noiseGenerator.GetNoiseValue(b1X + (chunk.x * _physicalSize),b1Z + (chunk.y * _physicalSize));
            Vector3 b1 = new Vector3(b1X, b1Y + 1f, b1Z);
            
            float c1X = middleBC.x + 10 * Mathf.Cos(b.Value);
            float c1Z = middleBC.y + 10 * Mathf.Sin(b.Value);
            float c1Y = _noiseGenerator.GetNoiseValue(c1X + (chunk.x * _physicalSize),c1Z + (chunk.y * _physicalSize));
            Vector3 c1 = new Vector3(c1X, c1Y + 1f, c1Z);
            
            float d1X = middleBC.x - 10 * Mathf.Cos(b.Value);
            float d1Z = middleBC.y - 10 * Mathf.Sin(b.Value);
            float d1Y = _noiseGenerator.GetNoiseValue(d1X + (chunk.x * _physicalSize),d1Z + (chunk.y * _physicalSize));
            Vector3 d1 = new Vector3(d1X, d1Y + 1f, d1Z);
            
            return new[] { a1, c1, d1, a1, d1, b1 };
        }
        
        private Vector3[] GetVertices(KeyValuePair<Vector2,float> b, KeyValuePair<Vector2,float> c,Vector2Int chunk)
        {
            //each corner of the quad
            Vector2 middleAB = b.Key - new Vector2(chunk.x * _physicalSize, chunk.y * _physicalSize);
            Vector2 middleBC = new Vector2((c.Key.x + b.Key.x) / 2, (c.Key.y + b.Key.y) / 2) - new Vector2(chunk.x * _physicalSize, chunk.y * _physicalSize);
            
            //x and z position
            float a1X = middleAB.x + 10 * Mathf.Cos(b.Value);
            float a1Z = middleAB.y + 10 * Mathf.Sin(b.Value);
            // y position
            float a1Y = _noiseGenerator.GetNoiseValue(a1X + (chunk.x * _physicalSize),a1Z + (chunk.y * _physicalSize));
            //combine
            Vector3 a1 = new Vector3(a1X, a1Y + 1f, a1Z);
            
            float b1X = middleAB.x - 10 * Mathf.Cos(b.Value);
            float b1Z = middleAB.y - 10 * Mathf.Sin(b.Value);
            float b1Y = _noiseGenerator.GetNoiseValue(b1X + (chunk.x * _physicalSize),b1Z + (chunk.y * _physicalSize));
            Vector3 b1 = new Vector3(b1X, b1Y + 1f, b1Z);
            
            float c1X = middleBC.x + 10 * Mathf.Cos(b.Value);
            float c1Z = middleBC.y + 10 * Mathf.Sin(b.Value);
            float c1Y = _noiseGenerator.GetNoiseValue(c1X + (chunk.x * _physicalSize),c1Z + (chunk.y * _physicalSize));
            Vector3 c1 = new Vector3(c1X, c1Y + 1f, c1Z);
            
            float d1X = middleBC.x - 10 * Mathf.Cos(b.Value);
            float d1Z = middleBC.y - 10 * Mathf.Sin(b.Value);
            float d1Y = _noiseGenerator.GetNoiseValue(d1X + (chunk.x * _physicalSize),d1Z + (chunk.y * _physicalSize));
            Vector3 d1 = new Vector3(d1X, d1Y + 1f, d1Z);
            
            return new[] { a1, c1, d1, a1, d1, b1 };
        }
        
        Vector2[] GetUvs()
        {
            //corners of the UVs
            Vector2 uv00 = new Vector2(0f, 0f);
            Vector2 uv10 = new Vector2(1f, 0f);
            Vector2 uv01 = new Vector2(1f, 1f);
            Vector2 uv11 = new Vector2(1f, 1f);
            
            return new[] { uv00, uv10, uv11, uv00, uv11, uv01 };
        }
        
        private DubbelList<Vector2,float> GetRenderedRoadNodes(OperatableVector2 chunkCorner0, OperatableVector2 chunkCorner1)
        {
            DubbelList<Vector2,float> nodes = new();
            foreach (KeyValuePair<Vector2,float> node in _renderedPositions)
            {
                if (node.Key >= chunkCorner0 && node.Key <= chunkCorner1)
                {
                    nodes.Add(node.Key,node.Value);
                }
            }

            if (nodes.Count == 0) return nodes;

            int last = _renderedPositions.Keys.ToList().IndexOf(nodes.Last().Key);
            if (last + 1 < _renderedPositions.Count)
            {
                nodes.Add(_renderedPositions.ElementAt(last + 1).Key,_renderedPositions.ElementAt(last + 1).Value);
            }

            if (last + 2 < _renderedPositions.Count)
            {
                nodes.Add(_renderedPositions.ElementAt(last + 2).Key,_renderedPositions.ElementAt(last + 2).Value);
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
        }
        
        private void GenerateNewPosition(Vector2 lastPos)
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