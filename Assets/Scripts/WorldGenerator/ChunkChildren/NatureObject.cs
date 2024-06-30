using UnityEngine;

namespace Fase1.MeshComponents
{
    public struct NatureObject
    {
        GameObject _parent;
        Vector3 _position;
        float _rotation;
        GameObject _object;
        Vector3 _tPosition;
        
        public NatureObject(GameObject parent, Vector3 position, float rotation, GameObject obj)
        {
            _parent = parent;
            _position = position;
            _rotation = rotation;
            _object = obj;
            _tPosition = new Vector3(position.x + parent.transform.position.x, 0, position.z + parent.transform.position.z);
        }
        
        public GameObject Instantiate(WorldGenerator generator)
        {
            return generator.RequestTree(_parent, _object, _position, Quaternion.Euler(0, _rotation, 0));
        }
        
        public Vector2 CheckDistance(Vector3 position)
        {
            return new Vector2(position.x - _tPosition.x, position.z - _tPosition.z);
        }
        
        public GameObject GetParent()
        {
            return _parent;
        }
    }
}