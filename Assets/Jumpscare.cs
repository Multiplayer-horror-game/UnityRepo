using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Jumpscare : MonoBehaviour
{
    public AudioSource Scream;
    public GameObject jumpscareObject;
    public GameObject Player;
    public GameObject FlashImg;
    
    
    // Start is called before the first frame update
    void Start()
    {
        jumpscareObject.SetActive(false);
        FlashImg.SetActive(false);
    }

     void OnTriggerEnter(Collider player)
     {
        if(player.tag == "Player")
        {
            Scream.Play();
            jumpscareObject.SetActive(true);
            FlashImg.SetActive(true);
            StartCoroutine(DisableObject());
        }
     }
    IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(2.5f);
        jumpscareObject.SetActive(false);
        gameObject.SetActive(false);
        FlashImg.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
}
