using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpscare : MonoBehaviour
{
    public GameObject jumpscareObject;
    // Start is called before the first frame update
    void Start()
    {
        jumpscareObject.SetActive(false);
    }

     void OnTriggerEnter(Collider player)
     {
        if(player.tag == "Player")
        {
            jumpscareObject.SetActive(true);
            StartCoroutine(DisableObject());
        }
     }
    IEnumerator DisableObject()
    {
        yield return new WaitForSeconds(1.5f);
        jumpscareObject.SetActive(false);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
