using System.Collections;
using System.Collections.Generic;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] SceneReference lobbyScene;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(lobbyScene.Name, LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
