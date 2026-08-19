using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer Video;
    public RawImage rawImage;
    // Start is called before the first frame update
    void Start()
    {
        Video = GetComponent<VideoPlayer>();

        Video.loopPointReached += OnVideoFinished;
        rawImage = FindAnyObjectByType<RawImage>();
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        Debug.Log("≤•∑≈ÕÍ≥…");

        StartCoroutine(delayShow());
    }

    IEnumerator delayShow()
    {
        yield return new WaitForSeconds(1);
        UIPanelManager.Instance.ShownPanel("UIPanel/main_Panal");
        rawImage.gameObject.SetActive(false);

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
