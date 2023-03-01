using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Linq;

public class VideoLayerManager : MonoBehaviour
{
    List<VideoLayer> layerList;
    public Experiment exp { get { return Experiment.Instance; } }

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
    private int activeLayer; /*1 corresponding to sunny; 2 for rainy; 3 for night*/
    public ItemLayer itemLayer;

    public float globalFrameUpdateSpeed = 1f;

    private string baseURL = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/";


    public enum Direction
    {
        Forward,
        Backward
    };

    public Direction currentPlaybackDirection = Direction.Forward;

    public List<Texture2D> newTextures = new List<Texture2D>();

    List<int> validStartFrames = new List<int>();


    void Awake()
    {

        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;

        //backgroundLayer = rainyLayer;
        backgroundLayer = null;
        activeLayer = 0;

        newTextures = new List<Texture2D>();
        layerList = new List<VideoLayer>();
         validStartFrames = new List<int>();
    }

    public IEnumerator ResumePlayback()
    {
        UnityEngine.Debug.Log("RESUMING playback");
        yield return StartCoroutine("TogglePauseLayerPlayback", false);

           yield return null;

    }



    IEnumerator GetTextureRequest(string url)
    {
        UnityEngine.Debug.Log("attempting to retrieve from URL " + url);

        using (var www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                if (www.isDone)
                {
                    var texture = DownloadHandlerTexture.GetContent(www);
                    UnityEngine.Debug.Log("retrieved texture " + texture.name);
                    newTextures.Add(texture); 
                }
            }
        }
        yield return null;
    }

    public int GetTotalFramesOfCurrentClip()
    {
        return 1131;
    }

    public void SpawnPointReached()
    {
        //inform the experiment to pause and show the object
        StartCoroutine("RunSpawnProcedure");
        
    }

    public void nextSTIMPointReached()
    {
        StartCoroutine("RunNextSTIMProcedure");
    }
    public int GetMainLayerCurrentFrameNumber()
    {
        return backgroundLayer.GetCurrentFrameNumber();
    }

    public void RetrievalPointReached()
    {
        StartCoroutine("RunRetrievalProcedure");
    }

    IEnumerator RunNextSTIMProcedure()
    {
        Experiment.Instance.elememInterface.SendStimFreq();
        Experiment.nextSendSTIMFrame = -10000;
        yield return null;
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
        Experiment.Instance.uiController.markerCirclePanel.alpha = 1f;

        Transform currTransform = Experiment.Instance.GetTransformForFrame(GetMainLayerCurrentFrameNumber()); //this gets us the position of the player corresponding to the frame

        currTransform.position += currTransform.forward * 2.5f; // the object is located 2.5 units away from the player's current spot

        GameObject stimObject = Instantiate(Experiment.Instance.objController.placeholder, currTransform.position, currTransform.rotation);
        UnityEngine.Debug.Log("SPAWNED AT " + currTransform.position.ToString());
        stimObject.GetComponent<StimulusObject>().stimuliDisplayName = stimDisplayText;
        stimObject.GetComponent<StimulusObject>().stimuliDisplayTexture = stimImage;
        stimObject.gameObject.name = stimDisplayText;
        Experiment.Instance.spawnedObjects.Add(stimObject); //add it to the list

        UnityEngine.Debug.Log("adding spawn object");
        UnityEngine.Debug.Log("new spawnedobjects count " + Experiment.Instance.spawnedObjects.Count.ToString());

        UnityEngine.Debug.Log("FRAME SPAWN " + GetMainLayerCurrentFrameNumber().ToString());
        Experiment.Instance.trialLogTrack.LogItemEncodingEvent(stimDisplayText, GetMainLayerCurrentFrameNumber(),Experiment.Instance.encodingIndex);
        Experiment.Instance.trialLogTrack.LogItemPresentation(stimDisplayText, Experiment.Instance.shuffledStimuliIndices[Experiment.Instance.objController.RandIndex], true);

        Experiment.Instance.stimuliBlockSequence.Add(stimObject); //add to the stim block sequence for end of the block tests

        for(int i=0;i<Experiment.Instance.stimuliBlockSequence.Count;i++)
        {
            //UnityEngine.Debug.Log("inside stim block sequence " + i.ToString() + " : " + Experiment.Instance.stimuliBlockSequence[i].ToString());
        }


        UnityEngine.Debug.Log("NEXT FRAME SPAWN: " + Experiment.nextSpawnFrame);

        Experiment.Instance.retrievalFrameObjectDict.Add(Experiment.nextSpawnFrame, stimObject);

        float waitTime = Configuration.itemPresentationTime + Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);

        yield return new WaitForSeconds(waitTime);
        Experiment.Instance.uiController.markerCirclePanel.alpha = 0f;
        Experiment.Instance.uiController.stimDisplayPanel.alpha = 0f;
        yield return StartCoroutine(TogglePauseLayerPlayback(false));


        Experiment.Instance.trialLogTrack.LogItemPresentation(stimDisplayText, Experiment.Instance.shuffledStimuliIndices[Experiment.Instance.objController.RandIndex], false);
        UnityEngine.Debug.Log("SpawnProcedure: ShuffledStimuliLength: " + Experiment.Instance.shuffledStimuliIndices.Count);
        for (int i = 0; i < Experiment.Instance.shuffledStimuliIndices.Count; i++) {
            UnityEngine.Debug.Log("SpawnProcedure: integers: " + Experiment.Instance.shuffledStimuliIndices[i]);
        }

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
        bool isLure = Experiment.Instance.isLure;
        if(Experiment.Instance.retrievalFrameObjectDict.TryGetValue(currFrame, out retObject))
        {
            UnityEngine.Debug.Log("RetObject RetObject RetObject: " + retObject);
            if(retObject!=null)
            {
                UnityEngine.Debug.Log("Not a NULL. YESSSS");
                yield return StartCoroutine(Experiment.Instance.ShowLocationCuedReactivation(retObject));
                     
            }
        }

        UnityEngine.Debug.Log("Not a NULL. YESSSS. Did we come out");
        //reset the event invoked flag
        VideoLayer.isInvoked = false;
        yield return StartCoroutine(Experiment.Instance.UpdateNextSpawnFrame());
        yield return null;
    }

    public IEnumerator BeginFramePlay()
    {
        //UnityEngine.Debug.Log("efoi3jfo34fo34ro3fopmcmpo3popon4con3xo3");
        StartCoroutine(sunnyLayer.FramePlay());
        //UnityEngine.Debug.Log("efoi3jfo34fo34ro3fopmcmpo3popon4con3xo3   12334");
        StartCoroutine(rainyLayer.FramePlay());
        //UnityEngine.Debug.Log("efoi3jfo34fo34ro3fopmcmpo3popon4con3xo3   12334  1213213");
        StartCoroutine(nightLayer.FramePlay());
        //UnityEngine.Debug.Log("efoi3jfo34fo34ro3fopmcmpo3popon4con3xo3   12334  1213213  123123");
        yield return null;
    }

    IEnumerator DownloadImages(string weather)
    {
        string layerName = "";
        VideoLayer targetLayer = null;
        switch(weather)
        {
            case "Sunny":
                layerName = "sunny";
                targetLayer = sunnyLayer;
                break;
            case "Rainy":
                layerName = "rain";
                targetLayer = rainyLayer;
                break;
            case "Night":
                layerName = "night";
                targetLayer = nightLayer;
                break;
        }

        yield return StartCoroutine(Experiment.Instance.assetBundleLoader.LoadTexturesFromBundle(layerName));

        yield return StartCoroutine(targetLayer.FillImages(newTextures));
        //clear the texture list
        newTextures.Clear();
        yield return null;
    }

    IEnumerator SetupItemLayer()
    {
        yield return StartCoroutine(Experiment.Instance.assetBundleLoader.LoadItemLayer("item"));
        yield return null;
    }

    public IEnumerator SetupLayers()
    {
        //load images in all layers

        yield return StartCoroutine(DownloadImages("Sunny"));
        Experiment.Instance.uiController.UpdateLoadingProgress(40f);
        yield return StartCoroutine(DownloadImages("Rainy"));
        Experiment.Instance.uiController.UpdateLoadingProgress(60f);
        yield return StartCoroutine(DownloadImages("Night"));
        Experiment.Instance.uiController.UpdateLoadingProgress(80f);

        //TODO: Control this from elsewhere
        yield return StartCoroutine(AddToActiveVideoLayer(sunnyLayer, false));
        yield return StartCoroutine(AddToActiveVideoLayer(rainyLayer, false));
        yield return StartCoroutine(AddToActiveVideoLayer(nightLayer, false));
        backgroundLayer = rainyLayer; //set background to rain by default, for now
        activeLayer = 2;

        yield return StartCoroutine(SetupItemLayer());
        yield return null;
    }



    public void UpdateWeather(Weather.WeatherType targetWeather)
    {
        //make current layer invisible

        //UnityEngine.Debug.Log("updating weather to " + targetWeather.ToString());

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
                Experiment.Instance.trialLogTrack.LogWeather(Weather.WeatherType.Sunny);
                sunnyLayer.TogglePause(false);
                backgroundLayer = sunnyLayer;
                activeLayer = 1;
                break;
            case Weather.WeatherType.Rainy:
                //rainyLayer.ToggleLayerVisibility(true);
                rainyLayer.gameObject.SetActive(true);
                Experiment.Instance.trialLogTrack.LogWeather(Weather.WeatherType.Rainy);
                rainyLayer.TogglePause(false);
                backgroundLayer = rainyLayer;
                activeLayer = 2; 
                break;
            case Weather.WeatherType.Night:
                //nightLayer.ToggleLayerVisibility(true);

                Experiment.Instance.trialLogTrack.LogWeather(Weather.WeatherType.Night);
                nightLayer.gameObject.SetActive(true);
                nightLayer.TogglePause(false);
                backgroundLayer = nightLayer;
                activeLayer = 3;
                break;

            default:
                //sunnyLayer.ToggleLayerVisibility(true);
                sunnyLayer.gameObject.SetActive(true);
                Experiment.Instance.trialLogTrack.LogWeather(Weather.WeatherType.Sunny);
                sunnyLayer.TogglePause(false);
                backgroundLayer = sunnyLayer;
                activeLayer = 1;
                break;

     
        }

        itemLayer.UpdateImage(targetWeather);
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

        //check to see if layer should be visible for playback
        layer.ToggleLayerVisibility(isVisible);

        yield return null;
    }

    

    // Update is called once per frame
    void Update()
    {

        if (isManual && !(exp.CanSelectUI()))
        {
            if ((exp.beginScreenSelect == 0) ||
                ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    StartCoroutine(Experiment.Instance.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
                }

                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    StartCoroutine(Experiment.Instance.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Reverse));
                }
            }
        }

        if (backgroundLayer != null)
        {
            if (backgroundLayer.isPaused == false)
            {
                Experiment.Instance.trialLogTrack.LogPosition(activeLayer, GetMainLayerCurrentFrameNumber());
            }
        }

    }


    public void ChangePlaybackDirection(Direction newDirection)
    {
        UnityEngine.Debug.Log("changing direction to " + newDirection.ToString());
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
        UnityEngine.Debug.Log("changing drive mode to " + newDriveMode.ToString());
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

    List<int> ReturnValidStartFrames()
    {
        validStartFrames = new List<int>();
        
        for (int i = 50; i < layerList[0].numberOfFrames-100; i++)
        {
            validStartFrames.Add(i);
        }
        List<int> keys = Experiment.Instance.retrievalFrameObjectDict.Keys.ToList();
        for (int i=0;i<keys.Count;i++)
        {
            for (int j = keys[i] - 24; j < keys[i] + 24; j++)
            {
                int indexToRemove = j - 50;
                if (indexToRemove > 0 && indexToRemove < validStartFrames.Count)
                {
                    validStartFrames.RemoveAt(indexToRemove);
                }
            }
        }

        return validStartFrames;
    }

    public IEnumerator MoveToRandomPoint()
    {
        ReturnValidStartFrames();
        while(validStartFrames.Count<=0)
        {
            yield return 0;
        }
        int randStartFrame = validStartFrames[Random.Range(0,validStartFrames.Count)];
        UnityEngine.Debug.Log("scrolling to random frame: " + randStartFrame.ToString());
        for (int i = 0; i < layerList.Count; i++)
        {
            yield return StartCoroutine(layerList[i].ScrollToFrame(randStartFrame));

            yield return StartCoroutine(layerList[i].TogglePause(false));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }

        //log the frame in the logfile
        Experiment.Instance.trialLogTrack.LogRetrievalStartPosition(Experiment.Instance.GetTransformForFrame(randStartFrame).position);
        yield return null;
    }


    //moves us to the starting/first frame of the current weather
    public IEnumerator ReturnToStart()
    {
        for(int i=0;i<layerList.Count;i++)
        {
            //UnityEngine.Debug.Log("scrolling to start frame");
            yield return StartCoroutine(layerList[i].ScrollToFrame(1));
            yield return StartCoroutine(layerList[i].TogglePause(false));
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }
        yield return null;
    }

    public IEnumerator PauseAllLayers()
    {
        for(int i=0;i<layerList.Count;i++)
        {
            //UnityEngine.Debug.Log("pausing layers");
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }
        yield return null;
    }


    public int GetFrameRangeStart()
    {
        return Configuration.startBuffer;
    }

    public int GetFrameRangeEnd()
    {
        return GetTotalFramesOfCurrentClip() - Configuration.endBuffer;
    }


    /// <summary>
    /// DEBUG ONLY FUNCTIONS
    /// </summary>
    /// <returns></returns>
    public IEnumerator Debug_MoveToFrame(int targetFrame)
    {
        for (int i = 0; i < layerList.Count; i++)
        {
            yield return StartCoroutine(layerList[i].ScrollToFrame(targetFrame));

            yield return StartCoroutine(layerList[i].TogglePause(false));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(layerList[i].TogglePause(true));
        }

        yield return null;
    }

}
