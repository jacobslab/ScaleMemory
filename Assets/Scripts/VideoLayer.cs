﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoLayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip vidClip;


    private bool isPaused = true;
    private float directionMultiplier = 1f;

    private int updateVal = 0;

    private float target = 0f; 




    //colors
    //private Color visibleColor = new Color(1, 1, 1, 1);
    //private Color hiddenColor = new Color(0,0,0,0);


    public int numberOfFrames = 1130;
    public float frameRate = 30;

    private int playbackDirection = 1;
    public RawImage bgLayer;

    private float timeVar = 0f;

    public static float speed = 1f;

    private Texture2D[] frames;

    public string layerName = "";

    private UnityEvent spawnPointReachedEvent;
    private UnityEvent retrievalPointReachedEvent;

    public VideoLayerManager videoLayerManager;

    public static bool isInvoked = false;

    private int currentFrame = 0;

  
    private void Awake()
    {
    }

    public int GetCurrentFrameNumber()
    {
        return currentFrame;
    }


    void Start()
    {
        // load all frames of this layer
        frames = new Texture2D[numberOfFrames];
        for (int i = 0; i < numberOfFrames; ++i)
        {
            frames[i] = (Texture2D)Resources.Load(layerName + "/" + string.Format(layerName + "-{0:d3}", i + 1));
        }

        bgLayer.texture = frames[0];

        UnityEngine.Debug.Log("loaded " + frames.Length.ToString() + " frames for " + gameObject.name);
        if (spawnPointReachedEvent == null)
            spawnPointReachedEvent = new UnityEvent();

        spawnPointReachedEvent.AddListener(OnSpawnPointReached);

        if (retrievalPointReachedEvent == null)
                retrievalPointReachedEvent = new UnityEvent();

        retrievalPointReachedEvent.AddListener(OnRetrievalPointReached);
        StartCoroutine("RandomizeFrameSpeed");

    }

    IEnumerator RandomizeFrameSpeed()
    {
        while (Application.isPlaying)
        {
            speed = Random.Range(Configuration.minFrameSpeed, Configuration.maxFrameSpeed);
            yield return new WaitForSeconds(5f + Random.Range(1f, 8f));
            yield return 0;
        }
        yield return null;
    }

    //private void OnEnable()
    //{

    //    videoPlayer.loopPointReached += OnVideoLoop;
    //}

    //private void OnDisable()
    //{
    //    videoPlayer.loopPointReached -= OnVideoLoop;
    //}
    void OnVideoLoop()
    {
        UnityEngine.Debug.Log("looping");
        LapCounter.OnCompleteVideoLoop();
    }

    void OnVideoReverseLoop()
    {
        UnityEngine.Debug.Log("on reverse loop");
    }

    public void OnSpawnPointReached()
    {
        videoLayerManager.SpawnPointReached();
    }

    public void OnRetrievalPointReached()
    {
        videoLayerManager.RetrievalPointReached();
    }



    void Update()
    {
        if (!isPaused)
        {
            if (playbackDirection == 1)
                timeVar += Time.deltaTime * speed;
            else
                timeVar -= Time.deltaTime * speed;

            currentFrame = (int)(timeVar * frameRate);
            //UnityEngine.Debug.Log("current frame: " + currentFrame.ToString());
            if (Mathf.Abs(currentFrame - Experiment.nextSpawnFrame) < 2)
            {
                if (!isInvoked)
                {
                    UnityEngine.Debug.Log("invoking spawn point event");
                    isInvoked = true;
                    if (Experiment.Instance.currentStage == Experiment.TaskStage.Encoding)
                        spawnPointReachedEvent.Invoke();
                    else
                        retrievalPointReachedEvent.Invoke();
                }
            }
            if (currentFrame >= frames.Length - 1)
            {
                //UnityEngine.Debug.Log("exceeded video");
                timeVar = 0f;
                OnVideoLoop();
                currentFrame = 0;
            }
            if (currentFrame < 0)
            {
                //UnityEngine.Debug.Log("before start");
                currentFrame = frames.Length - 3;
                OnVideoReverseLoop();
                timeVar = currentFrame / frameRate;
            }
            //UnityEngine.Debug.Log("current frame " + currentFrame.ToString());
            //UnityEngine.Debug.Log("timevar " + timeVar.ToString());
            //UnityEngine.Debug.Log("associating frame" + frames[currentFrame].name.ToString());
            bgLayer.texture = frames[currentFrame];
        }

        //if (Input.GetKeyDown(KeyCode.Y))
        //    speed += 0.1f;
        //if (Input.GetKeyDown(KeyCode.H))
        //    speed -= 0.1f;
    }

    //public void Forward(float deltaTime)
    //{
    //    if (!videoPlayer.isPlaying) return;
    //    //videoPlayer.time+=  Time.deltaTime * 2f;
    //    videoPlayer.frame++;
    //    //UnityEngine.Debug.Log("time " + videoPlayer.time.ToString());
    //    UnityEngine.Debug.Log("frame num " + videoPlayer.frame.ToString());
    //}

    //public void Backward(float deltaTime)
    //{
    //    if (!videoPlayer.isPlaying) return;
    //    videoPlayer.time = videoPlayer.time - Time.deltaTime * 1f;

    //    UnityEngine.Debug.Log("time " + videoPlayer.time.ToString());
    //}

    public IEnumerator PrepareVideoTexture()
    {
        if (videoPlayer == null || bgLayer == null)
            yield break;



        yield return null;
    }

   
    //this usually corresponds to the layer being added to the "active stack"
    public IEnumerator BeginPlayback()
    {
        UnityEngine.Debug.Log("beginning playback of " + gameObject.name);
        yield return StartCoroutine(TogglePause(false));

        yield return StartCoroutine(TogglePause(true)); 
        

        //StartCoroutine("RunVideoTest");
        yield return null;
    }

    public void ToggleLayerVisibility(bool isVisible)
    {
        UnityEngine.Debug.Log("making " + gameObject.name + " visibility: " + isVisible.ToString());
    }

    public double GetPlaybackTime()
    {
        return videoPlayer.time;
    }    

    public IEnumerator TogglePause(bool shouldPause)
    {
        UnityEngine.Debug.Log("should pause? " + shouldPause.ToString());
        if (shouldPause)
        {
            UnityEngine.Debug.Log("paused");
            isPaused = true;
        }
        //videoPlayer.Pause();
        else
        {
            UnityEngine.Debug.Log("unpaused");
            isPaused = false;
        }
        //videoPlayer.Play();
        yield return null;
    }



    public IEnumerator ScrollToFrame(long frameNum)
    {
        UnityEngine.Debug.Log("scrolling to frame " + frameNum.ToString());
        int targetFrame = (int)frameNum;
        float targetTime = targetFrame / frameRate;
        timeVar = targetTime; //update time var and the frame will be set correspondigly so when the Update loop runs next time
        currentFrame = (int)frameNum;
        //videoPlayer.frame = frameNum;
        yield return null;
    }

    public IEnumerator ScrollToPlaybackTime(double playbackTime)
    {
        videoPlayer.time = playbackTime;
        yield return null;
    }

    public void ChangePlaybackDirection(VideoLayerManager.Direction newDirection)
    {
        switch(newDirection)
        {
            case VideoLayerManager.Direction.Forward:
                playbackDirection = 1;
                break;
            case VideoLayerManager.Direction.Backward:
                playbackDirection = -1;
                break;
            default:
                playbackDirection = 1;
                break;
        }
    }

    public IEnumerator AbortPlayback()
    {
        //videoPlayer.Stop();
        //rawImage.color = hiddenColor;
        yield return null;
    }
}
