using System.Collections;
using System.Collections.Generic;
using Fase1.MeshComponents;
using UnityEngine;

namespace Fase1.Car
{
    public class CarBehavior : MonoBehaviour
    {
        public float speed = 10;
        
        public WorldGenerator worldGenerator;

        private RoadComponent _roadComponent;
        
        private int _currentControlPointIndex = 0;
        
        private KeyValuePair<Vector3,float> _currentControlPoint;
        private KeyValuePair<Vector3,float> _lastControlPoint;
        
        private Rigidbody rb;
        
        public void Start()
        {
            _roadComponent = worldGenerator.GetRoadComponent();

            rb = GetComponent<Rigidbody>();

            NextPoint();
            
        }

        IEnumerator Waiting()
        {
            yield return new WaitForSeconds(10f);
            
            NextPoint();
        }
        
        private void Update()
        {
            MoveOverSpline();
        }

        private void MoveOverSpline()
        {
            if(Mathf.Abs(_currentControlPoint.Key.magnitude - transform.position.magnitude) < 0.01f)
            {
                NextPoint();
            }
            
            rb.velocity = (_currentControlPoint.Key - _lastControlPoint.Key).normalized * speed;
            
            transform.rotation = Quaternion.Euler(0,Mathf.Lerp(_lastControlPoint.Value,_currentControlPoint.Value,1f),0);
            
            
        }
        
        private void NextPoint()
        {
            if (_currentControlPointIndex == 0)
            {
                _currentControlPoint = _roadComponent.GetControlPoint(_currentControlPointIndex);
            }
            
            _lastControlPoint = _currentControlPoint;
            _currentControlPointIndex++;
            _currentControlPoint = _roadComponent.GetControlPoint(_currentControlPointIndex);
        }
        
    }
}