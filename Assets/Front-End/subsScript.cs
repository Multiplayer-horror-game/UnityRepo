using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class NewBehaviourScript : MonoBehaviour
{

    private NewBehaviourScript instance;

    public GameObject textBox;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TheSequence());
    }

    IEnumerator TheSequence() {
        yield return new WaitForSeconds(1);
        textBox.GetComponent<TextMeshProUGUI>().text = "hallo hallo hallo hallo";
        yield return new WaitForSeconds(3);
        textBox.GetComponent<TextMeshProUGUI>().text = "";
        yield return new WaitForSeconds(1);
        textBox.GetComponent<TextMeshProUGUI>().text = "hoi hoi hoi hoi";
        yield return new WaitForSeconds(3);
        textBox.GetComponent<TextMeshProUGUI>().text = "";
        yield return new WaitForSeconds(2);
        textBox.GetComponent<TextMeshProUGUI>().text = "doei doei doei doei";
        yield return new WaitForSeconds(2);
        textBox.GetComponent<TextMeshProUGUI>().text = "";
    }


}
