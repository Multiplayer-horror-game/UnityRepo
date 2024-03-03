using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = System.Random;

namespace Fase1

/*
 * hello future dami :)
 * omdat je geen zin meer had gisteren heb je dit voor jezelf achtergelaten
 * zorg ervoor dat je een lijst hebt met de chunks die je moet hebben geladen hier moet gewoon alles in
 * dan kan je checken wanneer er eentje is die niet in de lijst staat dat die weg moet
 * en dan natuurlijk de lisjt met de dingen die je wel al in de scene hebt
 * en de queue met de dingen die je nog moet renderen
 * gr je beste vriend dami
 */
{
    public class WorldGenerator : MonoBehaviour
    {
        private int _xOffset;
        private int _yOffset;
         
        private NoiseGenerator _noiseGenerator;
        
        private Dictionary<Vector2Int,GameObject> _loadedChunks = new();
        private Queue<Vector2Int> _renderList = new();
        
        private List<Thread> _threads = new();
        
        private Queue<MeshBuilder> _meshBuilders = new();
        

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
        public int width;
        [Range(1,20)]
        public int height;
        
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
            
            UpdateRenderList();
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            Debug.Log(_loadedChunks.Count + " " + _renderList.Count);
            
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
            
            if(_threads.Count < multithreading && _renderList.Count > 0)
            {
                bool chunkInRange = false;
                
                Vector3 position = mainObject.transform.position;
            
                Vector2Int chunkPosition = new Vector2Int((int)position.x / physicalSize,(int)position.z / physicalSize);
                
                while (!chunkInRange && _renderList.Count > 0)
                {
                    Vector2Int chunk = _renderList.Peek();
                    if (Vector2.Distance(chunk, chunkPosition) <= renderDistance)
                    {
                        chunkInRange = true;
                    }
                    else
                    {
                        _renderList.Dequeue();
                    }
                }
                
                if(_renderList.Count > 0)
                {
                    GenerateChunkAsync(_renderList.Peek());
                    _renderList.Dequeue();
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
                    
                    if (!_loadedChunks.ContainsKey(chunk) && !_renderList.Contains(chunk))
                    {
                        //Debug.Log("Adding chunk to render list" + chunk);
                        _renderList.Enqueue(chunk);
                    }
                }
            }
            
            foreach (var chunk in _loadedChunks)
            {
                if (Vector2.Distance(chunk.Key,chunkPosition) >= renderDistance)
                {
                    Destroy(chunk.Value);
                    _loadedChunks.Remove(chunk.Key);
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
            
            _loadedChunks.Add(position,meshObj);
        }
    }
}
