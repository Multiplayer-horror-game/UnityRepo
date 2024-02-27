using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Fase1
{
    public class WorldGenerator : MonoBehaviour
    {
        private int _xOffset;
        private int _yOffset;

        [Range(0.1f,500f)]
        public float heightMultiplier = 20f;

        [Range(-100f,100f)]
        public float heightOffset = -10f;

        [Range(0.1f, 100f)] 
        public float scale = 0.1f;

        [Range(6, 3000)]
        public int verticesPerChunk;
        
        // Start is called before the first frame update
        void Start()
        {
            Random rnd = new Random();

            _xOffset = rnd.NextInt(-10000,10000);
            _yOffset = rnd.NextInt(-10000,10000);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
