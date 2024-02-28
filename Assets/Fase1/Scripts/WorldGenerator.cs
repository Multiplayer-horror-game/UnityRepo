using UnityEngine;
using Random = System.Random;

namespace Fase1
{
    public class WorldGenerator : MonoBehaviour
    {
        private int _xOffset;
        private int _yOffset;
        private NoiseGenerator _noiseGenerator;
        private DrawMesh _drawMesh;

        [Range(0.1f,500f)]
        public float heightMultiplier = 20f;

        [Range(-100f,100f)]
        public float heightOffset = -10f;

        [Range(0.001f, 100f)] 
        public float scale = 0.1f;
        
        [Range(0.1f,10000f)]
        public float physicalSize = 100;

        [Range(6, 100)]
        public int verticesPerChunk;

        [Range(1,100)]
        public int width;
        [Range(1,100)]
        public int height;
        
        public GameObject chunkPrefab;
        
        // Start is called before the first frame update
        void Start()
        {
            Random rnd = new Random();

            _xOffset = rnd.Next(-10000,10000);
            _yOffset = rnd.Next(-10000,10000);
            
            _noiseGenerator = new NoiseGenerator(scale, _xOffset, _yOffset, verticesPerChunk, heightMultiplier, heightOffset);
            _drawMesh = new DrawMesh();
            
            GenerateWorld();
        }
        
        private void GenerateWorld()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float[,] noise = _noiseGenerator.GenerateNoiseChunk(x,y);
                    Mesh mesh = _drawMesh.CreateMeshChunk(noise, null, verticesPerChunk,physicalSize);
                    GameObject meshObj = Instantiate(chunkPrefab, new Vector3(x*physicalSize - x ,0,y*physicalSize - y), new Quaternion(0,0,0,0));
                    meshObj.AddComponent<MeshFilter>().mesh = mesh;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
