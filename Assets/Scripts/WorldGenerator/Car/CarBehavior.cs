using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fase1.MeshComponents;
using UnityEngine;

namespace Fase1.Car
{
    public class CarBehavior : MonoBehaviour
    {
        
        public float speed = 1f;
        
        public int loops = 1000;
        
        public WorldGenerator worldGenerator;

        private RoadComponent _roadComponent;
        
        private float t = 0;
        
        private Rigidbody rb;

        private int _mainIndex = 0;
        
        private Dictionary<Vector3,float> _mainPositions;
        
        private List<Vector2> _controlPoints;
        
        private NoiseGenerator _noiseGenerator;
        
        Transform transform1;
        
        public void Start()
        {
            
            StartCoroutine(Waiting());

            rb = GetComponent<Rigidbody>();
            
            transform1 = transform;
        }

        IEnumerator Waiting()
        {
            yield return new WaitForSeconds(1f);
            
            _roadComponent = worldGenerator.GetRoadComponent();
            
            _mainPositions = _roadComponent.GetControlPoints();
            
            RenderRoadPiece(_mainPositions.ElementAt(0),_mainPositions.ElementAt(1));
        }
        
        private void Update()
        {
            MoveOverSpline();
        }

        private void MoveOverSpline()
        {
            if(_roadComponent == null)
                return;

            t += 1f / loops * speed * Time.deltaTime;

            if (t >= 1)
            {
                t = 1f;
            }
            
            Vector2 position = BezierCurve.DeCasteljau(_controlPoints, t);
            
            if (_noiseGenerator != null)
            {
                transform1.position = new Vector3(position.x,_noiseGenerator.GetNoiseValue(position.x,position.y) + 1,position.y - 4f);
            }
            else
            {
                transform1.position = new Vector3(position.x,transform.position.y,position.y - 4f);
            }
            
            
            transform.rotation = Quaternion.Euler(0,GetRotation(position,BezierCurve.DeCasteljau(_controlPoints, t + 0.01f)),0);
            
            if (t == 1)
            {
                _mainIndex++;
                RenderRoadPiece(_mainPositions.ElementAt(_mainIndex),_mainPositions.ElementAt(_mainIndex + 1));
                t = 0;
            }
        }
        
        private void RenderRoadPiece(KeyValuePair<Vector3,float> a, KeyValuePair<Vector3,float> b)
        {
            _controlPoints = new List<Vector2>();
            
            Vector2[] aTranslated = TranslateNode(a);
            Vector2[] bTranslated = TranslateNode(b);
            
            List<Vector2> nodes = new() {aTranslated[1],aTranslated[2],bTranslated[0],bTranslated[1]};
            
            for (float t = 0; t <= 1f; t += 0.01f) {
                Vector2 point = BezierCurve.CalculateBezierPoint(t, nodes.ToArray());
                
                if (!_controlPoints.Contains(point))
                {
                    _controlPoints.Add(point);
                }
            }
        }
        
        private Vector2[] TranslateNode(KeyValuePair<Vector3,float> node)
        {
            Vector2[] nodes = new Vector2[3];
            nodes[1] = new Vector2(node.Key.x,node.Key.z);
            
            nodes[0] = new Vector2(
                node.Key.x - 100f * Mathf.Cos(node.Value),
                node.Key.z - 100f * Mathf.Sin(node.Value)
            );
            
            nodes[2] = new Vector2(
                node.Key.x + 100f * Mathf.Cos(node.Value),
                node.Key.z + 100f * Mathf.Sin(node.Value)
            );
            
            return nodes;
        }
        
        private float GetRotation(Vector2 point, Vector2 nextPoint)
        {
            return -Mathf.Atan2(nextPoint.y - point.y, nextPoint.x - point.x) * Mathf.Rad2Deg;
        }

        public void OnDrawGizmos()
        {
            if (_controlPoints == null)
                return;
            
            for (int i = 0; i < _controlPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(_controlPoints[i].x,0,_controlPoints[i].y),new Vector3(_controlPoints[i + 1].x,0,_controlPoints[i + 1].y));
            }
        }

        public void GiftNoiseGenerator(NoiseGenerator noiseGenerator)
        {
            _noiseGenerator = noiseGenerator;
        }
    }
}