using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace Fase1

{
    public class WorldGenerator : MonoBehaviour
    {
        private int _xOffset;
        private int _yOffset;
         
        private NoiseGenerator _noiseGenerator;
        
        private List<Thread> _threads = new();
        
        private Queue<MeshBuilder> _meshBuilders = new();
        

        private Dictionary<Vector2Int,GameObject> _chunks = new();

        private List<Vector2Int> _requestedChunks = new();

        private Queue<Vector2Int> _unInitialized = new();
         
        [Range(0.1f,500f)]
        public float heightMultiplier = 20f;

        [Range(-100f,100f)]
        public float heightOffset = -10f;

        [Range(0.001f, 100f)] 
        public float scale = 0.1f;
        
        [Range(1,10000f)]
        public int physicalSize = 100;

        [Range(6, 100)]
        public int verticesPerChunk;
        
        [Range(1,20)]
        public int renderDistance = 8;

        [Range(1,10)]
        public int multithreading = 1;

        public GameObject chunkPrefab;

        public GameObject mainObject;
        
        
        // Start is called before the first frame update
        void Start()
        {
            Random rnd = new Random();

            _xOffset = rnd.Next(-10000,10000);
            _yOffset = rnd.Next(-10000,10000);
            
            _noiseGenerator = new NoiseGenerator(scale, _xOffset, _yOffset, verticesPerChunk, heightMultiplier, heightOffset);
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            
            if(_meshBuilders.Count > 0)
            {
                MeshBuilder meshBuilder = _meshBuilders.Dequeue();
                BuildMesh(meshBuilder);
            }
            
            
            if(_threads.Count >= multithreading)
            {
                foreach (var thread in _threads)
                {
                    if (!thread.IsAlive)
                    {
                        _threads.Remove(thread);
                        break;
                    }
                }
            }
            
            if(_threads.Count < multithreading && _unInitialized.Count > 0)
            {
                
                if(_unInitialized.Count > 0)
                {
                    GenerateChunkAsync(_unInitialized.Peek());
                    _unInitialized.Dequeue();
                }
            }
            
            UpdateRenderList();
        }
        
        void GenerateChunkAsync(Vector2Int position)
        {
            ThreadStart threadStart = () =>
            {
                GenerateChunk(position);
            };

            Thread thread = new Thread(threadStart);
            thread.Start();
            
            _threads.Add(thread);
        }

        private void UpdateRenderList()
        {
            
            Vector3 position = mainObject.transform.position;
            
            Vector2Int chunkPosition = new Vector2Int((int)position.x / physicalSize,(int)position.z / physicalSize);
            
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                for (int y = -renderDistance; y < renderDistance; y++)
                {
                    Vector2Int chunk = new Vector2Int(chunkPosition.x + x, chunkPosition.y + y);
                    
                    if (!_requestedChunks.Contains(chunk) && Vector2.Distance(chunk,chunkPosition) <= renderDistance)
                    {
                        _requestedChunks.Add(chunk);
                        _unInitialized.Enqueue(chunk);
                    }
                }
            }

            List<Vector2Int> remove = new();
            foreach (var chunk in _requestedChunks)
            {
                if (Vector2.Distance(chunk,chunkPosition) > (renderDistance + 1))
                {
                    remove.Add(chunk);
                }
            }

            _requestedChunks.RemoveAll(item => remove.Contains(item));

            Dictionary<Vector2Int, GameObject> copy = new(_chunks);
            
            foreach (var chunk in copy)
            {
                if (!_requestedChunks.Contains(chunk.Key))
                {
                    Destroy(chunk.Value);
                    _chunks.Remove(chunk.Key);
                }
            }
            
        }
        
        private void GenerateChunk(object objectPosition)
        {
            Vector2Int position = (Vector2Int) objectPosition;
            
            float[,] noise = _noiseGenerator.GenerateNoiseChunk(position.x,position.y);
            MeshBuilder meshBuilder = new MeshBuilder(noise, verticesPerChunk, physicalSize, position);
            
            _meshBuilders.Enqueue(meshBuilder);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void BuildMesh(MeshBuilder meshBuilder)
        {
            Vector2Int position = meshBuilder.GetChunkPosition();
            Mesh mesh = meshBuilder.BuildMesh();
            
            GameObject meshObj = Instantiate(chunkPrefab, new Vector3(position.x*physicalSize - position.x ,0,position.y*physicalSize - position.y), new Quaternion(0,0,0,0));
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            meshObj.name = "Chunk (" + position.x + "," + position.y + ")";
            
            _chunks.Add(position,meshObj);
        }
    }
}
