using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.Vehicles.Car;

public class Experiment : MonoBehaviour {

    //EXPERIMENT IS A SINGLETON
    private static Experiment _instance;
    public ObjectManager objManager;
    public UIController uiController;
    public ObjectController objController;

    public InterfaceManager interfaceManager;
    public RamulatorInterface ramulatorInterface;

    public GameObject player;

    public static bool isPaused = false;

    //to determine if keypresses can page forward/backwards UI instructions
    private bool uiActive = false;
    //to determine if keypresses can select onscreen options
    private bool canSelect = false;

    //audio clips
    public AudioClip magicWand;

    //video player
    public VideoLayerManager videoLayerManager;

    public GameObject imagePlanePrefab;

    public List<Transform> spawnableWaypoints;
    //public List<Transform> rightSpawnableWaypoints;

    public PostProcessVolume ppVolumeRef;

    public PostProcessProfile pp_Day;
    public PostProcessProfile pp_Rainy;
    public PostProcessProfile pp_Night;

    private List<int> retrievalTypeList;
    private List<int> weatherChangeTrials; // this list will maintain the index of trials where encoding and retrieval weather condtions will be distinct
    private List<WeatherPair> weatherPairs;
    private List<int> randomizedWeatherOrder; //stores the order which determines weather of trials where weather doesn't change

    private Weather currentWeather;
    private Weather encodingWeather;
    private Weather retrievalWeather;

    private List<GameObject> stimuliBlockSequence; //sequence of encoding stimuli for the current block; utilized in tests at the end of each block
    private List<GameObject> contextDifferentWeatherTestList;
    private List<GameObject> contextSameWeatherTestList;

    //private Dictionary<Configuration.WeatherMode, Configuration.WeatherMode> retrievalWeatherMode;

    //used to control what page of instructions are shown;  when relevant
    private int currUIPageID = 0;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
    public bool skipEncoding = false;
        public bool skipVerbalRetrieval = false;
        public bool skipSpatialRetrieval = false;
#else
    private bool skipEncoding = false;
    private bool skipVerbalRetrieval = false;
    private bool skipSpatialRetrieval = false;
#endif

    private float carSpeed = 0f; //this is used exclusively to control car speed directly during spatial retrieval phase


    //traffic light controller
    public TrafficLightController trafficLightController;

    public enum TaskStage
    {
        ItemScreening,
        TrackScreening,
        Practice,
        Encoding,
        SpatialRetrieval,
        VerbalRetrieval,
        BlockTests,
        Retrieval,
        Feedback,
        WeatherFamiliarization,
        PostTaskScreening
    }

    public TaskStage currentStage = TaskStage.ItemScreening;

    public List<GameObject> spawnedObjects;
    //public List<Vector3> spawnLocations;
    public List<int> spawnFrames;

    private List<int> sortedSpawnFrames;
    private List<int> sortedRetrievalFrames;

    public List<GameObject> lureObjects;
    //public List<Vector3> lureLocations;
    public List<int> lureFrames;

    private List<GameObject> retrievalObjList;
    private List<Vector3> retrievalPositions;
    private List<int> retrievalFrames;

    public List<Transform> startableTransforms;
     
    public WaypointCircuit waypointCircuit;

    public static int recallTime = 6;

    public static int totalTrials = 12;
    private int blockCount = 0;
    public static int trialsPerBlock = 4;

    public static int listLength = 5;

    private int testLength = 10;

    public Transform startTransform;

    private bool wasMovingForward = false;
    private bool wasMovingReverse = false;
    private float movementTimer = 0f;

    private GameObject leftSpawnObj;
    private GameObject rightSpawnObj;


    private int currentMaxFrames = 0;


    //blackrock variables
    public static string ExpName = "CityBlock";
    public static string BuildVersion = "0.9.92";
    public static bool isSystem2 = false;

    public bool verbalRetrieval = false;

    //track screening
    public GameObject overheadCam;
    public GameObject trackFamiliarizationQuad;
    public GameObject feedbackQuad;
    public GameObject playerIndicatorSphere;

    //spatial retrieval
    public GameObject correctIndicator;
    public GameObject wrongIndicator;
    private List<bool> spatialFeedbackStatus;
    private List<Vector3> spatialFeedbackPosition;

    //logging
    public static bool isLogging = false;
    private string subjectLogfile; //gets set based on the current subject in Awake()
    public Logger_Threading subjectLog;
    private string eegLogfile; //gets set based on the current subject in Awake()
    public Logger_Threading eegLog;
    public string sessionDirectory;
    public static string sessionStartedFileName = "sessionStarted.txt";
    public static int sessionID;

    public string subjectName = "";

    public SubjectReaderWriter subjectReaderWriter;

    public List<BlockTestPair> blockTestPairList;

    private bool subjectInfoEntered = false;
    private bool ipAddressEntered = false;

    public IPAddress targetAddress;

    public static string defaultLoggingPath; //SET IN RESETDEFAULTLOGGINGPATH();
                                             //string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
                                             //public Text defaultLoggingPathDisplay;
                                             //public InputField loggingPathInputField;

    public Dictionary<int, Transform> playerPosDict = new Dictionary<int,Transform>();

    public static Subject currentSubject
    {
        get { return _currentSubject; }
        set
        {
            _currentSubject = value;
            //fileName = "TextFiles/" + _currentSubject.name + "Log.txt";
        }
    }

    private static Subject _currentSubject;
    public SubjectSelectionController subjectSelectionController;


    public Dictionary<int, GameObject> retrievalFrameObjectDict;


    public SimpleTimer lapTimer;

    public GameObject chequeredFlag;

    private float fixedTime = 1f;

    private int maxLaps = 1;

    public Transform targetWaypoint;

    private float prevLapTime = 0f;
    private float bestLapTime = 1000f;


    public TrialLogTrack trialLogTrack;
    private int objLapper = 0;

    public AudioRecorder audioRecorder;
    private int retCount = 0;
    private int trialCount = 0;


    public static bool isPractice = false;

    private string camTransformPath;

    private bool retrievedAsNew = false;

    private Scene currScene;


    public Material clearSkybox;
    public Material overcastSkybox;
    public Material nightSkybox;

    public static int nextSpawnFrame = -10000; //if no spawn, then it will be at -1000


    public TCPServer tcpServer;
    public static Experiment Instance {
        get {
            return _instance;
        }
    }


