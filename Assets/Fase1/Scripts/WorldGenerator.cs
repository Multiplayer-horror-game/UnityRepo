using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fase1.MeshComponents;
using UnityEditor.Rendering;
using Fase1.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fase1

{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Seed")]
        public string textBasedSeed;

        public int seed;
         
        [Header("WorldSettings")]
        [Range(0.1f,500f)]
        public float heightMultiplier = 20f;

        [Range(-500f,500f)]
        public float heightOffset = -10f;

        [Range(0.001f, 100f)] 
        public float scale = 0.1f;
        
        [Range(1,10000f)]
        public int physicalSize = 100;

        [Range(6, 100)]
        public int verticesPerChunk;
        
        [Header("PerformanceSettings")]
        [Range(1,20)]
        public int renderDistance = 8;

        [Range(1,10)]
        public int multithreading = 1;

        [Header("Prefabs")]
        public GameObject chunkPrefab;

        public GameObject mainObject;

        [Header("ScriptableObject")] 
        public NatureObjects natureObjects;
        
        
        private int _xOffset;
        private int _yOffset;

        private int _destroyC;
         
        private NoiseGenerator _noiseGenerator;
        
        private List<Thread> _threads = new();
        
        private Queue<KeyValuePair<Vector2Int, MeshBuilder>> _meshBuilders = new();

        private Dictionary<Vector2Int,GameObject> _chunks = new();

        private List<Vector2Int> _requestedChunks = new();

        private Queue<Vector2Int> _unInitialized = new();

        private Queue<GameObject> _destroyList = new();
        
        private int _failedCount;
        
        private RoadComponent _roadComponent;
        
        
        void Start()
        {

            seed = textBasedSeed.GetHashCode();
            
            Random.InitState(seed);
            
            //generate random seed data
            _xOffset = Random.Range(-100000,100000);
            _yOffset = Random.Range(-100000,100000);
            
            //initialize noise and meshcomponents for the world mesh
            _noiseGenerator = new NoiseGenerator(scale, _xOffset, _yOffset, verticesPerChunk, heightMultiplier, heightOffset);
            
            //FloorComponent floorComponent = new FloorComponent(_noiseGenerator);
            //MeshBuilder.AddMeshComponent(floorComponent);
            
            _roadComponent = new RoadComponent(mainObject, _noiseGenerator);
            MeshBuilder.AddMeshComponent(_roadComponent);
            
            OperatableVector2 chunkCorner0 = new Vector2(0,0);
            OperatableVector2 chunkCorner1 = new Vector2(physicalSize,physicalSize);
            
            OperatableVector2 random = new Vector2(2000,1000);
            
            if(chunkCorner0 <= random && chunkCorner1 >= random)
            {
                Debug.Log("true");
            }

            //NatureComponent natureComponent = new NatureComponent(_noiseGenerator,natureObjects);
            //MeshBuilder.AddChildren(natureComponent);

        }
        
        void OnDrawGizmos()
        {
            if(_roadComponent == null) return;
            
            Gizmos.color = Color.green;
            new List<Vector2>(_roadComponent.RenderedNodes.Keys).ForEach(point => Gizmos.DrawSphere(new Vector3(point.x,0,point.y), 0.1f));
            
            
            Gizmos.color = Color.red;
            new List<Vector2>(_roadComponent.Nodes.Keys).ForEach(point => Gizmos.DrawSphere(new Vector3(point.x,0,point.y), 0.5f));
            
            
        }
        
        // Update is called once per frame
        void FixedUpdate()
        {
            //if there are any meshbuilders in the queue, build them
            if(_meshBuilders.Count != 0)
            {
                
                KeyValuePair<Vector2Int, MeshBuilder> meshBuilderKvp = _meshBuilders.Peek();
                
                if (meshBuilderKvp.Value != null)
                {
                    if (meshBuilderKvp.Value.State == MeshState.Generated)
                    {
                        BuildMesh(_meshBuilders.Dequeue().Value);
                        _failedCount = 0;
                    }
                }
                else
                {
                    if (_failedCount > 20)
                    {
                        //_meshBuilders.Dequeue();
                        
                        //MeshBuilder regeneratedBuilder = new MeshBuilder(verticesPerChunk, physicalSize, meshBuilderKvp.Key);
                        
                        //BuildMesh(regeneratedBuilder);
                        
                        //Debug.Log("Failed to generate chunk at: " + meshBuilderKvp.Key);
                        _failedCount = 0;
                    }
                    _failedCount++;
                }
            }
            
            //remove dead threads
            _threads.RemoveAll(thread => !thread.IsAlive);

            //if there are any unitialized chunks, start a new thread to generate them
            ThreadStart threadStart = () =>
            {
                while (_threads.Count < multithreading && _unInitialized.Count > 0)
                {
                    var position = _unInitialized.Dequeue();
                    GenerateChunkThread(position);
                }
            };
            
            Thread thread = new Thread(threadStart);
            thread.Start();
            
            UpdateRenderList();
            
            DestroyNextChunk();
            
            // Debug.Log("chunks:"  +_chunks.Count + " requested:" + _requestedChunks.Count + " uninit:" + _unInitialized.Count + " meshbuilders:" + _meshBuilders.Count + " threads:" + _threads.Count + " destroylist:" + _destroyList.Count);
            
        }
        
        private void DestroyNextChunk()
        {
            if (_destroyC == 3)
            {
                if (_destroyList.Count != 0)
                {
                    Destroy(_destroyList.Dequeue());
                }

                _destroyC = 0;
            }

            _destroyC++;
        }
        
        //generate a chunk in a new thread
        void GenerateChunkThread(Vector2Int position)
        {
            MeshBuilder meshBuilder = new MeshBuilder(verticesPerChunk, physicalSize, position);
            
            _meshBuilders.Enqueue(new KeyValuePair<Vector2Int, MeshBuilder>(meshBuilder.GetChunkPosition(),meshBuilder));
            
            ThreadStart threadStart = () =>
            {
                meshBuilder.Start();
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
                    _destroyList.Enqueue(chunk.Value);
                    
                    //remove the chunk from the origional dictionary
                    _chunks.Remove(chunk.Key);
                }
            }
            
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void GenerateChunk(Vector2Int objectPosition)
        {
            MeshBuilder meshBuilder = new MeshBuilder(verticesPerChunk, physicalSize, objectPosition);
            
            _meshBuilders.Enqueue(new KeyValuePair<Vector2Int, MeshBuilder>(objectPosition,meshBuilder));
        }
        
        
        //build the mesh and instantiate the gameobject
        //cus we are using threads, we need to use the main thread to instantiate the gameobject and create meshes
        private void BuildMesh(MeshBuilder meshBuilder)
        {
            Vector2Int position = meshBuilder.GetChunkPosition();
            
            Mesh mesh = meshBuilder.BuildMesh();
            
            GameObject meshObj = Instantiate(chunkPrefab, new Vector3(position.x * physicalSize ,0,position.y * physicalSize), new Quaternion(0,0,0,0));
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            meshObj.name = "Chunk (" + position.x + "," + position.y + ")";
            
            //last meshbuilder operation to add children to the gameobject (trees,stones,etc)
            meshBuilder.ChildOperation(this,meshObj);
            
            _chunks.Add(position,meshObj);
        }
        

        public void ForceInstantiate(GameObject parent, GameObject child, Vector3 position, Quaternion rotation)
        {
            Instantiate(child, position, rotation, parent.transform).transform.localPosition = position;
        }
    }
}
