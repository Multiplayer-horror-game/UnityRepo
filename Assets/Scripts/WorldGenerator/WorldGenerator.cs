using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eflatun.SceneReference;
using Fase1.Car;
using Fase1.MeshComponents;
using UnityEditor.Rendering;
using Fase1.ScriptableObjects;
using Fase1.Scripts.Math;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Fase1

{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("scene")] 
        public SceneReference sceneRef;
        private Scene scene;
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

        [Header("NatureSettings")]
        public int natureThreadCount = 0;
        
        public int TreeDistance = 200;
        
        public int TreeCount = 2000;
        
        
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
        
        private RoadComponent _roadComponent;
        
        // The WorldObject Variables
        private HeatSortedQueue<NatureObject> _objectList = new();
        
        private List<Thread> _natureThreads = new();
        
        private List<GameObject> _activeTrees = new();
        private List<GameObject> _inactiveTrees = new();
        private int AllTrees => _inactiveTrees.Count + _activeTrees.Count;
        
        void Start()
        {

            scene = SceneManager.GetSceneByName(sceneRef.Name);
            
            seed = textBasedSeed.GetHashCode();
            
            Random.InitState(seed);
            
            //generate random seed data
            _xOffset = Random.Range(-100000,100000);
            _yOffset = Random.Range(-100000,100000);
            
            //initialize noise and meshcomponents for the world mesh
            _noiseGenerator = new NoiseGenerator(scale, _xOffset, _yOffset, verticesPerChunk, heightMultiplier, heightOffset, physicalSize);
            
            mainObject.GetComponent<CarBehavior>().GiftNoiseGenerator(_noiseGenerator);
            
            FloorComponent floorComponent = new FloorComponent(_noiseGenerator);
            MeshBuilder.AddMeshComponent(floorComponent);
            
            _roadComponent = new RoadComponent(mainObject, _noiseGenerator);
            MeshBuilder.AddMeshComponent(_roadComponent);
            

            NatureComponent natureComponent = new NatureComponent(_noiseGenerator,natureObjects);
            MeshBuilder.AddChildren(natureComponent);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            
            natureThreadCount = _natureThreads.Count;
            
            //if there are any objects in the object list, instantiate them
            if (_objectList.Count != 0)
            {
                GameObject obj = _objectList.GetHighestHeat().Value.Instantiate(this);
                if (obj != null)
                {
                    _objectList.Dequeue();
                } 
            }
            
            //execute threads
            
            for (int i = 0; i < _natureThreads.Count; i++)
            {
                if(_natureThreads.Count != 0)
                {
                    _natureThreads[0].Start();
                    _natureThreads.RemoveAt(0);
                }
            }
            
            //if there are any meshbuilders in the queue, build them
            if(_meshBuilders.Count != 0)
            {
                KeyValuePair<Vector2Int, MeshBuilder> meshBuilderKvp = _meshBuilders.Peek();
                
                if (meshBuilderKvp.Value != null)
                {
                    if (meshBuilderKvp.Value.State == MeshState.Generated)
                    {
                        BuildMesh(_meshBuilders.Dequeue().Value);
                    }
                    
                    if(meshBuilderKvp.Value.State == MeshState.Failed)
                    {
                        KeyValuePair<Vector2Int,MeshBuilder> failedBuilder = _meshBuilders.Dequeue();
                        _unInitialized.Enqueue(failedBuilder.Key);
                    }
                }
            }
            
            
            //remove dead threads
            _threads.RemoveAll(thread => !thread.IsAlive);

            //if there are any unitialized chunks, start a new thread to generate them
            for (int i = 0; i < _unInitialized.Count; i++)
            {
                if (!(_threads.Count < multithreading)) break;
                
                var position = _unInitialized.Dequeue();
                GenerateChunkThread(position);
            }
                
            UpdateRenderList();
            
            DestroyNextChunk();
            
        }
        
        private void DestroyNextChunk()
        {
            if (_destroyC == 3)
            {
                if (_destroyList.Count != 0)
                {
                    RemoveNatureObjectsByParent(_destroyList.Peek());
                    GameObject toDestroy = _destroyList.Dequeue();

                    for (int i = 0; i < toDestroy.transform.childCount; i++)
                    {
                        GameObject child = toDestroy.transform.GetChild(i).gameObject;
                        child.transform.SetParent(null);
                        child.SetActive(false);
                        
                        _activeTrees.Remove(child);
                        _inactiveTrees.Add(child);
                    }
                    
                    Destroy(toDestroy);
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
        
        
        //build the mesh and instantiate the gameobject
        //cus we are using threads, we need to use the main thread to instantiate the gameobject and create meshes
        private void BuildMesh(MeshBuilder meshBuilder)
        {
            Vector2Int position = meshBuilder.GetChunkPosition();
            
            Mesh mesh = meshBuilder.BuildMesh();
            
            GameObject meshObj = Instantiate(chunkPrefab, new Vector3(position.x * physicalSize ,0,position.y * physicalSize), new Quaternion(0,0,0,0));
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            meshObj.GetComponent<MeshCollider>().sharedMesh = mesh;
            meshObj.name = "Chunk (" + position.x + "," + position.y + ")";
            
            SceneManager.MoveGameObjectToScene(meshObj,scene);
            
            //last meshbuilder operation to add children to the gameobject (trees,stones,etc)
            meshBuilder.ChildOperation(this,meshObj);
            
            
            _chunks.Add(position,meshObj);
        }
        

        public GameObject RequestTree(GameObject parent, GameObject child, Vector3 position, Quaternion rotation)
        {
            if(_inactiveTrees.Count == 0 && AllTrees > TreeCount)
            {
                Debug.Log("no Trees to reuse");
                return null;
            }
            
            if(AllTrees > TreeCount || _inactiveTrees.Count != 0)
            {
                Debug.Log("reuse Tree");
                
                _inactiveTrees[0].SetActive(true);
                
                _inactiveTrees[0].transform.SetParent(parent.transform);
                
                Debug.Log(_inactiveTrees[0].transform.position);
                _inactiveTrees[0].transform.localPosition = position;
                _inactiveTrees[0].transform.rotation = rotation;
                
                _activeTrees.Add(_inactiveTrees[0]);
                _inactiveTrees.RemoveAt(0);
                
                return _activeTrees[_activeTrees.Count - 1];
            }
            else
            {
                GameObject obj = Instantiate(child, position, rotation, parent.transform);
                obj.transform.localPosition = position;
                _activeTrees.Add(obj);
                return obj;
            }
            
        }
        
        public void RequestNatureObject(NatureObject natureObject)
        {
            _natureThreads.Add(new Thread(() => CheckNatureObjectHeat(natureObject)));
        }

        private void CheckNatureObjectHeat(NatureObject natureObject)
        {
            List<Vector2> nodes = _roadComponent.RenderedNodes.Keys.ToList();
            Vector2 closestVector = natureObject.CheckDistance(new Vector3(nodes[0].x,0,nodes[0].y));
            
            foreach (var node in nodes)
            {
                Vector2 distanceVector = natureObject.CheckDistance(new Vector3(node.x,0,node.y));
                if (distanceVector.magnitude <= closestVector.magnitude)
                {
                    closestVector = distanceVector;
                }
                else
                {
                    break;
                }
            }
            
            float distance = Mathf.Abs(closestVector.magnitude);
            
            if(distance > TreeDistance) return;

            //then the -distance will be clamped between 0 and 100
            int heat = (int) Mathf.Abs(-distance + TreeDistance) / (TreeDistance / 100);
            
            _objectList.Enqueue(heat, natureObject);
        }
        
        public void RemoveNatureObjectsByParent(GameObject parent)
        {
            _objectList.GetValues().RemoveAll(natureObject => natureObject.Value.GetParent() == parent);
        }
        
        public RoadComponent GetRoadComponent()
        {
            return _roadComponent;
        }
    }
}
