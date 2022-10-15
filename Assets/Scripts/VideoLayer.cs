using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoLayer : MonoBehaviour
{

    public Experiment exp { get { return Experiment.Instance; } }
    public bool isPaused = true;
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

    public static float Fixspeed = 1f;
    public static float speed = 1f;

    public Texture2D[] frames;

    public string layerName = "";

    private UnityEvent spawnPointReachedEvent;
    private UnityEvent retrievalPointReachedEvent;

    public VideoLayerManager videoLayerManager;

    public static bool isInvoked = false;

    private int currentFrame = 0;

    public bool boolLoggingCorner;
    public TrialLogTrack trialLogTrack;

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
        /*
#if !UNITY_WEBGL
        frames = new Texture2D[numberOfFrames];
        for (int i = 0; i < numberOfFrames; ++i)
        {
            frames[i] = (Texture2D)Resources.Load(layerName + "/" + string.Format(layerName + "-{0:d3}", i + 1));
        }

        bgLayer.texture = frames[0];
#endif
        */
        //UnityEngine.Debug.Log("loaded " + frames.Length.ToStri ng() + " frames for " + gameObject.name);
        boolLoggingCorner = false;
        if (spawnPointReachedEvent == null)
            spawnPointReachedEvent = new UnityEvent();

        spawnPointReachedEvent.AddListener(OnSpawnPointReached);

        if (retrievalPointReachedEvent == null)
                retrievalPointReachedEvent = new UnityEvent();

        retrievalPointReachedEvent.AddListener(OnRetrievalPointReached);
        StartCoroutine("RandomizeFrameSpeed");


    }

    public IEnumerator FillImages(List<Texture2D> newTextures)
    {
        frames = new Texture2D[numberOfFrames];
        for (int i = 0; i < newTextures.Count; ++i)
        {
            //UnityEngine.Debug.Log("adding " + newTextures[i].name.ToString());
            frames[i] = newTextures[i];
            //frames[i] = (Texture2D)Resources.Load(layerName + "/" + string.Format(layerName + "-{0:d3}", i + 1));
        }

        bgLayer.texture = frames[0];
        yield return null;
    }

    IEnumerator RandomizeFrameSpeed()
    {
        int MChange = -1;
        while (Application.isPlaying)
        {
            if (Experiment.Instance.currentStage == Experiment.TaskStage.SpatialRetrieval || Experiment.Instance.currentStage == Experiment.TaskStage.VerbalRetrieval)
            {
                Fixspeed = Random.Range(Configuration.minRetrievalFrameSpeed, Configuration.maxRetrievalFrameSpeed);
                if (MChange != 0) {
                    MChange = 0;
                    speed = Fixspeed;
                    Experiment.Instance.trialLogTrack.LogDefaultFixSpeed(speed);
                } 
            }
            else
            {
                Fixspeed = Random.Range(Configuration.minFrameSpeed, Configuration.maxFrameSpeed);
                if (MChange != 1)
                {
                    MChange = 1;
                    speed = Fixspeed;
                    Experiment.Instance.trialLogTrack.LogDefaultFixSpeed(speed);
                }
            }

            Debug.Log("RandomizeFramSpeed: Speed Multiple?? " + speed);
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

    public IEnumerator FramePlay()
    {
        speed = Fixspeed;
        Experiment.Instance.trialLogTrack.LogDefaultFixSpeed(speed);

        while (Experiment.Instance.IsExpActive())
        {
            if (gameObject.activeSelf)
            {

                if (!isPaused)
                {
                    KeyCode findkey_inc = KeyCode.RightArrow;
                    KeyCode findkey_dec = KeyCode.LeftArrow;
                    KeyCode findkey_for = KeyCode.UpArrow;
                    KeyCode findkey_back = KeyCode.DownArrow;

                    if ((exp.beginScreenSelect == 0) ||
                        ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                    {
                        findkey_inc = KeyCode.Alpha8;
                        findkey_dec = KeyCode.Alpha9;
                        findkey_for = KeyCode.Alpha6;
                        findkey_back = KeyCode.Alpha7;
                    }
                    if ((exp.currentStage == Experiment.TaskStage.SpatialRetrieval) ||
                            (exp.currentStage == Experiment.TaskStage.VerbalRetrieval))
                    {
                            if (Input.GetKeyDown(findkey_inc))
                            {
                                if (System.Math.Abs(speed - Configuration.slowSpeed) < 0.04f)
                                {
                                    float A = speed;
                                    speed = Fixspeed;
                                    Experiment.Instance.trialLogTrack.LogSpeedUp(A,speed);
                                }
                                else if ((System.Math.Abs(speed - Fixspeed) < 0.04f) ||
                                     (System.Math.Abs(speed - Configuration.fastSpeed) < 0.04f))
                                {
                                    float A = speed;
                                    speed = Configuration.fastSpeed;
                                    Experiment.Instance.trialLogTrack.LogSpeedUp(A, speed);
                                }
                            }

                            if (Input.GetKeyDown(findkey_dec))
                            {
                                if (System.Math.Abs(speed - Configuration.fastSpeed) < 0.04f)
                                {
                                    float A = speed;
                                    speed = Fixspeed;
                                    Experiment.Instance.trialLogTrack.LogSpeedDown(A, speed);
                                }
                                else if ((System.Math.Abs(speed - Fixspeed) < 0.04f) ||
                                         (System.Math.Abs(speed - Configuration.slowSpeed) < 0.04f))
                                {
                                    float A = speed;
                                    speed = Configuration.slowSpeed;
                                    Experiment.Instance.trialLogTrack.LogSpeedDown(A, speed);
                                }
                            }

                        if (exp.currentStage == Experiment.TaskStage.SpatialRetrieval)
                        {
                            if ((exp.beginScreenSelect == 0) ||
                                ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
                            {
                                if (Input.GetKey(findkey_for))
                                {
                                    StartCoroutine(exp.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));

                                }
                                else if (Input.GetKey(findkey_back))
                                {
                                    StartCoroutine(exp.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Reverse));

                                }
                            }
                        }

                     }
                     else
                     {
                            float A = speed;
                            speed = Fixspeed;
                            if (System.Math.Abs(A - speed) > 0.04f)
                            {
                                Experiment.Instance.trialLogTrack.LogDefaultFixSpeed(speed);
                            }
                     }
                    //}
                    //speed = 1.05f;
                    if (playbackDirection == 1)
                        timeVar += Time.deltaTime * speed;
                    else
                        timeVar -= Time.deltaTime * speed;

                    currentFrame = (int)(timeVar * frameRate);

                    if (Mathf.Abs(currentFrame - Experiment.nextSpawnFrame) < 12)
                    {

                        if (!isInvoked)
                        {
                            UnityEngine.Debug.Log("invoking spawn point event");
                            isInvoked = true;
                            if (Experiment.Instance.currentStage == Experiment.TaskStage.Encoding)
                            {
                                UnityEngine.Debug.Log("VideoLayer: SPAWN POINT");
                                spawnPointReachedEvent.Invoke();
                            }
                            else
                            {
                                UnityEngine.Debug.Log("VideoLayer: RETRIEVAL POINT");
                                Experiment.Instance.uiController.selectionControls.alpha = 0f;
                                retrievalPointReachedEvent.Invoke();
                            }

                        }
                    }
                }
                else {
                    //speed = Fixspeed;
                }
                //UnityEngine.Debug.Log("Current frame: " + currentFrame);
                if ((
                     (currentFrame > 300 && currentFrame < 310) ||
                     (currentFrame > 520 && currentFrame < 530) ||
                     (currentFrame > 770 && currentFrame < 780) ||
                     (currentFrame > 1100 && currentFrame < 1110)
                    ))
                {
                    if (boolLoggingCorner == false) {
                        boolLoggingCorner = true;
                        Experiment.Instance.expLogCorner(currentFrame);
                    }
                }
                else {
                    boolLoggingCorner = false;
                }

                if (currentFrame >= frames.Length-1)
                {
                    UnityEngine.Debug.Log("exceeded video: " + LapCounter.lapCount);
                    UnityEngine.Debug.Log(frames.Length);
                    timeVar = 0f;
                    OnVideoLoop();
                    currentFrame = 0;
                    /*if (LapCounter.lapCount >= 5) {
                        Experiment.Instance.setexpAct();
                    }*/
                }
                if (currentFrame < 0)
                {
                    UnityEngine.Debug.Log("before start");
                    currentFrame = frames.Length - 3;
                    OnVideoReverseLoop();
                    timeVar = currentFrame / frameRate;
                }
                bgLayer.texture = frames[currentFrame];
            }
            else
            {
                //UnityEngine.Debug.Log(gameObject.name + " is paused");
            }
            yield return 0;
        }

        //if (Input.GetKeyDown(KeyCode.Y))
        //    speed += 0.1f;
        //if (Input.GetKeyDown(KeyCode.H))
        //    speed -= 0.1f;
        yield return null;
    }
    /*
    private void FixedUpdate()
    {
        if (Mathf.Abs(currentFrame - Experiment.nextSpawnFrame) < 12)
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
            UnityEngine.Debug.Log("exceeded video");
            timeVar = 0f;
            OnVideoLoop();
            currentFrame = 0;
        }
        if (currentFrame < 0)
        {
            UnityEngine.Debug.Log("before start");
            currentFrame = frames.Length - 3;
            OnVideoReverseLoop();
            timeVar = currentFrame / frameRate;
        }
        //UnityEngine.Debug.Log("current frame " + currentFrame.ToString());
        //UnityEngine.Debug.Log("timevar " + timeVar.ToString());
        //UnityEngine.Debug.Log("associating frame" + frames[currentFrame].name.ToString());
        //Experiment.Instance.trialLogTrack.LogFramePosition(currentFrame);
        bgLayer.texture = frames[currentFrame];
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


            //if (Input.GetKeyDown(KeyCode.Y))
            //    speed += 0.1f;
            //if (Input.GetKeyDown(KeyCode.H))
            //    speed -= 0.1f;
        }
    }
    */

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

    public IEnumerator TogglePause(bool shouldPause)
    {
        //UnityEngine.Debug.Log("should pause? " + shouldPause.ToString());
        if (shouldPause)
        {
            
            //UnityEngine.Debug.Log(gameObject.name  + " paused");
            isPaused = true;
        }
        //videoPlayer.Pause();
        else
        {
            //UnityEngine.Debug.Log(gameObject.name + " unpaused");
            isPaused = false;
            
        }
        UnityEngine.Debug.Log("TogglePause: isPaused: : " + isPaused);
        //videoPlayer.Play();
        yield return null;
    }



    public IEnumerator ScrollToFrame(long frameNum)
    {
        UnityEngine.Debug.Log("scrolling to frame " + frameNum.ToString() + " for  " + gameObject.name);
        int targetFrame = (int)frameNum;
        float targetTime = targetFrame / frameRate;
        timeVar = targetTime; //update time var and the frame will be set correspondigly so when the Update loop runs next time
        currentFrame = (int)frameNum;
        //videoPlayer.frame = frameNum;

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