    void Awake() {
        if (_instance != null) {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;
        tcpServer.gameObject.SetActive(false);
        defaultLoggingPath = Application.dataPath;
        trackFamiliarizationQuad.SetActive(false);
        carSpeed = 0f;

        //initialize the weather as Night, by default
        currentWeather = new Weather(Weather.WeatherType.Rainy);
        ChangeLighting(currentWeather);


        //test length is stimuli items + lure items
        testLength = listLength + Configuration.luresPerTrial;
        



    }
    // Use this for initialization
    void Start()
    {
        //player.GetComponent<CarController>().ChangeMaxSpeed(40f);
        spatialFeedbackStatus = new List<bool>();
        spatialFeedbackPosition = new List<Vector3>();
        StartCoroutine("BeginExperiment");
        spawnedObjects = new List<GameObject>();
        //spawnLocations = new List<Vector3>();
        spawnFrames = new List<int>();
        lureObjects = new List<GameObject>();
        //lureLocations = new List<Vector3>();
        lureFrames = new List<int>();
        retrievalObjList = new List<GameObject>();
        retrievalPositions = new List<Vector3>();
        retrievalFrames = new List<int>();

        blockCount = totalTrials / trialsPerBlock;
        retrievalFrameObjectDict = new Dictionary<int, GameObject>();

#if UNITY_EDITOR_OSX
        camTransformPath = Application.dataPath + "/cam_transform.txt";
#endif

        StartCoroutine("ReadCamTransform");


    }

    IEnumerator ReadCamTransform()
    {
        string path = camTransformPath;

        using (StreamReader sr = new StreamReader(path))
        {
            int index = 0;
            while (sr.Peek() >= 0)
            {
                string currentLine = sr.ReadLine();
                string currIndex = currentLine.Split(':')[1];
                string currPos = currIndex.Split('R')[0];
                string currRot = currentLine.Split(':')[2];

                float posX = float.Parse(currPos.Split(',')[0]);
                float posY = float.Parse(currPos.Split(',')[1]);
                float posZ = float.Parse(currPos.Split(',')[2]);


                float rotX = float.Parse(currRot.Split(',')[0]);
                float rotY = float.Parse(currRot.Split(',')[1]);
                float rotZ = float.Parse(currRot.Split(',')[2]);

                Transform currTrans = gameObject.transform;
                currTrans.position = new Vector3(posX, posY, posZ);
                currTrans.eulerAngles = new Vector3(rotX, rotY, rotZ);

                //then add it to the dict
                playerPosDict.Add(index,currTrans);

                index++;
            }
        }


        yield return null;
    }


    //this changes the "time of the day" in the scene through lighting
    void ChangeLighting(Weather targetWeather)
    {
        currentWeather = targetWeather;
        currentMaxFrames = videoLayerManager.GetTotalFramesOfCurrentClip();

        //unload the current scene first,if one is loaded

        //if (currScene != null && currScene.IsValid())
        //    SceneManager.UnloadSceneAsync(currScene);


        switch (targetWeather.weatherMode)
        {
            case Weather.WeatherType.Sunny:
                UnityEngine.Debug.Log("load sunny");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Sunny);
                //SceneManager.LoadScene("DayLighting", LoadSceneMode.Additive);
                //currScene = SceneManager.GetSceneByName("DayLighting");
                //ppVolumeRef.profile = pp_Day;
                //RenderSettings.skybox = clearSkybox;
                break;
            case Weather.WeatherType.Rainy:
                UnityEngine.Debug.Log("load rainy");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Rainy);
                //SceneManager.LoadScene("RainyLighting", LoadSceneMode.Additive);
                //currScene = SceneManager.GetSceneByName("RainyLighting");
                //ppVolumeRef.profile = pp_Rainy;
                //RenderSettings.skybox = overcastSkybox;
                break;
            case Weather.WeatherType.Night:
                UnityEngine.Debug.Log("load night");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Night);
                //load dusk scene by default first
                //SceneManager.LoadScene("DuskLighting", LoadSceneMode.Additive);
                //currScene = SceneManager.GetSceneByName("DuskLighting");
                //ppVolumeRef.profile = pp_Night;
                //RenderSettings.skybox = nightSkybox;
                break;

        }
    }



    public void MarkIPAddrEntered()
    {
        ipAddressEntered = true;
    }

    //TODO: move to logger_threading perhaps? *shrug*
    IEnumerator InitLogging()
    {
        string subjectDirectory = defaultLoggingPath + "/" + subjectName + "/";
        sessionDirectory = subjectDirectory + "session_0" + "/";
        sessionID = 0;
        string sessionIDString = "_0";

        if (!Directory.Exists(subjectDirectory))
        {
            Directory.CreateDirectory(subjectDirectory);
        }
        while (File.Exists(sessionDirectory + sessionStartedFileName))
        {
            sessionID++;

            sessionIDString = "_" + sessionID.ToString();

            sessionDirectory = subjectDirectory + "session" + sessionIDString + "/";
        }

        //delete old files.
        if (Directory.Exists(sessionDirectory))
        {
            DirectoryInfo info = new DirectoryInfo(sessionDirectory);
            FileInfo[] fileInfo = info.GetFiles();
            for (int i = 0; i < fileInfo.Length; i++)
            {
                File.Delete(fileInfo[i].ToString());
            }
        }
        else
        { //if directory didn't exist, make it!
            Directory.CreateDirectory(sessionDirectory);
        }

        subjectLog.fileName = sessionDirectory + subjectName + "Log" + ".txt";
        eegLog.fileName = sessionDirectory + subjectName + "EEGLog" + ".txt";


        yield return null;
    }


    //In order to increment the session, this file must be present. Otherwise, the session has not actually started.
    //This accounts for when we don't successfully connect to hardware -- wouldn't want new session folders.
    //Gets created in TrialController after any hardware has conneinitcted and the instruction video has finished playing.
    public void CreateSessionStartedFile()
    {
        StreamWriter newSR = new StreamWriter(sessionDirectory + sessionStartedFileName);
    }

    IEnumerator ConnectToElemem()
    {
       // ramulatorInterface.StartThread();
        yield return StartCoroutine(ramulatorInterface.BeginNewSession(sessionID));
        yield return null;
    }

    public void ParseSubjectCode()
    {
        subjectName = uiController.subjectInputField.text;
        UnityEngine.Debug.Log("got subject name");
        subjectInfoEntered = true;
    }

    IEnumerator GetSubjectInfo()
    {
        subjectInfoEntered = false;
        uiController.subjectInfoPanel.gameObject.SetActive(true);
        uiController.subjectInfoPanel.alpha = 1f;
        while (!subjectInfoEntered)
        {
            yield return 0;
        }
        uiController.subjectInfoPanel.alpha = 0f;
        uiController.subjectInfoPanel.gameObject.SetActive(false);
        yield return null;
    }


    IEnumerator BeginExperiment()
    {
        //skip this if we're in the web version
#if !UNITY_WEBGL
        yield return StartCoroutine(GetSubjectInfo());
        subjectName = "subj_" + GameClock.SystemTime_MillisecondsString;
#endif

#if !UNITY_EDITOR
        yield return StartCoroutine(InitLogging());
#endif
        UnityEngine.Debug.Log("set subject name: " + subjectName);
        trialLogTrack.LogBegin();
        //only run if system2 is expected
        
    if (isSystem2)
    {
            /*
        uiController.ipEntryPanel.alpha = 1f;

        while(!ipAddressEntered)
        {
            yield return 0;
        }
        int portNum = int.Parse(uiController.ipAddrInput.text);
        UnityEngine.Debug.Log("target port  " + portNum.ToString());
        TCP_Config.ConnectionPort = portNum;
        ipAddressEntered = false;
        uiController.ipEntryPanel.alpha = 0f;
        
        */
        
     //   tcpServer.gameObject.SetActive(true);
            uiController.blackrockConnectionPanel.alpha = 1f;


            interfaceManager.Do(new EventBase(interfaceManager.LaunchExperiment));
            //   yield return StartCoroutine(ConnectToElemem());


            trialLogTrack.LogBlackrockConnectionAttempt();
        uiController.connectionText.text = "Attempting to connect with server...";
        //wait till the SYS2 Server connects
        while (!tcpServer.isConnected)
        {
            yield return 0;
        }
        uiController.connectionText.text = "Waiting for server to start...";
        while (!tcpServer.canStartGame)
        {
            yield return 0;
        }

        uiController.blackrockConnectionPanel.alpha = 0f;
    }
    else
    {
        uiController.blackrockConnectionPanel.alpha = 0f;
    }
    trialLogTrack.LogBlackrockConnectionSuccess();
    
        uiController.blackrockConnectionPanel.alpha = 0f;
        trialLogTrack.LogBlackrockConnectionSuccess();


/////REMOVE THIS
///
 /*
        Experiment.Instance.uiController.connectionSuccessPanel.alpha = 1f;
        while(!Input.GetKeyDown(KeyCode.Escape))
        {
            
            yield return 0;
        }
        Application.Quit();
        */

        ///END REMOVE THIS
        ///
        trialLogTrack.LogIntroInstruction(true);
        uiController.taskIntroPanel.alpha = 1f;

        yield return StartCoroutine(WaitForActionButton());
        uiController.taskIntroPanel.alpha = 0f;
        trialLogTrack.LogIntroInstruction(false);


       // player.GetComponent<CarMover>().TurnCarEngine(true);


        verbalRetrieval = false;

        //setting up car but do not move it yet
        //StartCoroutine(player.GetComponent<CarMover>().MoveCar());
        //SetCarMovement(false);
        

        //track familiarization
          //yield return StartCoroutine("BeginTrackScreening");

        //practice
         yield return StartCoroutine("BeginPractice");

        yield return StartCoroutine("PrepTrials"); //will perform all necessary trial and weather randomization

        //repeat blocks twice
        for (int i = 0; i < blockCount; i++)
        {
            yield return StartCoroutine("BeginTaskBlock");
        }

        //once all the trials are complete, run the followup test
        yield return StartCoroutine("RunFollowUpTest");

        uiController.endSessionPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());

        //player.GetComponent<CarMover>().TurnCarEngine(false);
        Application.Quit();
        yield return null;
    }




    IEnumerator RandomizeTravelSpeed()
    {
        while (currentStage != Experiment.TaskStage.PostTaskScreening)
        {
            while (!player.GetComponent<Rigidbody>().isKinematic)
            {
                float timer = 0f;
                float maxTime = Random.Range(3f, 10f);
                float speed = Random.Range(30f, 60f);
                UnityEngine.Debug.Log("new max time " + maxTime.ToString());
                while (timer < maxTime)
                {
                    timer += Time.deltaTime;
                    yield return 0;
                }
                player.GetComponent<CarController>().ChangeMaxSpeed(speed);
                //player.GetComponent<CarController>().SetCurrentSpeed(speed);
                UnityEngine.Debug.Log("changed speed " + speed.ToString());
                timer = 0f;
                yield return 0;
            }

            yield return 0;
        }
        yield return null;
    }



    IEnumerator BeginTrackScreening()
    {
        UnityEngine.Debug.Log("starting track screening");
        currentStage = TaskStage.TrackScreening;
        //overheadCam.SetActive(true);
        //trackFamiliarizationQuad.SetActive(true);
        //playerIndicatorSphere.SetActive(true);
        trialLogTrack.LogTaskStage(currentStage, true);


        uiController.trackScreeningPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());
        uiController.trackScreeningPanel.alpha = 0f;
        //player.gameObject.SetActive(true);
        //trafficLightController.MakeVisible(true);
        //yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        //SetCarMovement(true);
        yield return StartCoroutine(videoLayerManager.ResumePlayback());

        //do one single lap of the sunny weather
        

        trafficLightController.MakeVisible(false);
        while (LapCounter.lapCount < 1)
        {
            yield return 0;
        }
        //  SetCarMovement(false);

        yield return StartCoroutine(videoLayerManager.ReturnToStart());
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        LapCounter.lapCount = 0;
        //SetCarMovement(false);
        //overheadCam.SetActive(false);
        //trackFamiliarizationQuad.SetActive(false);
        //playerIndicatorSphere.SetActive(false);
        trialLogTrack.LogTaskStage(currentStage, false);

        UnityEngine.Debug.Log("finished track screening");
        yield return null;
    }


    IEnumerator BeginPractice()
    {
        UnityEngine.Debug.Log("beginning practice");
        isPractice = true;
        currentStage = TaskStage.Practice;
        yield return StartCoroutine(ShowPracticeInstructions("PreEncoding"));

        ////run encoding
        //yield return StartCoroutine("RunEncoding");

        //////run retrieval
        //verbalRetrieval = false;
        //currentStage = TaskStage.SpatialRetrieval;
        //trialLogTrack.LogTaskStage(currentStage, true);
        //yield return StartCoroutine("RunSpatialRetrieval");
        //trialLogTrack.LogTaskStage(currentStage, false);

        //ToggleFixation(true);
        //yield return new WaitForSeconds(1f);
        //yield return StartCoroutine("ResetTrack");
        //ToggleFixation(false);


        //yield return StartCoroutine(ShowPracticeInstructions("SecondEncoding"));

        ////run encoding
        //yield return StartCoroutine("RunEncoding");

        //verbalRetrieval = true;
        //currentStage = TaskStage.VerbalRetrieval;
        //trialLogTrack.LogTaskStage(currentStage, true);


        ////run retrieval
        //yield return StartCoroutine("RunVerbalRetrieval");
        //trialLogTrack.LogTaskStage(currentStage, false);

        //ToggleFixation(true);
        //yield return new WaitForSeconds(0.5f);
        //yield return StartCoroutine("ResetTrack");
        //ToggleFixation(false);


        //do two more practice laps with randomized retrieval conditions

        //List<int> randRetrievalOrder = UsefulFunctions.ReturnShuffledIntegerList(2);

        //for (int i=0;i<2;i++)
        //{
        //    yield return StartCoroutine("RunEncoding");

        //    int retrievalType = randRetrievalOrder[i];

        //    switch (retrievalType)
        //    {
        //        case 0:
        //            verbalRetrieval = true;
        //            currentStage = TaskStage.VerbalRetrieval;
        //            trialLogTrack.LogTaskStage(currentStage, true);


        //            //run retrieval
        //            yield return StartCoroutine("RunVerbalRetrieval");
        //            break;
        //        case 1:
        //            verbalRetrieval = false;
        //            currentStage = TaskStage.SpatialRetrieval;
        //            trialLogTrack.LogTaskStage(currentStage, true);
        //            yield return StartCoroutine("RunSpatialRetrieval");
        //            break;


        //    }

        //    trialLogTrack.LogTaskStage(currentStage, false);
        //    ToggleFixation(true);
        //    yield return new WaitForSeconds(0.5f);
        //    yield return StartCoroutine("ResetTrack");
        //    ToggleFixation(false);
        //}

        yield return StartCoroutine(ShowPracticeInstructions("PreWeather"));
        currentStage = TaskStage.WeatherFamiliarization;

        trialLogTrack.LogTaskStage(currentStage, true);
        yield return StartCoroutine("RunWeatherFamiliarization");
        trialLogTrack.LogTaskStage(currentStage, false);


        isPractice = false;
        yield return null;
    }

    public void ShowTurnDirection(WaypointProgressTracker.TrackDirection turnDirection)
    {
        switch (turnDirection)
        {
            case WaypointProgressTracker.TrackDirection.Left:
                uiController.leftTurnArrow.alpha = 1f;
                break;
            case WaypointProgressTracker.TrackDirection.Right:
                uiController.rightTurnArrow.alpha = 1f;
                break;
        }
    }

    public void HideTurnDirection(WaypointProgressTracker.TrackDirection turnDirection)
    {
        switch (turnDirection)
        {
            case WaypointProgressTracker.TrackDirection.Left:
                uiController.leftTurnArrow.alpha = 0f;
                break;
            case WaypointProgressTracker.TrackDirection.Right:
                uiController.rightTurnArrow.alpha = 0f;
                break;
        }

    }

    public IEnumerator BeginCrashSequence(Transform crashZone)
    {
        SetCarMovement(true);
        Vector3 origTransform = player.transform.position;
        float lerpTimer = 0f;

        while (lerpTimer < 2f)
        {
            lerpTimer += Time.deltaTime;
            player.transform.position = Vector3.Lerp(origTransform, crashZone.position, lerpTimer / 2f);
            yield return 0;
        }
        uiController.crashNotification.alpha = 1f;
        yield return new WaitForSeconds(2f);

        uiController.crashNotification.alpha = 0f;

        //conceal the transformation
        uiController.blackScreen.alpha = 1f;
        //make sure we transport the car back to the starting transform
        player.transform.position = startTransform.position;
        player.transform.rotation = startTransform.rotation;

        player.GetComponent<WaypointProgressTracker>().Reset(); //reset the waypoint system to begin from the beginning

        uiController.blackScreen.alpha = 0f;
        SetCarMovement(false);
        //	player.GetComponent<CarController>().SetCurrentSpeed(0f);
        yield return null;
    }

    IEnumerator ShowPracticeInstructions(string instType)
    {
        switch (instType)
        {
            case "PreEncoding":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preEncodingInstructions.enabled = true;
                yield return StartCoroutine(WaitForActionButton());
                uiController.preEncodingInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "SecondEncoding":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.secondEncodingInstructions.enabled = true;
                yield return StartCoroutine(WaitForActionButton());
                uiController.secondEncodingInstructions.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
            case "PreWeather":
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preWeatherCondition.enabled = true;
                yield return StartCoroutine(WaitForActionButton());
                uiController.preWeatherCondition.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                break;
        }
        yield return null;
    }

    IEnumerator ShowEncodingInstructions()
    {
        uiController.encodingPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());
        uiController.encodingPanel.alpha = 0f;
        yield return null;
    }

    public IEnumerator UpdateVerbalInstructions()
    {
        yield return StartCoroutine(ShowVerbalRetrievalInstructions(uiController.GetCurrentUIPage()));
        yield return null;
    }

    IEnumerator ShowVerbalRetrievalInstructions(int pageID)
    {
        UnityEngine.Debug.Log("setting spatial instruction to page : " + pageID.ToString());
        switch (pageID)
        {
            //page one
            case 0:
                uiController.verbalInstructionA.enabled = true;
                uiController.verbalInstructionB.enabled = false;
                uiController.verbalRetrievalPanel.alpha = 1f;
                break;
            //    yield return StartCoroutine(WaitForActionButton());

            //page two
            case 1:
                uiController.verbalInstructionA.enabled = false;
                uiController.verbalInstructionB.enabled = true;
                break;
            // yield return StartCoroutine(WaitForActionButton());
            case 2:
                uiController.verbalRetrievalPanel.alpha = 0f;
                break;

                //    
        }
        yield return null;
    }

    public IEnumerator UpdateSpatialInstructions()
    {
        UnityEngine.Debug.Log("updating spatial instructions");
        yield return StartCoroutine(ShowRetrievalInstructions(uiController.GetCurrentUIPage()));
        yield return null;
    }

    IEnumerator ShowRetrievalInstructions(int pageID)
    {
        UnityEngine.Debug.Log("setting spatial instruction to page : "  + pageID.ToString());
        switch (pageID)
        {
            case 0:
                uiController.practiceInstructionPanel.alpha = 1f;
                uiController.preSpatialRetrieval.enabled = true;
                break;
            //  yield return StartCoroutine(WaitForActionButton());
            case 1:
                uiController.preSpatialRetrieval.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;
                uiController.preSpatialRetrieval.enabled = false;
                uiController.practiceInstructionPanel.alpha = 0f;

                string itemName = objController.ReturnStimuliDisplayText();
                uiController.itemReactivationText.text = itemName;
                uiController.itemReactivationPanel.alpha = 1f;
                break;
            // yield return new WaitForSeconds(2f);
            case 2:
                uiController.spatialInstructionA.enabled = true;
                uiController.spatialInstructionB.enabled = false;
                uiController.retrievalPanel.alpha = 1f;
                break;
            //yield return StartCoroutine(WaitForActionButton());
            case 3:
                uiController.itemReactivationPanel.alpha = 0f;
                uiController.spatialInstructionA.enabled = false;
                uiController.spatialInstructionB.enabled = true;
                break;
            // yield return StartCoroutine(WaitForActionButton());
            case 4:
                uiController.itemReactivationPanel.alpha = 0f;
                uiController.retrievalPanel.alpha = 0f;
                break;

        }



        yield return null;
    }


    void ChangeUIPage(bool isForwards)
    {
        //we are moving one page forwards
        if(isForwards)
        {

        }
        //else we are moving one page backwards
        else
        {

        }
    }

    string FormatTime(float timeInSeconds)
    {
        int displayMinutes = (int)Mathf.Floor(timeInSeconds / 60.0f);
        int displaySeconds = (int)Mathf.Floor(timeInSeconds - (displayMinutes * 60));

        string result = "";
        if (displaySeconds < 10)
        {
            result = displayMinutes + ":0" + displaySeconds;
        }
        else
        {
            result = displayMinutes + ":" + displaySeconds;
        }

        return result;
    }
    /*
    IEnumerator ChangeWeather(Configuration.WeatherMode targetWeatherMode)
    {
        //transition into a black fixation screen
        ToggleFixation(true);

        //change the weather
        Configuration.currentWeatherMode = targetWeatherMode;


        //transition out
        ToggleFixation(false);
        yield return null;
    }
    */


    Weather FindPairedRetrievalWeather(Weather pairWeather)
    {
        Weather retrievalWeather = new Weather(Weather.WeatherType.Sunny); // create a default weather first
        //cycle through all the weather pairs until a matching weather to our argument is found
        for (int i=0;i<weatherPairs.Count;i++)
        {
            if(pairWeather.weatherMode == weatherPairs[i].encodingWeather.weatherMode)
            {
                UnityEngine.Debug.Log("matching weather pair found");
                retrievalWeather = weatherPairs[i].retrievalWeather;
                i = weatherPairs.Count; // once pair is found, we break out of the loop

            }
        }

        return retrievalWeather;
    }

    IEnumerator GenerateLureSpots()
    {
        List<Texture> lureImageTextures =  objController.SelectImagesForLures();

        for (int i = 0; i < Configuration.luresPerTrial; i++)
        {
            int associatedLureFrame = lureFrames[i];
            Transform currentLureTransform = GetTransformForFrame(associatedLureFrame);
            currentLureTransform.position += currentLureTransform.forward * 2.5f;
            GameObject lureParent = Instantiate(objController.placeholder, currentLureTransform.position,currentLureTransform.rotation) as GameObject;


            lureParent.GetComponent<StimulusObject>().stimuliDisplayTexture = lureImageTextures[i];
            lureParent.GetComponent<StimulusObject>().stimuliDisplayName = lureImageTextures[i].name;

            lureObjects.Add(lureParent);


            retrievalFrameObjectDict.Add(associatedLureFrame, lureParent);

            UnityEngine.Debug.Log("generated lure object " + lureParent.GetComponent<StimulusObject>().stimuliDisplayName + associatedLureFrame.ToString());
            //GameObject colliderBoxRef = Instantiate(objController.lureColliderPrefab, lureLocations[i], Quaternion.identity) as GameObject;
            //parent the collider box with the lure 
            //colliderBoxRef.transform.parent = lureParent.transform;

            ////associate the stimulus object 
            //lureParent.GetComponent<StimulusObject>().LinkColliderObj(colliderBoxRef);
            //lureParent.GetComponent<VisibilityToggler>().TurnVisible(false);

            //lureParent.GetComponent<StimulusObject>().stimuliDisplayTexture = lureImageTextures[i];
            //lureParent.GetComponent<StimulusObject>().stimuliDisplayName = lureImageTextures[i].name;

            //lureObjects.Add(lureParent);
        }


        yield return null;
    }

    IEnumerator RunEncoding()
    {
            UnityEngine.Debug.Log("in encoding now");
        currentStage = TaskStage.Encoding;

        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        LapCounter.lapCount = 0;
        if (isPractice)
        {
            yield return StartCoroutine(objController.SelectPracticeItems());
        }
        else
        {
            trialLogTrack.LogTaskStage(currentStage, true);
            yield return StartCoroutine(objController.SelectEncodingItems());
            trialLogTrack.LogInstructions(true);
            yield return StartCoroutine(ShowEncodingInstructions());
            trialLogTrack.LogInstructions(false);
        }

        //reset the waypoint tracker of the car
        //player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        //yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        //player.GetComponent<CarMover>().ResetWaypointTarget();

        //player.transform.position = startTransform.position;
        //    player.transform.rotation = startTransform.rotation;
            //int targetIndex = player.GetComponent<CarAIControl>().FindSubsequentWaypointIndexFromStart(player.transform); //this sets the target waypoint when you start from a random location; will ALWAYS be facing the forward direction
            //player.GetComponent<CarAIControl>().SetTarget(player.GetComponent<WaypointProgressTracker>().currentCircuit.Waypoints[targetIndex], targetIndex);

            //chequeredFlag.transform.position = startTransform.position;
            //chequeredFlag.transform.rotation = startTransform.rotation;

            //	player.GetComponent<CarAIControl>().ResetTargetToStart(); //reset waypoint target transform to forward facing the startTransform
            yield return StartCoroutine(PickEncodingLocations());

        yield return StartCoroutine(UpdateNextSpawnFrame());
            //yield return StartCoroutine(SpawnEncodingObjects()); //this will spawn all encoding objects on the track

        if (!skipEncoding)
        {
            yield return StartCoroutine(videoLayerManager.ResumePlayback());
            while (LapCounter.lapCount < 1)
            {
                //trafficLightController.MakeVisible(true);
                //SetCarMovement(false);
                //set drive mode to auto
                //player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
                //yield return StartCoroutine(trafficLightController.StartCountdownToGreen());




                //trafficLightController.MakeVisible(false);

                UnityEngine.Debug.Log("began lap number : " + LapCounter.lapCount.ToString());
                //SetCarMovement(true);
                LapCounter.canStop = false;
                while (!LapCounter.canStop)
                {
                    yield return 0;
                }
                //SetCarMovement(false);
                LapCounter.canStop = false;

                yield return StartCoroutine(videoLayerManager.PauseAllLayers());

                //can press spacebar to stop
                //trafficLightController.MakeVisible(true);
                //yield return StartCoroutine(trafficLightController.ShowRed());

                UnityEngine.Debug.Log("stopping now");
                float forceStopTimer = 0f;
                bool forceStopped = false;
                while (!forceStopped)
                {
                    forceStopTimer += Time.deltaTime;
                    if (forceStopTimer > 1.15f)
                    {
                        forceStopTimer = 0f;
                        forceStopped = true;
                    }
                    yield return 0;
                }

                forceStopped = false;


                //trafficLightController.MakeVisible(false);

                //reset collisions for encoding objects
                //for (int k = 0; k < spawnedObjects.Count; k++)
                //{
                //    if (spawnedObjects[k] != null)
                //        //spawnedObjects[k].GetComponent<StimulusObject>().ToggleCollisions(true);
                //}


                //yield return new WaitForSeconds(1f);
                ToggleFixation(true);

                //return the video to the start
                yield return StartCoroutine(videoLayerManager.ReturnToStart());

                float totalFixationTime = fixedTime + UnityEngine.Random.Range(0.1f, 0.3f);
                yield return new WaitForSeconds(totalFixationTime);
                ToggleFixation(false);

                SetCarMovement(false);
                player.transform.position = startTransform.position;

                if(!isPractice)
                {

                    trialLogTrack.LogTaskStage(currentStage, false);
                }
                yield return 0;
            }
        }

            

            yield return null;
        }

        IEnumerator ResetTrack()
        {
        UnityEngine.Debug.Log("resetting track");
        UnityEngine.Debug.Log("spawned object count " + spawnedObjects.Count.ToString());
            for (int k = 0; k < spawnedObjects.Count; k++)
            {
                //destroy the ItemColliderBox which is the parent
                Destroy(spawnedObjects[k].transform.gameObject);
            }

            //reset everything before the next block begins
            spawnFrames.Clear();
            spawnedObjects.Clear();

        //destroy all lure objects
        for(int i=0;i<Configuration.luresPerTrial;i++)
        {
            if (i < lureObjects.Count)
            {
                if (lureObjects[i] != null)
                    Destroy(lureObjects[i]);
            }
        }
            //reset lure lists as well
            lureFrames.Clear();
            lureObjects.Clear();

            //player.GetComponent<CarController>().ChangeMaxSpeed(40f);
            chequeredFlag.SetActive(true);

        //make sure video is paused and returned to start
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        yield return StartCoroutine(videoLayerManager.ReturnToStart());

            //	objController.encodingList.Clear();
            LapCounter.lapCount = 0;
            player.transform.position = startTransform.position;
            yield return null;
        }

    IEnumerator PrepTrials()
    {
           GenerateRandomizedRetrievalConditions();
           yield return null;
    }

    //this will generate fresh lists of randomized retrieval order as well as weather differences
    void GenerateRandomizedRetrievalConditions()
    {
        retrievalTypeList = new List<int>();
        weatherChangeTrials = new List<int>();
        randomizedWeatherOrder = new List<int>();


        retrievalTypeList  = UsefulFunctions.ReturnShuffledIntegerList(totalTrials);

        //changing weather trials will be interleaved, so an ordered list of ints will suffice
        weatherChangeTrials = UsefulFunctions.ReturnListOfOrderedInts(totalTrials);

        //only half the trials will have same weather
        randomizedWeatherOrder = UsefulFunctions.ReturnShuffledIntegerList(totalTrials / 2);

        //generate different combinations of weather pairs for trials with different encoding and retrieval weathers; will have 3P2 permutations
        weatherPairs = new List<WeatherPair>();
        weatherPairs = GenerateWeatherPairs();

        
        

        //List<int> tempWeatherList = UsefulFunctions.ReturnShuffledIntegerList(blockLength/2); //we will only have different weather for nine trials

    }

    List<WeatherPair> GenerateWeatherPairs()
    {
        List<WeatherPair> resultPair = new List<WeatherPair>();
        int numWeathers = Configuration.ReturnWeatherTypes();
        for (int i=0;i<numWeathers; i++)
        {
            List<int> possibleWeatherCombinations = UsefulFunctions.ReturnListOfOrderedInts(numWeathers);
            possibleWeatherCombinations.RemoveAt(i); //remove self
            //store the current index's weather type enum into a variable
            Weather.WeatherType selfType = (Weather.WeatherType)i;

            //cycle through all possible combinations
            for(int j=0;j<possibleWeatherCombinations.Count;j++)
            {
                WeatherPair encodingPair = new WeatherPair(selfType, (Weather.WeatherType)possibleWeatherCombinations[j]);
                //WeatherPair retrievalPair = new WeatherPair((Weather.WeatherType)possibleWeatherCombinations[j],selfType);

                resultPair.Add(encodingPair);
                UnityEngine.Debug.Log("E: " + encodingPair.encodingWeather.weatherMode.ToString() + " R: " + encodingPair.retrievalWeather.weatherMode.ToString()); 
              //  resultPair.Add(retrievalPair);
              //  UnityEngine.Debug.Log("E: " + retrievalPair.encodingWeather.weatherMode.ToString() + " R: " + retrievalPair.retrievalWeather.weatherMode.ToString());
            }
        }

        return resultPair;
    }

    IEnumerator BeginTaskBlock()
    {
        //reset the block lists
        stimuliBlockSequence = new List<GameObject>();

        for (int i = 0; i < trialsPerBlock; i++)
            {
                trialCount = i + 1;
                yield return StartCoroutine("CheckForWeatherChange", TaskStage.Encoding);
                //run encoding
                yield return StartCoroutine("RunEncoding");


                //check to see if the weather should change between the encoding and retrieval
                yield return StartCoroutine("CheckForWeatherChange",TaskStage.Retrieval);

                //run retrieval
                yield return StartCoroutine("RunRetrieval");

                ToggleFixation(true);
                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine("ResetTrack");
                ToggleFixation(false);
        }


            uiController.targetTextPanel.alpha = 0f;
            SetCarMovement(true);
            trialLogTrack.LogTaskStage(currentStage, false);

        yield return StartCoroutine(RunBlockTests());
        
        yield return null;
    }




    IEnumerator CheckForWeatherChange(TaskStage upcomingStage)
    {
        if(weatherChangeTrials[trialCount] % 2 == 0)
        {
            if(upcomingStage == TaskStage.Encoding)
            {
                //we want to keep the weather the same as the previous trial's retrieval weather; so we check the currentWeather and not change anything
                UnityEngine.Debug.Log("DIFF WEATHER TRIAL:  keeping the weather same as previous trial: " + currentWeather.weatherMode.ToString());

                //we try to a pair with matching encoding weather and retrieve its corresponding retrieval weather
                retrievalWeather = FindPairedRetrievalWeather(currentWeather); 
            }
            else
            {
                UnityEngine.Debug.Log("changing weather for retrieval to " + retrievalWeather.ToString());
                ChangeLighting(retrievalWeather);
            }
        }
        else
        {
            // int randWeatherIndex = randomizedWeatherOrder[trialCount/2];
            //switch(randWeatherIndex % 3)
            // {
            //     case 0:
            //         currentWeather = new Weather(Weather.WeatherType.Sunny);
            //         break;
            //     case 1:
            //         currentWeather = new Weather(Weather.WeatherType.Foggy);
            //         break;
            //     case 2:
            //         currentWeather = new Weather(Weather.WeatherType.Night);
            //         break;
            // }
            UnityEngine.Debug.Log("SAME WEATHER TRIAL:  keeping the weather same as previous trial: " + currentWeather.weatherMode.ToString());
            //UnityEngine.Debug.Log("trial with same weather: " + currentWeather.weatherMode.ToString());
            ChangeLighting(currentWeather);
        }
        yield return null;
    }

    IEnumerator RunRetrieval()
    {

        //retrieval time
        SetCarMovement(false);
        UnityEngine.Debug.Log("beginning retrieval");
        //hide encoding objects and text
        for (int j = 0; j < spawnedObjects.Count; j++)
        {
            //spawnedObjects[j].gameObject.SetActive(false);
            spawnedObjects[j].gameObject.GetComponent<VisibilityToggler>().TurnVisible(false);

        }


        //	currentStage = Experiment.TaskStage.Retrieval;

        //pick a randomized starting retrieval position
      //  List<Transform> validStartTransforms = GetValidStartTransforms(); //get valid waypoints that don't have an object already spawned there
     //   int randWaypoint = UnityEngine.Random.Range(0, validStartTransforms.Count - 1);

     //   Transform randStartTransform = validStartTransforms[randWaypoint];

        //disable player collider box before transporting to new location

        player.GetComponent<CarMover>().playerRigidbody.GetComponent<Rigidbody>().isKinematic = true;

        //player.transform.position = randStartTransform.position;
        //player.transform.rotation = randStartTransform.rotation;
        //chequeredFlag.transform.position = randStartTransform.position;
        //chequeredFlag.transform.rotation = randStartTransform.rotation;


        //player.GetComponent<CarMover>().ResetTargetWaypoint(randStartTransform);

        trialLogTrack.LogRetrievalStartPosition(player.transform.position);

        player.GetComponent<CarMover>().playerRigidbody.GetComponent<Rigidbody>().isKinematic = false;

        player.GetComponent<CarMover>().Reset();
        //player.GetComponent<WaypointProgressTracker>().Reset();

        LapCounter.lapCount = 0; //reset lap count for retrieval 

        string targetNames = "";

        //check the randomly ordered list to see what the retrieval type should be
        if (retrievalTypeList[trialCount] % 2 == 0)
        {
            verbalRetrieval = false;
            currentStage = TaskStage.SpatialRetrieval;
        }
        else
        {
            verbalRetrieval = true;

            currentStage = TaskStage.VerbalRetrieval;
        }

       trialLogTrack.LogTaskStage(currentStage, true);

        bool finishedRetrieval = false;
        retCount = 0;

        //set drive mode to manual
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Manual);

        while (LapCounter.lapCount < 10 && !finishedRetrieval)
        {
            if (verbalRetrieval)
            {
                if (!skipVerbalRetrieval)
                {
                   yield return StartCoroutine("RunVerbalRetrieval");

                }
                finishedRetrieval = true;
            }
            else
            {
                if (!skipSpatialRetrieval)
                {
                    yield return StartCoroutine("RunSpatialRetrieval");
                    

                   
                }
            }
            currentStage = TaskStage.Feedback;
            UnityEngine.Debug.Log("finished all retrievals");


            finishedRetrieval = true;
            SetCarMovement(false);

            uiController.fixationPanel.alpha = 1f;

            uiController.targetTextPanel.alpha = 0f;
            /*
            uiController.spatialRetrievalFeedbackPanel.alpha = 1f;
            yield return StartCoroutine("PerformSpatialFeedback");
            UnityEngine.Debug.Log("finished spatial feedback");
            uiController.spatialRetrievalFeedbackPanel.alpha = 0f;
            */

            trafficLightController.MakeVisible(false);
            yield return new WaitForSeconds(1f);
            uiController.fixationPanel.alpha = 0f;

            //set the car movement in forward direction
            yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
            yield return 0;
        }

        finishedRetrieval = false;
        yield return null;
    }

    IEnumerator RunVerbalRetrieval()
    {
        //player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        //yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        //player.GetComponent<CarMover>().ResetWaypointTarget();
        yield return StartCoroutine(GenerateLureSpots());


        //pick random start position
        yield return StartCoroutine(videoLayerManager.MoveToRandomPoint());
        //sort retrieval frames based on new starting position
        yield return StartCoroutine("SortRetrievalFrames");

        //pick next frame
        yield return StartCoroutine(UpdateNextSpawnFrame());
        
        UnityEngine.Debug.Log("starting verbal retrieval");

        if (isPractice)
        {

            trialLogTrack.LogInstructions(true);
            yield return StartCoroutine(uiController.SetActiveInstructionPage("Verbal"));

            //wait until the instructions sequence is complete
            while (uiController.showInstructions)
            {
                yield return 0;
            }

            trialLogTrack.LogInstructions(false);
        }

        //trafficLightController.MakeVisible(true);
        //yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        //SetCarMovement(true);


        yield return StartCoroutine(videoLayerManager.ResumePlayback());
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        //trafficLightController.MakeVisible(false);

        while (retCount < testLength)
        {
            yield return 0;
        }
        //verbalRetrieval = false;
        //SetCarMovement(false);

        trafficLightController.MakeVisible(false);
        yield return new WaitForSeconds(1f);
        yield return null;
    }

    IEnumerator RunSpatialRetrieval()
    {
        //SetCarMovement(false);
        player.GetComponent<CarMover>().ToggleSpatialRetrievalIndicator(true);
        yield return StartCoroutine(GenerateLureSpots()); //create lures
        //trafficLightController.MakeVisible(false);
        //carSpeed = 0f;
        spatialFeedbackStatus.Clear();
        spatialFeedbackStatus = new List<bool>();
        UnityEngine.Debug.Log("beginning spatial retrieval");

      
        //disable collision of item collider box during spatial retrieval

        //for (int k = 0; k < spawnedObjects.Count; k++)
        //{
        //    if (spawnedObjects[k] != null)
        //        spawnedObjects[k].GetComponent<StimulusObject>().ToggleCollisions(false);
        //}

      

        //show instructions only during the practice
        if (isPractice)
        {
            trialLogTrack.LogInstructions(true);
            UnityEngine.Debug.Log("setting instructions");
            yield return StartCoroutine(uiController.SetActiveInstructionPage("Spatial"));

            //wait until the instructions sequence is complete
            while(uiController.showInstructions)
            {
                yield return 0;
            }
            //   yield return StartCoroutine(ShowRetrievalInstructions());
            trialLogTrack.LogInstructions(false);
        }

        //chequeredFlag.SetActive(false);


        //trafficLightController.MakeVisible(true);
        //yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        //SetCarMovement(false);

        uiController.itemRetrievalInstructionPanel.alpha = 0f;
        //trafficLightController.MakeVisible(false);

        //mix spawned objects and lures into a combined list that will be used to test for this retrieval condition

        List<GameObject> spatialTestList = new List<GameObject>();

        for(int k=0;k<spawnedObjects.Count;k++)
        {
            spatialTestList.Add(spawnedObjects[k]);
        }
        for(int l = 0;l<lureObjects.Count;l++)
        {

            spatialTestList.Add(lureObjects[l]);
        }

        UnityEngine.Debug.Log("spatial test list has " + spatialTestList.Count.ToString() + " items in it");

        //shuffle the list
        var rand = new System.Random();
        var randomList = spatialTestList.OrderBy(x => rand.Next()).ToList();


        for (int j = 0; j < testLength; j++)
        {
            UnityEngine.Debug.Log("retrieval num " + j.ToString());
            //targetNames = spawnedObjects[randIndex[j]].gameObject.name.Split('(')[0];
            //uiController.zRetrievalText.color = Color.white;
            //uiController.zRetrievalText.text = targetNames;
            //yield return StartCoroutine(ShowItemCuedReactivation(spawnedObjects[randIndex[j]].gameObject));

            yield return StartCoroutine(ShowItemCuedReactivation(spatialTestList[j].gameObject));
            //SetCarMovement(true);

            yield return StartCoroutine(videoLayerManager.ResumePlayback());
            player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Manual);
            //  uiController.targetTextPanel.alpha = 1f;

            //wait for the player to press X to choose their location OR skip it if the player retrieved the object as "New"
            while (!Input.GetKeyDown(KeyCode.X) && !retrievedAsNew)
            {
                yield return 0;
            }
            retrievedAsNew = false; //reset this flag
            yield return StartCoroutine(videoLayerManager.PauseAllLayers());
            //stop car and calculate then proceed to next
            //SetCarMovement(false);


            float dist = Vector3.Distance(spatialTestList[j].transform.position, player.transform.position);
            UnityEngine.Debug.Log("spatial feedback dist for  " + spatialTestList[j].gameObject.name + " is  " + dist.ToString());
            if (dist < 15f)
            {
                spatialFeedbackStatus.Add(true);
            }
            else
            {
                spatialFeedbackStatus.Add(false);
            }
            spatialFeedbackPosition.Add(player.transform.position);
            trialLogTrack.LogRetrievalAttempt(spatialTestList[j].gameObject, player);

            yield return new WaitForSeconds(0.2f);

        }
        SetCarMovement(false);
        player.GetComponent<CarMover>().ToggleSpatialRetrievalIndicator(false);
        uiController.itemRetrievalInstructionPanel.alpha = 0f;
        yield return null;
    }


    IEnumerator RunWeatherFamiliarization()
    {
        UnityEngine.Debug.Log("running weather familiarization");
        for(int i=0;i<3;i++)
        {
            switch(i)
            {
                case 0:
                    currentWeather = new Weather(Weather.WeatherType.Sunny);
                    break;
                case 1:
                    currentWeather = new Weather(Weather.WeatherType.Rainy);
                    break;
                case 2:
                    currentWeather = new Weather(Weather.WeatherType.Night);
                    break;
            }

            ChangeLighting(currentWeather);
            UnityEngine.Debug.Log("changed weather to " +  currentWeather.weatherMode.ToString());
            yield return StartCoroutine("RunEncoding");
            ToggleFixation(true);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine("ResetTrack");
            ToggleFixation(false);
        }
        yield return null;
    }

    IEnumerator RunBlockTests()
    {

        yield return StartCoroutine(GenerateBlockTestPairs());
        yield return StartCoroutine(GenerateContextRecollectionList()); //generate list from the remaining indices in the stimuliBlockSequence


        //perform each of those tests for the paired list in sequence
        for (int i = 0; i < blockTestPairList.Count; i++)
        {
            yield return StartCoroutine(RunTemporalOrderTest());
            yield return StartCoroutine(RunTemporalDistanceTest());
        }

        //this will be run on a randomized set of items that weren't included in the tests above
        yield return StartCoroutine(RunContextRecollectionTest());
        yield return null;
    }

    //TODO: make this rule-based and not hard-coded
    IEnumerator GenerateBlockTestPairs()
    {
        //add 2 pairs encountered in different loops, different weather; see the design document for more information
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[2], stimuliBlockSequence[5])); //loop 1 and 2
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[12], stimuliBlockSequence[15]));  // loop 3 and 4

        //add 2 pairs encountered in the same loop
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[16], stimuliBlockSequence[19])); //loop 4
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[0], stimuliBlockSequence[3])); //loop 1

        //add 2 pairs encountered in the different loop, same weather
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[8], stimuliBlockSequence[11])); // loop 2 and 3
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[7], stimuliBlockSequence[10]));

        yield return null;
    }


    //TODO: make this rule-based and not hard-coded
    IEnumerator GenerateContextRecollectionList()
    {

        //different weather
        contextDifferentWeatherTestList = new List<GameObject>(); //2,5,14,15
        contextDifferentWeatherTestList.Add(stimuliBlockSequence[1]);
        contextDifferentWeatherTestList.Add(stimuliBlockSequence[4]);
        contextDifferentWeatherTestList.Add(stimuliBlockSequence[13]);
        contextDifferentWeatherTestList.Add(stimuliBlockSequence[14]);



        //same weather
        contextSameWeatherTestList = new List<GameObject>(); //7,10,18,19
        contextSameWeatherTestList.Add(stimuliBlockSequence[6]);
        contextSameWeatherTestList.Add(stimuliBlockSequence[9]);
        contextSameWeatherTestList.Add(stimuliBlockSequence[17]);
        contextSameWeatherTestList.Add(stimuliBlockSequence[18]);

        yield return null;
    }

    IEnumerator RunTemporalOrderTest()
    {

        uiController.temporalOrderTestPanel.alpha = 1f;
        uiController.ToggleSelection(true);
        //wait for the options to be selected
        canSelect = true;
        yield return StartCoroutine(WaitForSelection());
        canSelect = false;
        uiController.ToggleSelection(false);
        uiController.temporalOrderTestPanel.alpha = 0f;
        yield return null;
    }

    IEnumerator RunTemporalDistanceTest()
    {

        uiController.temporalDistanceTestPanel.alpha = 1f;

        //wait for the selection of options
        uiController.ToggleSelection(true);
        canSelect = true;
        yield return StartCoroutine(WaitForSelection());
        canSelect = false;
        uiController.ToggleSelection(false);

        uiController.temporalDistanceTestPanel.alpha = 0f;
        yield return null;
    }

    IEnumerator RunContextRecollectionTest()
    {
        uiController.contextRecollectionTestPanel.alpha = 1f;

        List<int> randOrder = new List<int>();

        yield return StartCoroutine(WaitForActionButton());
        uiController.contextRecollectionTestPanel.alpha = 0f;
        yield return null;
    }


 
    IEnumerator PerformSpatialFeedback()
    {
        List<GameObject> indicatorsList = new List<GameObject>();
        overheadCam.SetActive(true);
        feedbackQuad.SetActive(true);


        for (int k = 0; k < spawnedObjects.Count; k++)
        {
            spawnedObjects[k].GetComponent<VisibilityToggler>().TurnVisible(true);
            //spawnedObjects[k].GetComponent<FacePosition>().ShouldFacePlayer = false;
            //spawnedObjects[k].GetComponent<FacePosition>().TargetPositionTransform = overheadCam.transform;
            //spawnedObjects[k].GetComponent<FacePosition>().ShouldFaceOverheadCam = true;

            int childCount = spawnedObjects[k].transform.childCount;
            spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().ShouldFacePlayer = false;
            spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().TargetPositionTransform = overheadCam.transform;
            spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().ShouldFaceOverheadCam = true;

            spawnedObjects[k].transform.GetChild(childCount - 1).localScale *= 10f;
            GameObject prefabToSpawn = null;
            if (spatialFeedbackStatus[k])
            {
                prefabToSpawn = correctIndicator;
            }
            else
            {
                prefabToSpawn = wrongIndicator;
            }
            GameObject indicatorObj = Instantiate(prefabToSpawn, spatialFeedbackPosition[k], Quaternion.identity) as GameObject;
            indicatorsList.Add(indicatorObj);

            yield return new WaitForSeconds(1f);

        }

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return 0;
        }

        for (int i = 0; i < indicatorsList.Count; i++)
        {
            Destroy(indicatorsList[i]);
        }
        indicatorsList.Clear();

        overheadCam.SetActive(false);
        feedbackQuad.SetActive(false);
        yield return null;

    }

    IEnumerator WaitForSelection()
    {
        while (!Input.GetKeyDown(KeyCode.X))
        {
            yield return null;
        }
        yield return null;
    }

    IEnumerator CleanBlockSequence()
    {

        yield return null;
    }
    public Transform GetTransformForFrame(int frameNum)
    {
        Transform resultTrans=null;
        if (frameNum < playerPosDict.Count)
        {
            if(playerPosDict.TryGetValue(frameNum,out resultTrans))
            {
                return resultTrans;
            }

        }
        return resultTrans;
    }

    public IEnumerator ShowItemCuedReactivation(GameObject stimObject)
    {
        uiController.ResetRetrievalInstructions();
        uiController.itemReactivationPanel.alpha = 1f;
        uiController.itemReactivationText.text = stimObject.GetComponent<StimulusObject>().GetObjectName();
        yield return StartCoroutine(uiController.SetupSelectionOptions("Item"));
        yield return new WaitForSeconds(Configuration.itemReactivationTime);
        uiController.itemReactivationDetails.alpha = 1f;
        uiController.ToggleSelection(true);
        canSelect = true;
        yield return StartCoroutine(WaitForSelection());

        uiController.itemReactivationDetails.alpha = 0f;
        uiController.itemReactivationPanel.alpha = 0f;
        canSelect = false;
        uiController.ToggleSelection(false);

        //the option index 2 will correspond to "New", if that is selected, we skip the retrieval part
        if (uiController.GetSelectionIndex() == 2)
        {
            //do nothing
            retrievedAsNew = true; // set this as retrieved as new

            UnityEngine.Debug.Log("retrieved as new");
        }
        else
        {
        yield return StartCoroutine(uiController.SetItemRetrievalInstructions(stimObject.GetComponent<StimulusObject>().GetObjectName()));
        }
        yield return null;
    }


    public IEnumerator ShowLocationCuedReactivation(GameObject stimObject)
    {
        //SetCarMovement(false);
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        uiController.ResetRetrievalInstructions();
        uiController.locationReactivationPanel.alpha = 1f;
        yield return StartCoroutine(uiController.SetupSelectionOptions("Location"));
        yield return new WaitForSeconds(Configuration.locationReactivationTime);

        uiController.ToggleSelection(true);
        canSelect = true;
        yield return StartCoroutine(WaitForSelection());
        uiController.locationReactivationPanel.alpha = 0f;
        uiController.ToggleSelection(false);
        canSelect = false;

        //the option index 1 will correspond to "No", if that is selected, we skip the retrieval part
        if (uiController.GetSelectionIndex() == 1)
        {
            //do nothing
        }
        else
        {
            yield return StartCoroutine(uiController.SetLocationRetrievalInstructions());

            float randJitterTime = UnityEngine.Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);
            yield return new WaitForSeconds(randJitterTime);
            uiController.microphoneIconImage.color = Color.green;
            UnityEngine.Debug.Log("begin verbal recall");
            yield return StartCoroutine(StartVerbalRetrieval(stimObject));
        }
        uiController.ResetRetrievalInstructions();

        retCount++;
        UnityEngine.Debug.Log("finished verbal recall");
        SetCarMovement(true);
        yield return StartCoroutine(videoLayerManager.ResumePlayback());
        yield return null;
    }




    public IEnumerator StartVerbalRetrieval(GameObject objectQueried)
    {
        yield return new WaitForSeconds(1f);
        //uiController.verbalInstruction.alpha = 1f;
        string fileName = trialCount.ToString() + "_" + retCount.ToString();
        audioRecorder.beepHigh.Play();


        //start recording
        yield return StartCoroutine(audioRecorder.Record(sessionDirectory + "audio", fileName, recallTime));
        trialLogTrack.LogVerbalRetrievalAttempt(objectQueried, fileName);
        //play off beep
        audioRecorder.beepLow.Play();

        retCount++;
       // uiController.verbalInstruction.alpha = 0f;
        SetCarMovement(false);
        yield return null;
    }

    List<Transform> GetValidStartTransforms()
    {
        List<Transform> result = new List<Transform>();
        List<int> intResult = new List<int>();
        for (int k = 0; k < startableTransforms.Count; k++)
        {
            result.Add(startableTransforms[k]);
            intResult.Add(k);
        }

        for (int i = 0; i < listLength; i++)
        {
            

            UnityEngine.Debug.Log("for item " + i.ToString());
            for (int j = 0; j < startableTransforms.Count; j++)
            {

                float dist = Vector3.Distance(startableTransforms[j].position, spawnedObjects[i].transform.position);
              
                if (dist < 10f)
                {
                    UnityEngine.Debug.Log("distance " + dist.ToString());
                    UnityEngine.Debug.Log("excluding  " + j.ToString());
                    int indexToRemove = UsefulFunctions.FindIndexOfInt(intResult,j);
                    result.RemoveAt(indexToRemove);
                    intResult.RemoveAt(indexToRemove);
                    j = startableTransforms.Count; //break out of this loop and move onto the next spawned object; we'll exclude just one transform per item
                }


            }
        }


        return result;
    }

    public void ShowPathDirection()
    {
        switch (player.GetComponent<WaypointProgressTracker>().currentDirection)
        {
            case WaypointProgressTracker.TrackDirection.Left:
                break;
            case WaypointProgressTracker.TrackDirection.Right:
                break;

        }

    }

   
    IEnumerator SortRetrievalFrames()
    {
        int startFrame = videoLayerManager.GetMainLayerCurrentFrameNumber();
        List<int> temp = new List<int>();
        temp = DuplicateList(sortedRetrievalFrames);
      
        List<int> result = new List<int>();
        int insertIndex = -100;
        int minDiff = 10000;
        for(int i=0;i<sortedRetrievalFrames.Count;i++)
        {
            int currDiff = sortedRetrievalFrames[i] - startFrame;
            UnityEngine.Debug.Log("curr diff between " + sortedRetrievalFrames[i].ToString() + " and " + startFrame.ToString() + " = " + currDiff.ToString());
            if (currDiff > 0 && currDiff < minDiff)
            {
                minDiff = currDiff;

                UnityEngine.Debug.Log("min diff " + minDiff.ToString());
                insertIndex = i;
                UnityEngine.Debug.Log("new insert index " + insertIndex.ToString());
            }
        }
        UnityEngine.Debug.Log("insert index " + insertIndex.ToString());

        int reverseIndex = 0;
        //if insert index is -1, then the order will remain same; else change it accordingly
        if (insertIndex >=0)
        {
            for (int i = 0; i < sortedRetrievalFrames.Count; i++)
            {
                if (insertIndex + i < sortedRetrievalFrames.Count)
                {
                    result.Add(sortedRetrievalFrames[insertIndex + i]);
                    UnityEngine.Debug.Log("adding " + sortedRetrievalFrames[insertIndex + i] + " at " + (insertIndex + i).ToString());
                }
                else
                {
                    UnityEngine.Debug.Log("adding " + sortedRetrievalFrames[reverseIndex] + " at " + reverseIndex.ToString());
                    result.Add(sortedRetrievalFrames[reverseIndex]);
                    reverseIndex++;
                }
            }

            //clear the sortedretrievalframes and then update it
            sortedRetrievalFrames.Clear();
            for(int i=0;i<result.Count;i++)
            {
                sortedRetrievalFrames.Add(result[i]);
            }
        }

        yield return null;
    }

     public IEnumerator UpdateNextSpawnFrame()
    {
        if (currentStage == TaskStage.Encoding)
        {
            if (sortedSpawnFrames.Count > 0)
            {
                nextSpawnFrame = sortedSpawnFrames[0];
                UnityEngine.Debug.Log("next spawn frame " + nextSpawnFrame.ToString());
                sortedSpawnFrames.RemoveAt(0);
            }
            else
            {
                nextSpawnFrame = -10000;   
            }
        }

        //this includes both stim items and lures
        else
        {
            if (sortedRetrievalFrames.Count > 0)
            {
                nextSpawnFrame = sortedRetrievalFrames[0];
                UnityEngine.Debug.Log("next spawn frame " + nextSpawnFrame.ToString());
                sortedRetrievalFrames.RemoveAt(0);
            }
            else
            {

                nextSpawnFrame = -10000;
            }
        }


        yield return null;
    }

    void ToggleFixation(bool shouldShow)
    {
        
        trialLogTrack.LogFixation(shouldShow);
        uiController.fixationPanel.alpha = (shouldShow) ? 1f:0f;
        uiController.fixationCross.alpha =  (shouldShow)?1f:0f;
    }


    IEnumerator ShowFixation()
    {
        UnityEngine.Debug.Log("inside fixation");
        trialLogTrack.LogFixation(true);
        uiController.fixationPanel.alpha = 1f;
        uiController.fixationCross.alpha = 1f;
        float totalFixationTime = fixedTime + UnityEngine.Random.Range(0.1f, 0.3f);
        uiController.fixationCross.alpha = 0f;
        yield return new WaitForSeconds(totalFixationTime);
        UnityEngine.Debug.Log("finished waiting for fixation");
        uiController.fixationPanel.alpha = 0f;

        trialLogTrack.LogFixation(false);
        yield return null;
    }

    IEnumerator SpawnUniformly(int spawnCount)
    {

        List<int> intPicker = new List<int>();
        for (int i = 0; i < 4; i++)
        {

        }

        yield return null;
    }

    //GETS CALLED FROM DEFAULTITEM.CS WHEN CHEST OPENS ON COLLISION WITH PLAYER.
    public IEnumerator WaitForTreasurePause(GameObject specialObject)
    {

        //lock the avatar controls
      //  player.controls.ShouldLockControls = true;
       // player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        yield return new WaitForSeconds(Configuration.itemPresentationTime);

        //unlock the avatar controls
        //player.controls.ShouldLockControls = false;

        //turn the special object invisible
        if (specialObject != null)
        {
            specialObject.GetComponent<SpawnableImage>().TurnVisible(false);
        }


        //only after the pause should we increment the number of coins collected...
        //...because the trial controller waits for this to move on to the next part of the trial.
        Debug.Log("INCREMENT CHEST COUNT");
    //    IncrementNumDefaultObjectsCollected();
        yield return null;
    }

    int FindNearestIndex(List<int> collection, int currentInt)
    {
        int minDiff = 100;
        int nearest = 100;
        if (collection.Count == 0)
            return currentInt;

        for (int i=0;i<collection.Count;i++)
        {
            int currDiff = Mathf.Abs(collection[i] - currentInt);
            if(currDiff < minDiff)
            {
                minDiff = currDiff;
                nearest = collection[i];
            }
        }
        return nearest;
    }

    bool DetermineWaypointRemoval(int indexForRemoval, List<int> collection)
    {
        if(indexForRemoval > 0 && indexForRemoval < collection.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    IEnumerator PickEncodingLocations()
    {
        List<int> intPicker = new List<int>();
        List<int> waypointFrames = new List<int>();
        List<int> chosenEncodingFrames = new List<int>();



        //we keep last three and first three seconds as buffer
        for (int i = Configuration.startBuffer; i < currentMaxFrames - Configuration.endBuffer; i++)
        {
            intPicker.Add(i);
            waypointFrames.Add(i);
        }

        List<int> tempStorage = new List<int>();
        

        //we pick locations for encoding objects AND lure
        for (int i = 0; i < listLength; i++)
        {
            int randIndex = UnityEngine.Random.Range(0, intPicker.Count); // we won't be picking too close to beginning/end
            int randInt = intPicker[randIndex];
            int nearestIndex = 0;
            UnityEngine.Debug.Log("picked " + intPicker[randIndex].ToString());
            

            chosenEncodingFrames.Add(randInt);
            int minLimit = Mathf.Clamp(randIndex - Configuration.minFramesBetweenStimuli, 0, intPicker.Count - 1);
            int maxLimit = Mathf.Clamp(randIndex + Configuration.minFramesBetweenStimuli, 0, intPicker.Count - 1);

            //UnityEngine.Debug.Log("between " + intPicker[minLimit].ToString() + "  and  " + intPicker[maxLimit].ToString());
            //UnityEngine.Debug.Log("intpicker length " + intPicker.Count.ToString());
            //UnityEngine.Debug.Log("min " + minLimit.ToString() + " max " + maxLimit.ToString());
            int ind = minLimit;
            for (int j=minLimit; j<maxLimit;j++)
            {
                
                    //UnityEngine.Debug.Log("comparing " + intPicker[ind].ToString() + " with " + randInt.ToString());
                if (Mathf.Abs(intPicker[ind] - randInt) < Configuration.minFramesBetweenStimuli)
                    {
                        //UnityEngine.Debug.Log("removing " + intPicker[ind].ToString());
                        intPicker.RemoveAt(ind);
                    }
                else
                {
                    //UnityEngine.Debug.Log("incrementing ind");
                    ind++;
                }
                
            }
            if (i < listLength)
            {
                //UnityEngine.Debug.Log("picking object at  " + randInt.ToString());
                tempStorage.Add(randInt);
                spawnFrames.Add(chosenEncodingFrames[i]);
                //UnityEngine.Debug.Log("adding to spawn frames: " + chosenEncodingFrames[i].ToString());
            }

        }

        intPicker.Clear();
        waypointFrames.Clear();

        //refresh the lists; remove points with existing stim items associated with them; lures are not constrained to be at a min distance from nearest object
        for (int i = Configuration.startBuffer; i < currentMaxFrames - Configuration.endBuffer; i++)
        {
            //we will only add to this list if it doesn't have an existing item on it
            if (!CheckIndexHits(tempStorage, i))
            {
                intPicker.Add(i);
                waypointFrames.Add(i);
            }
        }

        //2 lures per trial
        for (int j = 0; j < Configuration.luresPerTrial; j++)
        {
            int randIndex = UnityEngine.Random.Range(0, intPicker.Count);
            //UnityEngine.Debug.Log("picking at " + randIndex.ToString() + " while intpicker count is: " + intPicker.Count.ToString());
            int randInt = intPicker[randIndex];
            intPicker.RemoveAt(randIndex);
            UnityEngine.Debug.Log("lure picked at " + randInt.ToString());
            lureFrames.Add(randInt);
        }
        //UnityEngine.Debug.Log("first index of lure frames " + lureFrames[0]);
        List<int> sortedLureFrames = new List<int>();
        sortedLureFrames = DuplicateList(lureFrames);
        //UnityEngine.Debug.Log("first index of duplicated lure frames " + sortedLureFrames[0]);
        sortedLureFrames = SortListInAscending(sortedLureFrames);

        //UnityEngine.Debug.Log("first index of encoding frames " + chosenEncodingFrames[0]);
        List<int> sortedWaypointFrames = new List<int>();
        sortedWaypointFrames = DuplicateList(chosenEncodingFrames);

        
        //UnityEngine.Debug.Log("first index of duplicated waypoint frames " + sortedWaypointFrames[0]);
        //sortedWaypointFrames.Sort();
        sortedWaypointFrames = SortListInAscending(sortedWaypointFrames);
        //UnityEngine.Debug.Log("first index of sorted duplicated lure frames " + sortedWaypointFrames[0]);

        sortedSpawnFrames = new List<int>();
        sortedRetrievalFrames = new List<int>();

        List<int> tempWaypointFrames = new List<int>();
        tempWaypointFrames = DuplicateList(sortedWaypointFrames);

        for (int i = 0; i < listLength; i++)
        {
            
                    //UnityEngine.Debug.Log("added " + sortedWaypointFrames[0].ToString() + " to sorted spawn frame");
                    sortedSpawnFrames.Add(sortedWaypointFrames[0]);
                    sortedWaypointFrames.RemoveAt(0);
            
        }

        //since it's sorted, we only concern ourself with the first index
        for (int i=0;i<listLength + Configuration.luresPerTrial ;i++)
        {
            if (sortedLureFrames.Count !=0 && tempWaypointFrames.Count != 0)
            {
                if (sortedLureFrames[0] < tempWaypointFrames[0])
                {
                    //UnityEngine.Debug.Log("added " + sortedLureFrames[0].ToString() + " to sorted lure frame");
                    sortedRetrievalFrames.Add(sortedLureFrames[0]);
                    sortedLureFrames.RemoveAt(0);
                }
                else
                {
                    //UnityEngine.Debug.Log("added " + sortedWaypointFrames[0].ToString() + " to sorted lure frame");
                    sortedRetrievalFrames.Add(tempWaypointFrames[0]);
                    tempWaypointFrames.RemoveAt(0);
                }
            }
            else
            {
                if (sortedLureFrames.Count == 0)
                {
                    sortedRetrievalFrames.Add(tempWaypointFrames[0]);
                    tempWaypointFrames.RemoveAt(0);
                }
                else
                {
                    sortedRetrievalFrames.Add(sortedLureFrames[0]);
                    sortedLureFrames.RemoveAt(0);
                }
            }
        }

        UnityEngine.Debug.Log("finished picking");

        for(int i=0;i<sortedSpawnFrames.Count;i++)
        {
            UnityEngine.Debug.Log("spawn order " + i.ToString() + ": " + sortedSpawnFrames[i].ToString());
        }

        retrievalFrameObjectDict = new Dictionary<int, GameObject>();
        yield return null;
    }

    //check to see if the target int exists in an integer list
    public bool CheckIndexHits(List<int> mainList, int targetInt)
    {
        for(int i=0;i<mainList.Count;i++)
        {
            if (targetInt == mainList[i])
                return true;
        }
        return false;
    }

    public List<int> SortListInAscending(List<int> targetList)
    {

        targetList = targetList.OrderBy(p => p).ToList();
        return targetList;
    }

    //used to present items
    public IEnumerator PresentStimuli(GameObject stimulusObject)
    {
        //stop the car first
        player.GetComponent<CarMover>().ToggleCarMovement(false);
        //SetCarMovement(false);


        //wait until the car has stopped
        //while (player.GetComponent<CarMover>().CheckIfMoving())
        //{
        //    UnityEngine.Debug.Log("waiting for the car to stop moving");
        //    yield return 0;
        //}

        //    string objectName = stimulusObject.GetComponent<StimulusObject>().GetObjectName();

        //move to stimuli presentation transform

        //  UnityEngine.Debug.Log("moving the item to presentation transform");
           //stimulusObject.transform.position = player.GetComponent<CarMover>().presentationTransform.position;
           //stimulusObject.transform.rotation = player.GetComponent<CarMover>().presentationTransform.rotation;

       // stimulusObject.GetComponent<VisibilityToggler>().TurnVisible(true);


        float randJitterTime = Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);
        float totalPresentationTime = Configuration.itemPresentationTime + randJitterTime;

        ////make object visible
        //if (stimulusObject.GetComponent<VisibilityToggler>() != null)
        //    stimulusObject.GetComponent<VisibilityToggler>().TurnVisible(true);

        //yield return StartCoroutine(SpawnSpecialObject(stimulusObject, stimulusObject.GetComponent<StimulusObject>().specialObjectSpawnPoint.position));
        //UnityEngine.Debug.Log("finished running collision");

        string objectName = objController.ReturnStimuliDisplayText();
       // uiController.presentationItemText.enabled = true;
     //   uiController.presentationItemText.text = objectName;
        trialLogTrack.LogItemPresentation(objectName, true);

        //wait for the calculated presentation time
        // yield return new WaitForSeconds(totalPresentationTime);

        //hide it after
        if (stimulusObject.GetComponent<VisibilityToggler>() != null)
            stimulusObject.GetComponent<VisibilityToggler>().TurnVisible(false);

        uiController.presentationItemText.enabled = false;
        trialLogTrack.LogItemPresentation(objectName, false);


        //add the gameobject to the sequence
        //stimuliBlockSequence.Add(stimulusObject);

        //resume movement after resetting UI
        SetCarMovement(true);
        yield return null;
    }


    IEnumerator SpawnSpecialObject(GameObject stimObject, Vector3 specialSpawnPos)
    {



        GameObject specialObject = Instantiate(Experiment.Instance.imagePlanePrefab, specialSpawnPos, Quaternion.identity) as GameObject;



        //GameObject specialObject = Experiment.Instance.SpawnSpecialObject(specialSpawnPos);
        Texture stimImage = Experiment.Instance.objController.ReturnStimuliToPresent();
        string stimDisplayText = Experiment.Instance.objController.ReturnStimuliDisplayText();
        stimObject.GetComponent<StimulusObject>().stimuliDisplayName = stimDisplayText;
        specialObject.GetComponent<SpawnableImage>().SetImage(stimImage);

        //attach image plane to treasure chest's parent which is ItemColliderBox
        specialObject.transform.parent = stimObject.transform;
        UnityEngine.Debug.Log("set " + specialObject.gameObject.name + " parented to " + stimObject.gameObject.name);

        //reset rotation to match the parent's rotation
        specialObject.transform.localRotation = Quaternion.identity;

        //	string name = specialObject.GetComponent<SpawnableObject>().GetDisplayName();
        //set special object text
        //stimObject.GetComponent<StimulusObject>().SetSpecialObjectText(stimDisplayText);

        //	Experiment.Instance.trialController.AddNameToList(specialObject, stimDisplayText);

        //stimObject.GetComponent<StimulusObject>().PlayJuice(true);

        //tell the trial controller to wait for the animation
        yield return StartCoroutine(Experiment.Instance.WaitForTreasurePause(specialObject));

        //should destroy the chest after the special object time
        //Destroy(gameObject);
    }
   

    IEnumerator SpawnEncodingObjects()
    {
        UnityEngine.Debug.Log("number of spawn locations " + spawnFrames.Count.ToString());
        for (int i = 0; i < spawnFrames.Count; i++)
        {
            //GameObject encodingObj = Instantiate(objController.encodingList[i], new Vector3(spawnLocations[i].x, spawnLocations[i].y + 1.5f, spawnLocations[i].z), Quaternion.identity) as GameObject;
            //GameObject colliderBox = Instantiate(objController.itemBoxColliderPrefab, new Vector3(spawnLocations[i].x, spawnLocations[i].y +1.5f, spawnLocations[i].z), Quaternion.identity) as GameObject;
            //GameObject encodingObj = colliderBox.GetComponent<CarStopper>().stimulusObject;
            //encodingObj.name = encodingObj.name + "_" + i.ToString();
            //spawnedObjects.Add(encodingObj);


            //trialLogTrack.LogEncodingItemSpawn(encodingObj.name.Split('(')[0], encodingObj.transform.position);

            //encodingObj.GetComponent<FacePosition>().ShouldFacePlayer = true;
            //encodingObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
          
            //encodingObj.GetComponent<VisibilityToggler>().TurnVisible(false);


            //adjust the stimulus object's position so it appears right above the indicator
         //   encodingObj.transform.position = colliderBox.GetComponent<CarStopper>().positionIndicator.transform.position + new Vector3(0f, 1.5f, 0f);

            //parent the collider box with the encoding object
           // colliderBox.transform.parent = encodingObj.transform;

            //colliderBox.transform.localPosition = Vector3.zero;
           // colliderBox.transform.localRotation = Quaternion.identity;

            //associate the stimulus object 
            //encodingObj.GetComponent<StimulusObject>().LinkColliderObj(colliderBox);



        }
        //reset the dictionary
        retrievalFrameObjectDict = new Dictionary<int, GameObject>(); 
        yield return null;
    }



    public IEnumerator WaitForActionButton()
    {
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return 0;
        }
        yield return null;
    }

    public void SetCarMovement(bool shouldMove)
    {

        player.GetComponent<CarMover>().playerRigidbody.isKinematic = !shouldMove;
        trialLogTrack.LogCarMovement(shouldMove);
        player.GetComponent<CarMover>().ToggleCarMovement(shouldMove);
    }

    IEnumerator GenerateRetrievalList()
    {
        List<int> tempList = new List<int>();
        List<int> retrievalSeqList = new List<int>();
        for (int i = 0; i < listLength; i++)
        {
            tempList.Add(i);
        }
        for (int j = 0; j < listLength; j++)
        {
            int randIndex = UnityEngine.Random.Range(0, tempList.Count);
            retrievalSeqList.Add(tempList[randIndex]);
            tempList.RemoveAt(randIndex);
        }

        retrievalObjList = new List<GameObject>();
        retrievalPositions = new List<Vector3>();

        //finally add them to the retrieval lists
        for (int i = 0; i < retrievalSeqList.Count; i++)
        {
            UnityEngine.Debug.Log("adding " + spawnedObjects[retrievalSeqList[i]]);
            UnityEngine.Debug.Log("adding " + spawnFrames[retrievalSeqList[i]]);
            retrievalObjList.Add(spawnedObjects[retrievalSeqList[i]]);
            retrievalFrames.Add(spawnFrames[retrievalSeqList[i]]);
        }

        yield return null;
    }

    public List<int> DuplicateList(List<int> targetList)
    {
        List<int> resultList = new List<int>();
        for(int i=0;i<targetList.Count;i++)
        {
            resultList.Add(targetList[i]);
        }
        return resultList;
    }

    public IEnumerator SpawnObjects()
    {
        while (LapCounter.lapCount < maxLaps)
        {
            //retrieval lap
            if (LapCounter.isRetrieval)
            {
                SetCarMovement(true);
                List<Vector3> chosenLocations = new List<Vector3>();
                yield return StartCoroutine(GenerateRetrievalList());
                for (int i = 0; i < totalTrials; i++)
                {
                    UnityEngine.Debug.Log("retrieval loop " + i.ToString());
                    LapCounter.finishedLap = false;
                    SetCarMovement(true);
                    UnityEngine.Debug.Log("retrieval obj list outside " + retrievalObjList.Count.ToString());
                    UnityEngine.Debug.Log(" what is " + retrievalObjList[i].ToString());
                    string objName = retrievalObjList[i].gameObject.name.Split('(')[0];
                    uiController.retrievalItemName.text = objName;
                  //  uiController.retrievalTextPanel.alpha = 1f;
                    while (!Input.GetKeyDown(KeyCode.Space))
                    {
                        yield return 0;
                    }
               //     uiController.retrievalTextPanel.alpha = 0f;
                    uiController.targetTextPanel.alpha = 1f;
                    uiController.retrievalItemName.text = objName;
                    SetCarMovement(false);
                    yield return new WaitForSeconds(1f);
                    while (!Input.GetKeyDown(KeyCode.Space) && !LapCounter.finishedLap)
                    {
                        yield return 0;
                    }
                    chosenLocations.Add(player.transform.position);
                    float difference = Vector3.Distance(retrievalPositions[i], player.transform.position);
                    UnityEngine.Debug.Log("difference: " + difference.ToString());

                    while (!LapCounter.finishedLap)
                    {
                        yield return 0;
                    }
                    UnityEngine.Debug.Log("finished waiting for lap in retrieval");
                    LapCounter.finishedLap = false;


                }
                LapCounter.isRetrieval = false;
                UnityEngine.Debug.Log("retrieval mode off");
                uiController.targetTextPanel.alpha = 0f;
               // uiController.retrievalTextPanel.alpha = 0f;
                SetCarMovement(false);

                //reset retrieval lists
                spawnedObjects.Clear();
                spawnFrames.Clear();
                yield return 0;
            }
            else
            {
                float randTimeSpawn = UnityEngine.Random.Range(5f, 45f);
                UnityEngine.Debug.Log("random spawn timer " + randTimeSpawn.ToString());
                float timer = 0f;
                while (timer < randTimeSpawn)
                {
                    timer += Time.deltaTime;
                    //	UnityEngine.Debug.Log(timer.ToString() + "/" + randTimeSpawn.ToString());
                    yield return 0;
                }
                //UnityEngine.Debug.Log("initiating spawn sequence");
                //yield return StartCoroutine(objController.InitiateSpawnSequence());

                UnityEngine.Debug.Log("objlapper " + objLapper.ToString() + " lapcount " + LapCounter.lapCount.ToString());
                while (objLapper == LapCounter.lapCount)
                {
                    yield return 0;
                }
                UnityEngine.Debug.Log("objlapper incrementing");
                objLapper++;
            }
            yield return 0;
        }

        yield return null;
    }

   

    IEnumerator MoveForward()
    {
        if (player.GetComponent<CarAIControl>().isReverse)
        {
            SetCarMovement(true);

            player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);

            player.GetComponent<CarAIControl>().ForwardMovement();
            trialLogTrack.LogReverseMovement(false);
            trialLogTrack.LogForwardMovement(true);
            yield return new WaitForSeconds(0.25f);
            SetCarMovement(false);
        }
        yield return null;
    }

    IEnumerator MoveReverse()
    {
        if (!player.GetComponent<CarAIControl>().isReverse)
        {
            SetCarMovement(true);


            player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Reverse);
            player.GetComponent<CarAIControl>().ReverseMovement();
            trialLogTrack.LogForwardMovement(false);
            trialLogTrack.LogReverseMovement(true); 
            yield return new WaitForSeconds(0.25f);
            SetCarMovement(false);
        }
        yield return null;
    }


    // Update is called once per frame
    void Update()
    {



        //UnityEngine.Debug.Log("player current  speed " + player.GetComponent<CarController>().CurrentSpeed.ToString());
        //UnityEngine.Debug.Log("car speed " + carSpeed.ToString());
        if (currentStage == TaskStage.SpatialRetrieval)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
                /*
                SetCarMovement(false);
                if (wasMovingReverse)
                {
                    carSpeed = 0f;
                    movementTimer = 0f;
                    wasMovingReverse = false;
                }
                wasMovingForward = true;
                //UnityEngine.Debug.Log("moving forward");
                StopCoroutine("MoveReverse");
                StartCoroutine("MoveForward");
                carSpeed += 0.5f;
                if (carSpeed > 40f)
                {
                    //UnityEngine.Debug.Log("exceeded max speed");
                    carSpeed = player.GetComponent<CarController>().MaxSpeed;
                }
                //UnityEngine.Debug.Log("increasing speed " + carSpeed.ToString());

    */
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Reverse));
                /*
                SetCarMovement(false);
                if (wasMovingForward)
                {
                    carSpeed = 0f;
                    movementTimer = 0f;
                    wasMovingForward = false;
                }
                wasMovingReverse = true;

                //UnityEngine.Debug.Log("moving reverse");
                StopCoroutine("MoveForward");
                StartCoroutine("MoveReverse");
                carSpeed += 0.5f;
                if (carSpeed > 40f)
                {
                    //UnityEngine.Debug.Log("exceeded max speed");
                    carSpeed = player.GetComponent<CarController>().MaxSpeed;
                }
                //UnityEngine.Debug.Log("increasing speed " + carSpeed.ToString());
                */
            }

        }
        if (uiController.showInstructions)
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                uiController.PerformUIPageChange(UIController.OptionSelection.Left);
            }
            if(Input.GetKeyDown(KeyCode.RightArrow))
            {
                uiController.PerformUIPageChange(UIController.OptionSelection.Right);

            }    
        }

        if (canSelect)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                uiController.PerformSelection(UIController.OptionSelection.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                uiController.PerformSelection(UIController.OptionSelection.Right);

            }

        }

    }
    

	public void OnExit()
	{ //call in scene controller when switching to another scene!
		if (isLogging)
		{
			subjectLog.close();
			eegLog.close();
		}
	}

	void OnApplicationQuit()
	{
		if (isLogging)
		{
			subjectLog.close();
			eegLog.close();
			//File.Copy("/Users/" + System.Environment.UserName + "/Library/Logs/Unity/Player.log", sessionDirectory + "Player.log");
		}
	}
}
