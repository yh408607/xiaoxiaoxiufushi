using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestScene : MonoBehaviour
{
    public GameObject shouyePanel;
    public GameObject selectPanel;
    public GameObject MainPanal;
    public Animator shouyeanimator;
    bool shouye = true;
    // Start is called before the first frame update
    void Start()
    {
        shouyePanel.SetActive(shouye);
        selectPanel.SetActive(false);
        MainPanal.SetActive(false);
       
    }

    // Update is called once per frame
    void Update()
    {
       
        Qingchu();
    }

    public void StartGame()
    {
        MainPanal.SetActive(false);
        shouyePanel.SetActive(shouye);
     
       // StartCoroutine(DelaystarGame());
    }

    IEnumerator DelaystarGame()
    {
        yield return new WaitForSeconds(2);
        shouyePanel.SetActive(false);
        selectPanel.SetActive(true) ;
    }
    void Qingchu()
    { 
       
        if (shouye && Input.GetMouseButtonDown(0))
        {
            Debug.Log(1111);
           // shouye = false;
          //  shouyePanel.SetActive(shouye);
           // MainPanal.SetActive(true);
            shouyeanimator.SetBool("is Mousedown", true);
        }
       
    }

}
