using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoLayerManager : MonoBehaviour
{
    List<VideoLayer> layerList;


    //EXPERIMENT IS A SINGLETON
    private static VideoLayerManager _instance;
    public static VideoLayerManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public VideoLayer backgroundLayer;
    public VideoLayer itemLayer;


    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;

       
        layerList = new List<VideoLayer>();
        
        StartCoroutine("SetupLayers");
        StartCoroutine("RunSpawnProcedure");

        //StartCoroutine("PrepareForMatchProcedure");
    }

    IEnumerator SetupLayers()
    {
        yield return StartCoroutine(AddToActiveVideoLayer(backgroundLayer, true));
        yield return StartCoroutine(AddToActiveVideoLayer(itemLayer, false)); //item layer is not visible by default
        yield return null;
    }

    IEnumerator PrepareForMatchProcedure()
    {

        yield return StartCoroutine(AddToActiveVideoLayer(backgroundLayer, false));
        yield return StartCoroutine(AddToActiveVideoLayer(itemLayer, false));
        yield return null;
    }

    IEnumerator MatchProcedure()
    {


        float randPlaybackTime = Random.Range(0f, (float)backgroundLayer.vidClip.length);
        UnityEngine.Debug.Log("moving to playback time " + randPlaybackTime.ToString());
        yield return StartCoroutine(backgroundLayer.ScrollToPlaybackTime(randPlaybackTime));
        yield return StartCoroutine(itemLayer.ScrollToPlaybackTime(randPlaybackTime));
        yield return null;
    }

    IEnumerator RunSpawnProcedure()
    {
        //backgroundLayer.ToggleLayerVisibility(true);
        //yield return AddToActiveVideoLayer(backgroundLayer,true);
        yield return null;
    }


    IEnumerator SpawnItem()
    {
        //pause all other active layers; mostly the background
        //yield return AddToActiveVideoLayer(itemLayer,true);

        itemLayer.ToggleLayerVisibility(true);

        yield return StartCoroutine("TogglePauseLayerPlayback", true);
        double playbackTime = backgroundLayer.GetPlaybackTime();
        int frame = (int)backgroundLayer.videoPlayer.frame;
        yield return StartCoroutine(itemLayer.ScrollToFrame(frame));
        yield return StartCoroutine(backgroundLayer.ScrollToFrame(frame));
      //  yield return StartCoroutine(itemLayer.ScrollToPlaybackTime(playbackTime));
       // yield return StartCoroutine(backgroundLayer.ScrollToPlaybackTime(playbackTime));
        UnityEngine.Debug.Log("moving to playback time " + playbackTime.ToString());
        yield return new WaitForSeconds(1.5f); //presentation/animation time
        yield return StartCoroutine(RemoveVideoLayer(itemLayer));

        yield return StartCoroutine("TogglePauseLayerPlayback", false);
        yield return null;
    }



    IEnumerator RemoveVideoLayer(VideoLayer layer)
    {
        layer.ToggleLayerVisibility(false);
        //layerList.Remove(layer);
        //yield return StartCoroutine(layer.AbortPlayback());
        yield return null;
    }


     public IEnumerator AddToActiveVideoLayer(VideoLayer layer,bool isVisible)
    {
        layerList.Add(layer);

        UnityEngine.Debug.Log("adding to list " + layerList.Count.ToString());
        //if (!layer.videoPlayer.isPrepared)
        //{
        //    UnityEngine.Debug.Log(layer.gameObject.name + " not prepared; preparing textures");
            yield return StartCoroutine(layer.PrepareVideoTexture());
        //}

        //check to see if we should immediately begin playback of the layer
        if (isVisible)
            layer.ToggleLayerVisibility(true);

        yield return StartCoroutine(layer.BeginPlayback());

        yield return null;
    }

    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine("MatchProcedure");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine("SpawnItem");
        }
            if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine("TogglePauseLayerPlayback",true);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine("TogglePauseLayerPlayback",false);
        }
    }

    IEnumerator TogglePauseLayerPlayback(bool isPaused)
    {
        UnityEngine.Debug.Log("toggling pause " + isPaused.ToString() + " for " + layerList.Count.ToString());
        for (int i = 0; i < layerList.Count; i++)
        {
                yield return StartCoroutine(layerList[i].TogglePlayback(isPaused));
        }
        yield return null;
    }

}
