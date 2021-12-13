﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoLayerManager : MonoBehaviour
{
    List<VideoLayer> layerList;


    //EXPERIMENT IS A SINGLETON
    private static VideoLayerManager _instance;

    private bool isManual = false;
    public static VideoLayerManager Instance
    {
        get
        {
            return _instance;
        }
    }

    public VideoLayer sunnyLayer;
    public VideoLayer rainyLayer;
    public VideoLayer nightLayer;

    private VideoLayer backgroundLayer;
    public VideoLayer itemLayer;

    public float globalFrameUpdateSpeed = 1f;

    

    public enum Direction
    {
        Forward,
        Backward
    };

    public Direction currentPlaybackDirection = Direction.Forward;


    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;

        backgroundLayer = rainyLayer;

       
        layerList = new List<VideoLayer>();
        
        StartCoroutine("SetupLayers");
    }

    public IEnumerator ResumePlayback()
    {
        UnityEngine.Debug.Log("RESUMING playback");
        yield return StartCoroutine("TogglePauseLayerPlayback", false);

           yield return null;

    }

    public int GetTotalFramesOfCurrentClip()
    {
        return 1131;
        //return (int)backgroundLayer.numberOfFrames;
    }

    public void SpawnPointReached()
    {
        //inform the experiment to pause and show the object
        StartCoroutine("RunSpawnProcedure");
        
    }

    public int GetMainLayerCurrentFrameNumber()
    {
        return backgroundLayer.GetCurrentFrameNumber();
    }

    public void RetrievalPointReached()
    {
        StartCoroutine("RunRetrievalProcedure");
    }

    IEnumerator RunSpawnProcedure()
    {
        //inform Experiment about spawn procedure for logging

        //pause all layers first
        yield return StartCoroutine(TogglePauseLayerPlayback(true));
        //itemLayer.ToggleLayerVisibility(true);
        Texture stimImage = Experiment.Instance.objController.ReturnStimuliToPresent();
        string stimDisplayText = Experiment.Instance.objController.ReturnStimuliDisplayText();

        Experiment.Instance.uiController.stimItemImage.texture = stimImage;
        Experiment.Instance.uiController.stimNameText.text = stimDisplayText;
        Experiment.Instance.uiController.stimDisplayPanel.alpha = 1f;

        Transform currTransform = Experiment.Instance.GetTransformForFrame(backgroundLayer.GetCurrentFrameNumber()); //this gets us the position of the player corresponding to the frame

        currTransform.position += currTransform.forward * 2.5f; // the object is located 2.5 units away from the player's current spot

        GameObject stimObject = Instantiate(Experiment.Instance.objController.placeholder, currTransform.position, currTransform.rotation);
        stimObject.GetComponent<StimulusObject>().stimuliDisplayName = stimDisplayText;
        stimObject.GetComponent<StimulusObject>().stimuliDisplayTexture = stimImage;
        stimObject.gameObject.name = stimDisplayText;
        Experiment.Instance.spawnedObjects.Add(stimObject); //add it to the list

        UnityEngine.Debug.Log("adding to stim block sequence");
        Experiment.Instance.stimuliBlockSequence.Add(stimObject); //add to the stim block sequence for end of the block tests
        UnityEngine.Debug.Log("new length of stim block sequence " + Experiment.Instance.stimuliBlockSequence.Count.ToString());

        for(int i=0;i<Experiment.Instance.stimuliBlockSequence.Count;i++)
        {
            UnityEngine.Debug.Log("inside stim block sequence " + i.ToString() + " : " + Experiment.Instance.stimuliBlockSequence[i].ToString());
        }


        Experiment.Instance.retrievalFrameObjectDict.Add(Experiment.nextSpawnFrame, stimObject);

        float waitTime = Configuration.itemPresentationTime + Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);

        yield return new WaitForSeconds(waitTime);

        Experiment.Instance.uiController.stimDisplayPanel.alpha = 0f;
        yield return StartCoroutine(TogglePauseLayerPlayback(false));

        yield return StartCoroutine(Experiment.Instance.UpdateNextSpawnFrame());

        //reset the event invoked flag
        VideoLayer.isInvoked = false;

        //wrap up logging details about the presentation

        yield return null;
    }

    IEnumerator RunRetrievalProcedure()
    {
        GameObject retObject = null;
        int currFrame = Experiment.nextSpawnFrame;
        UnityEngine.Debug.Log("trying to find object for frame " + currFrame.ToString());
        if(Experiment.Instance.retrievalFrameObjectDict.TryGetValue(currFrame, out retObject))
        {
            UnityEngine.Debug.Log("found object");
            if(retObject!=null)
            {
                UnityEngine.Debug.Log("and its not null");
                yield return StartCoroutine(Experiment.Instance.ShowLocationCuedReactivation(retObject));
            }
        }

        UnityEngine.Debug.Log("finishing check");

        //reset the event invoked flag
        VideoLayer.isInvoked = false;
        yield return StartCoroutine(Experiment.Instance.UpdateNextSpawnFrame());
        yield return null;
    }

    IEnumerator SetupLayers()
    {
        //TODO: Control this from elsewhere
        yield return StartCoroutine(AddToActiveVideoLayer(sunnyLayer, false));
        yield return StartCoroutine(AddToActiveVideoLayer(rainyLayer, false));
        yield return StartCoroutine(AddToActiveVideoLayer(nightLayer, false));
        backgroundLayer = rainyLayer; //set background to rain by default, for now

        //yield return StartCoroutine(AddToActiveVideoLayer(backgroundLayer, false));
        //yield return StartCoroutine(AddToActiveVideoLayer(itemLayer, false)); //item layer is not visible by default
        yield return null;
    }

    IEnumerator PrepareForMatchProcedure()
    {

        yield return StartCoroutine(AddToActiveVideoLayer(backgroundLayer, false));
        //yield return StartCoroutine(AddToActiveVideoLayer(itemLayer, false));
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


    public void UpdateWeather(Weather.WeatherType targetWeather)
    {
        //make current layer invisible

        UnityEngine.Debug.Log("updating weather to " + targetWeather.ToString());

        if (backgroundLayer != null)
        {
            VideoLayer tempOldLayer = backgroundLayer;
            //backgroundLayer.ToggleLayerVisibility(false);
            backgroundLayer.gameObject.SetActive(false);
            backgroundLayer.TogglePause(true);
        }
        switch(targetWeather)
        {
            case Weather.WeatherType.Sunny:
                //sunnyLayer.ToggleLayerVisibility(true);
                sunnyLayer.gameObject.SetActive(true);
                sunnyLayer.TogglePause(false);
                backgroundLayer = sunnyLayer;
                break;
            case Weather.WeatherType.Rainy:
                //rainyLayer.ToggleLayerVisibility(true);
                rainyLayer.gameObject.SetActive(true);
                rainyLayer.TogglePause(false);
                backgroundLayer = rainyLayer;
                break;
            case Weather.WeatherType.Night:
                //nightLayer.ToggleLayerVisibility(true);

                nightLayer.gameObject.SetActive(true);
                nightLayer.TogglePause(false);
                backgroundLayer = nightLayer;
                break;

            default:
                //sunnyLayer.ToggleLayerVisibility(true);
                sunnyLayer.gameObject.SetActive(true);
                sunnyLayer.TogglePause(false);
                backgroundLayer = sunnyLayer;
                break;


        }
    }





    IEnumerator SpawnItem()
    {
        //pause all other active layers; mostly the background
        //yield return AddToActiveVideoLayer(itemLayer,true);

        itemLayer.ToggleLayerVisibility(true);

        yield return StartCoroutine("TogglePauseLayerPlayback", true);
        double playbackTime = backgroundLayer.GetPlaybackTime();
        int frame = (int)backgroundLayer.videoPlayer.frame;
        //yield return StartCoroutine(itemLayer.ScrollToFrame(frame));
        yield return StartCoroutine(backgroundLayer.ScrollToFrame(frame));
      //  yield return StartCoroutine(itemLayer.ScrollToPlaybackTime(playbackTime));
       // yield return StartCoroutine(backgroundLayer.ScrollToPlaybackTime(playbackTime));
        UnityEngine.Debug.Log("moving to playback time " + playbackTime.ToString());
        yield return new WaitForSeconds(1.5f); //presentation/animation time
        //yield return StartCoroutine(RemoveVideoLayer(itemLayer));

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
            yield return StartCoroutine(layer.PrepareVideoTexture());


        //check to see if layer should be visible for playback
        layer.ToggleLayerVisibility(isVisible);
        
        //yield return StartCoroutine(layer.BeginPlayback());

        ////pause immediately after

        //yield return StartCoroutine("TogglePauseLayerPlayback", true);

        yield return null;
    }

    

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.M))
        //{
        //    StartCoroutine("MatchProcedure");
        //}
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine("SpawnItem");
        }
        //    if (Input.GetKeyDown(KeyCode.R))
        //{
        //    StartCoroutine("TogglePauseLayerPlayback",true);
        //}
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    StartCoroutine("TogglePauseLayerPlayback",false);
        //}

        if (isManual)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangePlaybackDirection(Direction.Forward);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ChangePlaybackDirection(Direction.Backward);
            }
        }
    }


    public void ChangePlaybackDirection(Direction newDirection)
    {
        for(int i=0;i<layerList.Count;i++)
        {
            layerList[i].ChangePlaybackDirection(newDirection);
        }
    }

    IEnumerator TogglePauseLayerPlayback(bool isPaused)
    {
        //UnityEngine.Debug.Log("toggling pause " + isPaused.ToString() + " for " + layerList.Count.ToString());
        for (int i = 0; i < layerList.Count; i++)
        {
                yield return StartCoroutine(layerList[i].TogglePause(isPaused));
        }
        yield return null;
    }

    public void SetNewPlaybackMode(CarMover.DriveMode newDriveMode)
    {
        switch(newDriveMode)
        {
            case CarMover.DriveMode.Auto:
                isManual = false;
                break;
            case CarMover.DriveMode.Manual:
                isManual = true;
                break;

                //auto by default
            default:
                isManual = false;
                break;

        }
    }

    public IEnumerator MoveToRandomPoint()
    {
        int randStartFrame = Random.Range(50,layerList[0].numberOfFrames-100);
        for (int i = 0; i < layerList.Count; i++)
        {
            UnityEngine.Debug.Log("scrolling to start frame");
            yield return StartCoroutine(layerList[i].ScrollToFrame(randStartFrame));

            yield return StartCoroutine(layerList[i].TogglePause(false));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }
        //log the frame in the logfile
        Experiment.Instance.trialLogTrack.LogRetrievalStartPosition(Experiment.Instance.GetTransformForFrame(randStartFrame).position);
        yield return null;
    }

    public IEnumerator ReturnToStart()
    {
        for(int i=0;i<layerList.Count;i++)
        {
            UnityEngine.Debug.Log("scrolling to start frame");
            yield return StartCoroutine(layerList[i].ScrollToFrame(1));
            yield return StartCoroutine(layerList[i].TogglePause(false));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }
        yield return null;
    }

    public IEnumerator PauseAllLayers()
    {
        for(int i=0;i<layerList.Count;i++)
        {
            UnityEngine.Debug.Log("pausing layers");
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }
        yield return null;
    }
}
