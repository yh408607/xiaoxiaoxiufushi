using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TestScene : MonoBehaviour
{
    public GameObject loadPanel;
    public GameObject selectPanel;
    public GameObject MainPanal;

    // Start is called before the first frame update
    void Start()
    {
        loadPanel.SetActive(false);
        selectPanel.SetActive(false);
        MainPanal.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        MainPanal.SetActive(false);
        loadPanel.SetActive(true);

        StartCoroutine(DelaystarGame());
    }

    IEnumerator DelaystarGame()
    {
        yield return new WaitForSeconds(2);
        loadPanel.SetActive(false);
        selectPanel.SetActive(true) ;
    }
}
