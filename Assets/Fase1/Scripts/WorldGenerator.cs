using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fase1.MeshComponents;
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

            //generate ranomd seed data
            _xOffset = rnd.Next(-10000,10000);
            _yOffset = rnd.Next(-10000,10000);
            
            //initialize noise and meshcomponents for the world mesh
            _noiseGenerator = new NoiseGenerator(scale, _xOffset, _yOffset, verticesPerChunk, heightMultiplier, heightOffset);
            
            FloorComponent floorComponent = new FloorComponent(_noiseGenerator);
            MeshBuilder.AddMeshComponent(floorComponent);
            
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            
            //if there are any meshbuilders in the queue, build them
            if(_meshBuilders.Count > 0)
            {
                MeshBuilder meshBuilder = _meshBuilders.Peek();
                if (meshBuilder != null)
                {
                    if(meshBuilder.State == MeshState.Generated) BuildMesh(_meshBuilders.Dequeue());
                    else _meshBuilders.Enqueue(meshBuilder);
                }
            }
            
            //remove dead threads
            _threads.RemoveAll(thread => !thread.IsAlive);

            //if there are any unitialized chunks, start a new thread to generate them
            while(_threads.Count < multithreading && _unInitialized.Count > 0)
            {
                var position = _unInitialized.Dequeue();
                Task.Run(() => GenerateChunkThread(position));
            }
            
            UpdateRenderList();
        }
        
        //generate a chunk in a new thread
        void GenerateChunkThread(Vector2Int position)
        {
            ThreadStart threadStart = () =>
            {
                GenerateChunk(position);
            };

            Thread thread = new Thread(threadStart);
            thread.Start();
            
            _threads.Add(thread);
        }

        //update the chunks that need to be rendered and unrendered
        private void UpdateRenderList()
        {
            //check for reference object position
            Vector3 position = mainObject.transform.position;
            
            //get the chunk position of the reference object
            Vector2Int chunkPosition = new Vector2Int((int)position.x / physicalSize,(int)position.z / physicalSize);
            
            //check for chunks that need to be rendered
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

            //check for chunks that need to be unrendered
            List<Vector2Int> remove = new();
            foreach (var chunk in _requestedChunks)
            {
                if (Vector2.Distance(chunk,chunkPosition) > (renderDistance + 1))
                {
                    remove.Add(chunk);
                }
            }

            //remove the chunks that need to be unrendered
            _requestedChunks.RemoveAll(item => remove.Contains(item));

            //copy the chunks dictionary to avoid concurrent modification
            Dictionary<Vector2Int, GameObject> copy = new(_chunks);
            
            
            //destroy the chunks that need to be unrendered
            //preformance is better if we destroy over slower time instead of all at once
            foreach (var chunk in copy)
            {
                if (!_requestedChunks.Contains(chunk.Key))
                {
                    Destroy(chunk.Value);
                    
                    //remove the chunk from the origional dictionary
                    _chunks.Remove(chunk.Key);
                }
            }
            
        }
        
        private void GenerateChunk(Vector2Int objectPosition)
        {
            MeshBuilder meshBuilder = new MeshBuilder(verticesPerChunk, physicalSize, objectPosition);
            
            _meshBuilders.Enqueue(meshBuilder);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        
        //build the mesh and instantiate the gameobject
        //cus we are using threads, we need to use the main thread to instantiate the gameobject and create meshes
        private void BuildMesh(MeshBuilder meshBuilder)
        {
            Vector2Int position = meshBuilder.GetChunkPosition();
            
            Mesh mesh = meshBuilder.BuildMesh();
            
            GameObject meshObj = Instantiate(chunkPrefab, new Vector3(position.x * physicalSize - (position.x * (physicalSize / 100)) ,0,position.y * physicalSize - (position.y * (physicalSize / 100))), new Quaternion(0,0,0,0));
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            meshObj.name = "Chunk (" + position.x + "," + position.y + ")";
            
            _chunks.Add(position,meshObj);
        }
    }
}
