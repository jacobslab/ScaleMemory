using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
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
    public InstructionsManager instructionsManager;

    public GameObject resultObj; //invisible object to keep track of results from GetTransformForFrame

    public AssetBundleLoader assetBundleLoader;
    private bool _expActive = false;

    private bool _firstAudio = true; //a flag to make sure in case of the microphone permission access popup, task can be forced into fullscreen after
#if !UNITY_WEBGL

    public ElememInterface elememInterface;
#endif

    public GameObject player;

    public static bool isPaused = false;

    private List<Trial> _trials;


    public int encodingIndex  = -1;
    public int retrievalIndex = -1;
    //to determine if keypresses can page forward/backwards UI instructions
    private bool _uiActive = false;
    //to determine if keypresses can select onscreen options
    private bool _canSelect = false;

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

    private TrialConditions _trialConditions;

    private List<int> _retrievalTypeList;
    private List<int> _weatherChangeTrials; // this list will maintain the index of trials where encoding and retrieval weather condtions will be distinct
    private List<WeatherPair> _weatherPairs;
    private List<int> _randomizedWeatherOrder; //stores the order which determines weather of trials where weather doesn't change

    private Weather _currentWeather;
    private Weather _encodingWeather;
    private Weather _retrievalWeather;

    public List<GameObject> stimuliBlockSequence; //sequence of encoding stimuli for the current block; utilized in tests at the end of each block
    private List<GameObject> _contextDifferentWeatherTestList;
    private List<GameObject> _contextSameWeatherTestList;

    //private Dictionary<Configuration.WeatherMode, Configuration.WeatherMode> _retrievalWeatherMode;

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
    public bool _skipEncoding = false;
        public bool _skipVerbalRetrieval = false;
        public bool _skipSpatialRetrieval = false;
#else
    private bool _skipEncoding = false;
    private bool _skipVerbalRetrieval = false;
    private bool _skipSpatialRetrieval = false;
#endif

    private float _carSpeed = 0f; //this is used exclusively to control car speed directly during spatial retrieval phase



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

    private int _currBlockNum = 0;

    public List<GameObject> spawnedObjects;
    //public List<Vector3> spawnLocations;
    public List<int> spawnFrames;

    private List<int> _sortedSpawnFrames;
    private List<int> _sorted_retrievalFrames;

    public List<GameObject> lureObjects;
    //public List<Vector3> lureLocations;
    public List<int> lureFrames;

    private bool _showVerbalInstructions = true;
    private bool _showSpatialInstructions = true;

    private List<GameObject> _retrievalObjList;
    private List<Vector3> _retrievalPositions;
    private List<int> _retrievalFrames;

    public List<Transform> startableTransforms;
     
    public WaypointCircuit waypointCircuit;

    public static int recallTime = 6;

    public static int totalTrials = 24;
    private int _blockCount = 0;
    public static int trialsPerBlock = 4;

    public static int listLength = 5;

    private int _testLength = 7;

    public Transform startTransform;

    public bool isLure = false;
    private List<bool> lureBools = new List<bool>();


    private int _currentMaxFrames = 0;


    //elemem variables
    public static string BuildVersion = "0.9.95";
    public static string ExpName = "CityBlock";
#if CLINICAL
    public static bool isElemem = true;
#elif CLINICAL_TEST
public static bool isElemem=false;
#else
    public static bool isElemem = false;
#endif

    public bool verbalRetrieval = false;

    //track screening
    public GameObject overheadCam;
    public GameObject trackFamiliarizationQuad;
    public GameObject feedbackQuad;
    public GameObject playerIndicatorSphere;

    //spatial retrieval
    public GameObject correctIndicator;
    public GameObject wrongIndicator;
    private List<bool> _spatialFeedbackStatus;
    private List<Vector3> _spatialFeedbackPosition;

    //logging
    public static bool isLogging = true;
    private string _subjectLogfile; //gets set based on the current subject in Awake()
    public Logger_Threading subjectLog;
    private string _eegLogfile; //gets set based on the current subject in Awake()
    public Logger_Threading eegLog;
    private string _subjectDirectory;
    public string sessionDirectory;
    public static string sessionStartedFileName = "sessionStarted.txt";
    
    private int _sessionID;

    private string _subjectName = "";

    private bool _canProceed = false;

    public SubjectReaderWriter subjectReaderWriter;

    public List<BlockTestPair> blockTestPairList;


    public List<int> currentstimuliIndices;

    List<int> weatherChangeIndicator;

    private bool _subjectInfoEntered = false;
    private bool _ipAddressEntered = false;

    public IPAddress targetAddress;

    public static string defaultLoggingPath; //SET IN RESETDEFAULTLOGGINGPATH();
                                             //string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
                                             //public Text defaultLoggingPathDisplay;
                                             //public InputField loggingPathInputField;

    public Dictionary<int, Transform> playerPosDict = new Dictionary<int,Transform>();


    public List<Vector3> playerPositions = new List<Vector3>();
    public List<Vector3> playerRotations = new List<Vector3>();

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


    private string _enteredSubjName;

    string prolific_pid = "";
    string study_id = "";
    string session_id = "";
    bool idAssigned = false;
    bool givenConsent = false;

    public SimpleTimer lapTimer;

    public GameObject chequeredFlag;

    private float _fixedTime = 1f;
    private int micStatus = 0;



    private int _maxLaps = 1;


    public TrialLogTrack trialLogTrack;
    private int _objLapper = 0;


    public WebGLMicrophone audioRec;
    public AudioRecorder audioRecorder;
    private int _retCount = 0;
    private int _trialCount = 0;


    public static bool isPractice = false;

    private string _camTransformPath;

    public TextAsset camTransformTextAsset;

    private bool _retrievedAsNew = false;

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
        _carSpeed = 0f;



        //test length is stimuli items + lure items
        _testLength = Configuration.spawnCount + Configuration.luresPerTrial;
        



    }
    // Use this for initialization
    void Start()
    {
        //instantiating lists and variables for use later
        _spatialFeedbackStatus = new List<bool>();
        _spatialFeedbackPosition = new List<Vector3>();
        spawnedObjects = new List<GameObject>();
        spawnFrames = new List<int>();
        lureObjects = new List<GameObject>();
        lureFrames = new List<int>();
        _retrievalObjList = new List<GameObject>();
        _retrievalPositions = new List<Vector3>();
        _retrievalFrames = new List<int>();
        blockTestPairList = new List<BlockTestPair>();

        _blockCount = totalTrials / (trialsPerBlock * Configuration.totalSessions);
        UnityEngine.Debug.Log("block count " + _blockCount.ToString());
        retrievalFrameObjectDict = new Dictionary<int, GameObject>();


        listLength = Configuration.spawnCount;
        _testLength = listLength + Configuration.luresPerTrial;

        /* MAIN EXPERIMENT COROUTINE CALLED HERE*/
        StartCoroutine("BeginExperiment");

        //read the cam transform file
        _camTransformPath = Application.dataPath + "/cam_transform.txt";
        StartCoroutine("ReadCamTransform");




    }

    IEnumerator LoadCamTransform()
    {

        if(camTransformTextAsset!=null)
        {
            yield return StartCoroutine(ParseTextAsset(camTransformTextAsset));
        }

        yield return null;
    }

    public IEnumerator ParseTextAsset(TextAsset targetAsset)
    {
        playerPositions = new List<Vector3>();
        playerRotations = new List<Vector3>();
        string fs = targetAsset.text;
        string[] fLines = fs.Split('\n');

        for (int i = 0; i < fLines.Length; i++)
        {
            string valueLine = fLines[i];
            ParseCamTransformLine(valueLine, i);
        }
            yield return null;
    }

    IEnumerator ReadCamTransform()
    {
        string path = _camTransformPath;

        using (StreamReader sr = new StreamReader(path))
        {
            int index = 0;
            while (sr.Peek() >= 0)
            {
                string currentLine = sr.ReadLine();
                 ParseCamTransformLine(currentLine,index);
                index++;
                    yield return 0;
            }
        }



        yield return null;
    }


     void ParseCamTransformLine(string currentLine, int index)
        {
            string currIndex = currentLine.Split(':')[1];
                string currPos = currIndex.Split('R')[0];
                string currRot = currentLine.Split(':')[2];

                float posX = float.Parse(currPos.Split(',')[0]);
                float posY = float.Parse(currPos.Split(',')[1]);
                float posZ = float.Parse(currPos.Split(',')[2]);


                float rotX = float.Parse(currRot.Split(',')[0]);
                float rotY = float.Parse(currRot.Split(',')[1]);
                float rotZ = float.Parse(currRot.Split(',')[2]);
        


        playerPositions.Add(new Vector3(posX, posY, posZ));
        playerRotations.Add(new Vector3(rotX, rotY, rotZ));


        }

    //this changes the "time of the day" in the scene through lighting
    void ChangeLighting(Weather targetWeather)
    {
        _currentWeather = targetWeather;
        _currentMaxFrames = videoLayerManager.GetTotalFramesOfCurrentClip();

        //unload the current scene first,if one is loaded


        switch (targetWeather.weatherMode)
        {
            case Weather.WeatherType.Sunny:
                UnityEngine.Debug.Log("load sunny");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Sunny);
                break;
            case Weather.WeatherType.Rainy:
                UnityEngine.Debug.Log("load rainy");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Rainy);
                break;
            case Weather.WeatherType.Night:
                UnityEngine.Debug.Log("load night");
                videoLayerManager.UpdateWeather(Weather.WeatherType.Night);
                break;

        }
    }



    public void MarkIPAddrEntered()
    {
        _ipAddressEntered = true;
    }


    public string ReturnSubjectName()
    {
        return _subjectName;
    }

    public string ReturnSessionID()
    {
        return _sessionID.ToString();
    }

    public string ReturnSubjectDirectory()
    {
        return _subjectDirectory;
    }
  
#if !UNITY_WEBGL
    //TODO: move to logger_threading perhaps? *shrug*
    IEnumerator InitLogging()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        string newPath = Path.GetFullPath(Path.Combine(defaultLoggingPath, @"..\"));
#else
        string newPath = Path.GetFullPath(Path.Combine(defaultLoggingPath, @"../../"));
#endif

        _subjectDirectory = newPath + _subjectName + "/";
        sessionDirectory = _subjectDirectory + "session_0" + "/";
        _sessionID = 0;
        string sessionIDString = "_0";

        UnityEngine.Debug.Log("new logging path is " + newPath);
        UnityEngine.Debug.Log("subject directory "+ _subjectDirectory);

            if (!Directory.Exists(_subjectDirectory))
            {
                Directory.CreateDirectory(_subjectDirectory);
            }
            while (File.Exists(sessionDirectory + sessionStartedFileName))
            {
            _sessionID++;

                sessionIDString = "_" + _sessionID.ToString();

                sessionDirectory = _subjectDirectory + "session" + sessionIDString + "/";
            }

#if CLINICAL || CLINICAL_TEST
        //once the current session directory has been created make sure, future sessions directory have also been created
        for(int i=1;i<Configuration.totalSessions;i++)
        {
            string dirPath = Path.Combine(sessionDirectory, _subjectDirectory, "session_" + i.ToString());
            if(!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

        }
#endif

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

            subjectLog.fileName = sessionDirectory + _subjectName + "Log" + ".txt";
            UnityEngine.Debug.Log("_subjectLogfile " + subjectLog.fileName);
            //now you can initiate logging
            yield return StartCoroutine(subjectLog.BeginLogging());
        
   

        yield return null;
    }
#else
        IEnumerator WriteAndSend()
    {
        string msg = "";
        bool skipLog = true;
        string ignore_msg = "";
        //UnityEngine.Debug.Log("subject log " + Experiment.Instance.subjectLog.ToString());
        //UnityEngine.Debug.Log("my logger queue " + Experiment.Instance.subjectLog.myLoggerQueue.ToString());
        //UnityEngine.Debug.Log("messages in log queue " + Experiment.Instance.subjectLog.myLoggerQueue.logQueue.Count.ToString());
        while (Experiment.Instance.subjectLog.myLoggerQueue.logQueue.Count > 0)
        {
            skipLog = false;
            string messageToAdd = Experiment.Instance.subjectLog.myLoggerQueue.GetFromLogQueue();
            //if (Experiment.Instance.subjectLog.myLoggerQueue.CheckForMessages())
            //msg += messageToAdd + "\n";
            //UnityEngine.Debug.Log("messagetoadd " + messageToAdd);
            msg += messageToAdd+ "\n";
            yield return 0;
        }
      
#if UNITY_WEBGL && !UNITY_EDITOR
if(!skipLog)
		BrowserPlugin.WriteOutput(msg,subjectName);
	//	BrowserPlugin.SendTextFileToS3();
#else
        //if(!skipLog)
            //UnityEngine.Debug.Log("msg  " + msg);
#endif
        yield return null;
    }

    IEnumerator InitLogging()
    {
        UnityEngine.Debug.Log("beginning initLogging");
        string subjName = "subj_" + GameClock.SystemTime_MillisecondsString;

        //string subjName = _enteredSubjName;
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityEngine.Debug.Log("inside webgl part of initlogging");
		BrowserPlugin.SetSubject(subjName);
#else
        string subjectDirectory = defaultLoggingPath + subjName + "/";
        sessionDirectory = subjectDirectory + "session_0" + "/";

        _sessionID = 0;
        string sessionIDString = "_0";
        Debug.Log("about to create directory");
        if (!Directory.Exists(subjectDirectory))
        {
            Directory.CreateDirectory(subjectDirectory);
        }
        Debug.Log("does " + sessionDirectory + "and" + sessionStartedFileName + " exist");
        while (File.Exists(sessionDirectory + sessionStartedFileName))
        {
            _sessionID++;

            sessionIDString = "_" + _sessionID.ToString();

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

        subjectLog.fileName = sessionDirectory + subjName + "Log" + ".txt";
        eegLog.fileName = sessionDirectory + subjName + "EEGLog" + ".txt";

#endif
        Logger_Threading.canLog = true;
        yield return null;
    }

#endif


    //In order to increment the session, this file must be present. Otherwise, the session has not actually started.
    //This accounts for when we don't successfully connect to hardware -- wouldn't want new session folders.
    //Gets created in TrialController after any hardware has conneinitcted and the instruction video has finished playing.
    public void CreateSessionStartedFile()
    {
        StreamWriter newSR = new StreamWriter(sessionDirectory + sessionStartedFileName);
    }



    IEnumerator ConnectToElemem()
    {
#if !UNITY_WEBGL
        // ramulatorInterface.StartThread();
        yield return StartCoroutine(elememInterface.BeginNewSession());
#endif
        yield return null;
    }

    public void ParseSubjectCode()
    {
        _subjectName = uiController.subjectInputField.text;
        UnityEngine.Debug.Log("got subject name");
        _subjectInfoEntered = true;
    }



    public void SetSubjectName()
    {
#if !UNITY_WEBGL
        _subjectName = "subj_" + GameClock.SystemTime_MillisecondsString;
#endif
        _enteredSubjName = uiController.subjectInputField.text;
        _subjectName = _enteredSubjName;
        UnityEngine.Debug.Log("enteredsubj name " + _subjectName);

        if (string.IsNullOrEmpty(_subjectName))
        {
            UnityEngine.Debug.Log("NO SUBJECT NAME ENTERED");
            //   StartCoroutine(uiController.ShowSubjectWarning());
        }
        else
        {
            StartCoroutine("InitLogging");

        }

    }

    public bool IsExpActive()
    {
        return _expActive;
    }

    public void setexpAct() {
        _expActive = false;
    }

    IEnumerator PeriodicallyWrite()
    {
        while (_expActive)
        {
            //UnityEngine.Debug.Log("writing");
            yield return StartCoroutine("WriteAndSend");
            yield return new WaitForSeconds(3f);
            yield return 0;
        }
        yield return null;
    }



    public int ListenForAssignmentID(string id)
    {
        UnityEngine.Debug.Log("inside unity;listening for assignment ID " + id);
        if (!idAssigned)
        {
            if (id != "")
            {

                //assignmentID = id;
                prolific_pid = id.Split(';')[0];
                study_id = id.Split(';')[1];
                session_id = id.Split(';')[2];
                trialLogTrack.LogProlificWorkerInfo(prolific_pid, study_id, session_id);
                _canProceed = true;
                idAssigned = true;
                return 1;
            }
            else
                return 0;
        }
        else
        {
            return 1;
        }
    }

    public void ListenForMicAccess()
    {
        UnityEngine.Debug.Log("mic access granted");
        micStatus = 1;

    }

    IEnumerator InitialSetup()
    {
        //show consent and wait till they agree to it
        yield return StartCoroutine(ShowConsentScreen());

#if !UNITY_EDITOR && UNITY_WEBGL
        uiController.prolificInfoPanel.alpha = 1f;
        yield return StartCoroutine("BeginListeningForWorkerID");
        uiController.prolificInfoPanel.alpha = 0f;

        yield return StartCoroutine(CheckMicAccess());

        yield return StartCoroutine(GoFullScreen());
#endif
#if CLINICAL || CLINICAL_TEST || BEHAVIORAL
        //if this is the first session, create data for both sessions
        if (_sessionID == 0)
        {
            yield return StartCoroutine(CreateSessionData());

            //create randomized trial conditions
            yield return StartCoroutine(GenerateRandomizedTrialConditions());
        }
        //if second day session, then parse text/JSON files and gather relevant data for this session
        else
        {
            yield return StartCoroutine(GatherSessionData());
        }
#endif

        yield return null;
    }


    IEnumerator CreateSessionData()
    {
        //shuffle stimuli images into a list
        List<int> shuffledStimuliIndices = UsefulFunctions.ReturnShuffledIntegerList(objController.permanentImageList.Count); //get total stimuli images
        UnityEngine.Debug.Log("shuffled indices" + shuffledStimuliIndices.Count.ToString());


        //this is the main global variable we will store our current sessions stimuli indices into
        currentstimuliIndices = new List<int>();

        ////how many stimuli per session
        int stimuliCountPerSession = shuffledStimuliIndices.Count / Configuration.totalSessions;

        int stim = 0;
        List<int> currList = new List<int>();

        //loop for each session
        for (int j = 0; j < Configuration.totalSessions; j++)
        {
            currList.Clear();
            string fileName = sessionDirectory + "sess_" + j.ToString() + "_stimuli.txt"; // path of the file where we will store all the stimuli indices of that particular session
            for (int i = stim * stimuliCountPerSession; i < (j + 1) * stimuliCountPerSession; i++) // break the shuffled stimuli indices list into chunks for different sessions
            {
                UnityEngine.Debug.Log("writing for session ");
                currList.Add(shuffledStimuliIndices[i]);

                //add stimuli indices associated with our current session into a private variable for later access
                if (j == _sessionID)
                    currentstimuliIndices.Add(shuffledStimuliIndices[i]); 

            }
            UsefulFunctions.WriteIntoTextFile(fileName, currList); //write the entire list into the sess_<sessionnumber>_stimuli.txt file

            stim++;
        }


        //now let's convert _stimuliIndices into a temporary string arr so we can pass it as an argument to objController.CreateSessionImageList
        string[] tempArr = new string[currentstimuliIndices.Count];
        for(int i=0;i< currentstimuliIndices.Count;i++)
        {
            tempArr[i] = currentstimuliIndices[i].ToString();
        }

        //this will create a stimuliImageList variable inside objController which will be our active list of stimuli images during this session
        yield return StartCoroutine(objController.CreateSessionImageList(tempArr));


        yield return null;
    }


    //TODO: modify it to work with checkpointing; currently this only 
    IEnumerator GatherSessionData()
    {
        UnityEngine.Debug.Log("gathering session data for  " + _sessionID.ToString());
        int prevSessID = _sessionID - 1;
        if(prevSessID >=0)
        {

            //LOAD STIMULI indices for this session
            //read the stimuli text file
            string targetFilePath = Path.Combine(_subjectDirectory + "session_" + prevSessID.ToString(), "sess_" + _sessionID.ToString() + "_stimuli.txt");
            UnityEngine.Debug.Log("trying to find file at " + targetFilePath.ToString());
            if(File.Exists(targetFilePath))
            {
                string fileContents = File.ReadAllText(targetFilePath);
                string[] splitStimuliIndices = fileContents.Split('\n');
                yield return StartCoroutine(objController.CreateSessionImageList(splitStimuliIndices));
                UnityEngine.Debug.Log("read " + splitStimuliIndices.Length.ToString() + " stimuli indices");

            }
        }
        else
        {
            UnityEngine.Debug.Log("invalid session number");
        }



        //LOAD randomized conditions for this session
        string conditionsFilePath = Path.Combine(_subjectDirectory, "session_" + prevSessID.ToString(), "session" + prevSessID.ToString() + "_trialConditions.txt");
        UnityEngine.Debug.Log("trying to find file at " + conditionsFilePath.ToString());
        if (File.Exists(conditionsFilePath))
        {
            //read the JSON string
            string fileContents = File.ReadAllText(conditionsFilePath);
            //we create a TrialConditions object from the JSON string
            TrialConditions _tempTrialConditions = TrialConditions.CreateFromJSON(fileContents); 

            //instantiate lists before populating them
            _retrievalTypeList = new List<int>();
            _weatherChangeTrials = new List<int>();
            _randomizedWeatherOrder = new List<int>();

            //these list variables will be used during the rest of the session
            _retrievalTypeList = UsefulFunctions.DuplicateList(_tempTrialConditions.retrievalTypeList);
            _weatherChangeTrials = UsefulFunctions.DuplicateList(_tempTrialConditions.weatherChangeTrials);
            _randomizedWeatherOrder = UsefulFunctions.DuplicateList(_tempTrialConditions.randomizedWeatherOrder);

        }
            yield return null;
    }

    IEnumerator ShowConsentScreen()
    {
        UnityEngine.Debug.Log("showing consent screen");
        uiController.consentPanel.alpha = 1f;
        trialLogTrack.LogUIEvent("CONSENT_SCREEN", true);

        //wait until consent button is pressed
        while (!givenConsent)
        {
            yield return 0;

        }
        UnityEngine.Debug.Log("given consent");

        trialLogTrack.LogUIEvent("CONSENT_SCREEN", false);
        uiController.consentPanel.alpha = 0f;
        yield return null;
    }


    //initiated by UI button on screen during the consent panel
    public void GiveConsent()
    {
        givenConsent = true;
    }

    IEnumerator BeginListeningForWorkerID()
    {
        trialLogTrack.LogUIEvent("PROLIFIC_INFO", true);
        bool shouldListen = true;
        int timesWaited = 0;
        while (shouldListen)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
	BrowserPlugin.CheckAssignmentIDStatus();
#endif

            if (idAssigned)
            {
                shouldListen = false;
                	UnityEngine.Debug.Log("got the proper prolific PID " + prolific_pid + " study ID " + study_id + " session id " +  study_id);
            }

            yield return new WaitForSeconds(1f);
            timesWaited++;
            if (timesWaited >= 5)
            {
                uiController.failProlificPanel.alpha = 1f;
                trialLogTrack.LogProlificFailEvent();

            }
            yield return 0;
        }
        trialLogTrack.LogUIEvent("PROLIFIC_INFO", false);
        yield return null;
    }


    IEnumerator GoFullScreen()
    {

        //going full-screen
        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        Screen.fullScreen = true;


#if UNITY_WEBGL && !UNITY_EDITOR
		BrowserPlugin.GoFullScreen();
#endif


        yield return null;
    }


    IEnumerator GetSubjectInfo()
    {
        _subjectInfoEntered = false;
        uiController.subjectInfoPanel.gameObject.SetActive(true);
        uiController.subjectInfoPanel.alpha = 1f;
        while (!_subjectInfoEntered)
        {
            yield return 0;
        }
        uiController.subjectInfoPanel.alpha = 0f;
        uiController.subjectInfoPanel.gameObject.SetActive(false);
        yield return null;
    }

    IEnumerator RunPermissionsCheck()
    {
        //check writing permissions
        //check microphone and audio permissions

        yield return null;
    }


    IEnumerator BeginExperiment()
    {
        //skip this if we're in the web version

#if !UNITY_WEBGL
             yield return StartCoroutine(GetSubjectInfo());
#endif

        SetSubjectName();
  
        	UnityEngine.Debug.Log("set subject name: " + _subjectName);
        trialLogTrack.LogBegin();

        //load the layers and all the relevant data from AssetBundles

        yield return StartCoroutine(BeginLoadingTaskData());

        //only run if system2 is expected
#if !UNITY_WEBGL
        if (isElemem)
    {

         uiController.elememConnectionPanel.alpha = 1f;

         yield return StartCoroutine(ConnectToElemem());
     
        uiController.elememConnectionPanel.alpha = 0f;
    }
    else
    {
        uiController.elememConnectionPanel.alpha = 0f;
    }
    
        uiController.elememConnectionPanel.alpha = 0f;
        trialLogTrack.LogElememConnectionSuccess();

#endif

        _expActive = true;
        verbalRetrieval = false;

        yield return StartCoroutine(videoLayerManager.BeginFramePlay());
        UnityEngine.Debug.Log("Hello this is Enddsnfiewdweqknqiw");
        //initialize the weather as Sunny, by default
        UnityEngine.Debug.Log("Quitting Here!!");
        _currentWeather = new Weather(Weather.WeatherType.Sunny);

        //create session started file
        CreateSessionStartedFile();

        yield return StartCoroutine(InitialSetup());
        //only perform practice if it is the first session
        if(_sessionID==0)
            yield return StartCoroutine(BeginPractice()); //runs both weather familarization and practice
        else
        {
            //show second day intro
            yield return StartCoroutine(instructionsManager.ShowSecondSessionWelcomeInstructions());
            yield return StartCoroutine(RunWeatherFamiliarization()); //run weather familiarization sequence
            yield return StartCoroutine(instructionsManager.ShowSecondEncodingInstructions());
        }

        UnityEngine.Debug.Log("about to prep trials");
        yield return StartCoroutine(CreateWeatherPairs()); //will create WeatherPair instances that will be referred to later

        UnityEngine.Debug.Log("finished prepping trials");
        uiController.postPracticePanel.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        uiController.postPracticePanel.alpha = 0f;

        if (_sessionID == 0)
            _trialCount = -1;
        else
            _trialCount = (totalTrials / Configuration.totalSessions) - 1;

        UnityEngine.Debug.Log("starting trial count " + _trialCount.ToString());
        //repeat blocks twice
        for (int i = 0; i < _blockCount; i++)
        {
            _currBlockNum = i;
            UnityEngine.Debug.Log("HHHHHHHHHHHHHHHHHHHHHHHHH Cure_Block: " + _currBlockNum + " _blockCount: " + _blockCount);

            //only show intermission instructions if it is a behavioral pilot
#if BEHAVIORAL
            if (_currBlockNum == 3)
                yield return StartCoroutine(instructionsManager.ShowIntermissionInstructions());
#endif

            trialLogTrack.LogBlock(i, true);
            yield return StartCoroutine(BeginTaskBlock()); //logic of an entire block of trials
            trialLogTrack.LogBlock(i, false);
        }

        uiController.endSessionPanel.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForExitButton());
        _expActive = false;

        /*#if !UNITY_WEBGL
                Application.Quit();
        #else
                Application.Quit();
        #endif*/
        UnityEngine.Debug.Log("Quitting Here!!");
        Application.Quit();
        yield return null;
    }



    IEnumerator BeginLoadingTaskData()
    {
        uiController.loadingScreen.alpha = 1f;
        uiController.UpdateLoadingProgress(0f); //reset the loading progress
        yield return StartCoroutine(assetBundleLoader.LoadStimuliImages());
        uiController.UpdateLoadingProgress(20f);
        yield return StartCoroutine(videoLayerManager.SetupLayers());

      //  yield return StartCoroutine(assetBundleLoader.LoadAudio());
        yield return StartCoroutine(LoadCamTransform());


        Experiment.Instance.uiController.UpdateLoadingProgress(100f);

        uiController.loadingScreen.alpha = 0f;
        yield return null;
    }


    IEnumerator RandomizeTravelSpeed()
    {
        while (currentStage != Experiment.TaskStage.PostTaskScreening)
        {
            while (!player.GetComponent<Rigidbody>().isKinematic)
            {
                float timer = 0f;
                float maxTime = UnityEngine.Random.Range(3f, 10f);
                float speed = UnityEngine.Random.Range(30f, 60f);
                UnityEngine.Debug.Log("new max time " + maxTime.ToString());
                while (timer < maxTime)
                {
                    timer += Time.deltaTime;
                    yield return 0;
                }
                player.GetComponent<CarController>().ChangeMaxSpeed(speed);
                UnityEngine.Debug.Log("changed speed " + speed.ToString());
                timer = 0f;
                yield return 0;
            }

            yield return 0;
        }
        yield return null;
    }



    IEnumerator BeginTrackScreening(bool isWeatherFamiliarization)
    {

        uiController.SetFamiliarizationInstructions(_currentWeather.weatherMode);
        currentStage = TaskStage.TrackScreening;
        trialLogTrack.LogTaskStage(currentStage, true);
        Experiment.Instance.uiController.driveControls.alpha = 1f;
          
        yield return StartCoroutine(videoLayerManager.ResumePlayback());
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Manual);

        float timerVal = 0f;
        while(timerVal < Configuration.familiarizationMaxTime)
        {
            timerVal += Time.deltaTime;
            yield return 0;
        }
        LapCounter.lapCount = 0;


        currentStage = TaskStage.TrackScreening;
        trialLogTrack.LogTaskStage(currentStage, false);

        uiController.familiarizationOverheadInstructions.alpha = 0f;
        Experiment.Instance.uiController.driveControls.alpha = 0f;

        yield return null;
    }


    IEnumerator BeginPractice()
    {
        UnityEngine.Debug.Log("beginning practice");
        isPractice = true;
        currentStage = TaskStage.Practice;


        trialLogTrack.LogInstructions(true);
        yield return StartCoroutine(instructionsManager.ShowEncodingInstructions());
        trialLogTrack.LogInstructions(false);


        //yield return StartCoroutine(ShowPracticeInstructions(""));
        yield return StartCoroutine(RunWeatherFamiliarization());
        
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));

        //reset the weather to sunny for the next two trials

        stimuliBlockSequence = new List<GameObject>();

        yield return StartCoroutine(instructionsManager.ShowPracticeInstructions("PreEncoding"));

        _trialCount = 0;

        _currentWeather = new Weather(Weather.WeatherType.Sunny);
        ChangeLighting(_currentWeather);
        ////run encoding
            yield return StartCoroutine(RunEncoding());

        //////run retrieval
        _currentWeather = new Weather(Weather.WeatherType.Sunny);
        ChangeLighting(_currentWeather);
        verbalRetrieval = false;
        currentStage = TaskStage.SpatialRetrieval;
        trialLogTrack.LogTaskStage(currentStage, true);
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        yield return StartCoroutine(RunSpatialRetrieval());

        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        trialLogTrack.LogTaskStage(currentStage, false);

        ToggleFixation(true);
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ResetTrack());
        ToggleFixation(false);


        yield return StartCoroutine(instructionsManager.ShowPracticeInstructions("SecondPracticeLoop"));
        _trialCount++;
        _currentWeather = new Weather(Weather.WeatherType.Sunny);
        ChangeLighting(_currentWeather);
        ////run encoding
          yield return StartCoroutine(RunEncoding());


        verbalRetrieval = true;
        currentStage = TaskStage.VerbalRetrieval;
        trialLogTrack.LogTaskStage(currentStage, true);


        ////run retrieval
        _currentWeather = new Weather(Weather.WeatherType.Sunny);
        ChangeLighting(_currentWeather);
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        yield return StartCoroutine(RunVerbalRetrieval());

        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        trialLogTrack.LogTaskStage(currentStage, false);

        ToggleFixation(true);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ResetTrack());
        ToggleFixation(false);

        

        //do two more practice laps with randomized retrieval conditions

        List<int> randRetrievalOrder = UsefulFunctions.ReturnShuffledIntegerList(2);

        for (int i = 0; i < 2; i++)
        {
            _trialCount++;
            switch(i)
            {
                case 0:
                    _currentWeather = new Weather(Weather.WeatherType.Rainy);
                    ChangeLighting(_currentWeather);
                    break;
                case 1:
                    _currentWeather = new Weather(Weather.WeatherType.Night);
                    ChangeLighting(_currentWeather);
                    break;

            }

            yield return StartCoroutine(DisplayNextTrialScreen());
            yield return StartCoroutine(RunEncoding());

            int retrievalType = randRetrievalOrder[i];

            yield return StartCoroutine(videoLayerManager.PauseAllLayers());
            ChangeLighting(_currentWeather);
            switch (retrievalType)
            {
                case 0:
                    verbalRetrieval = true;
                    currentStage = TaskStage.VerbalRetrieval;
                    trialLogTrack.LogTaskStage(currentStage, true);


                    //run retrieval
                    yield return StartCoroutine(RunVerbalRetrieval());
                    break;
                case 1:
                    verbalRetrieval = false;
                    currentStage = TaskStage.SpatialRetrieval;
                    trialLogTrack.LogTaskStage(currentStage, true);
                    yield return StartCoroutine(RunSpatialRetrieval());
                    break;


            }

            player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
            yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
            trialLogTrack.LogTaskStage(currentStage, false);
            ToggleFixation(true);
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ResetTrack());
            ToggleFixation(false);
        }

        yield return StartCoroutine(RunBlockTests());

        isPractice = false;

        yield return null;
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


    Weather FindPaired_retrievalWeather(Weather pairWeather)
    {
        Weather _retrievalWeather = new Weather(Weather.WeatherType.Sunny); // create a default weather first
        int matchingWeatherIndex = 0;
        //cycle through all the weather pairs until a matching weather to our argument is found
        for (int i=0;i<_weatherPairs.Count;i++)
        {
            if(pairWeather.weatherMode == _weatherPairs[i].encodingWeather.weatherMode)
            {
                UnityEngine.Debug.Log("matching weather pair found ");
                UnityEngine.Debug.Log("CHECK WEATHER PAIR FOUND E: " + _weatherPairs[i].encodingWeather.weatherMode.ToString() + " R: " + _weatherPairs[i].retrievalWeather.weatherMode.ToString());
                _retrievalWeather = _weatherPairs[i].retrievalWeather;
                matchingWeatherIndex = i;
                i = _weatherPairs.Count; // once pair is found, we break out of the loop

                _weatherPairs.RemoveAt(matchingWeatherIndex);
            }
        }

        return _retrievalWeather;
    }

    IEnumerator GenerateLureSpots()
    {
        List<Texture> lureImageTextures =  objController.SelectImagesForLures();

        for (int i = 0; i < Configuration.luresPerTrial; i++)
        {
            int associatedLureFrame = lureFrames[i];
            Transform currentLureTransform = GetTransformForFrame(associatedLureFrame); //this finds the associated world-space transform for the argument frame 
            currentLureTransform.position += currentLureTransform.forward * 2.5f;
            GameObject lureParent = Instantiate(objController.placeholder, currentLureTransform.position,Quaternion.identity) as GameObject;


            lureParent.GetComponent<StimulusObject>().stimuliDisplayTexture = lureImageTextures[i];
            lureParent.GetComponent<StimulusObject>().stimuliDisplayName = lureImageTextures[i].name;

            lureParent.gameObject.name = lureImageTextures[i].name;
            lureObjects.Add(lureParent);


            retrievalFrameObjectDict.Add(associatedLureFrame, lureParent);

            UnityEngine.Debug.Log("generated lure object " + lureParent.GetComponent<StimulusObject>().stimuliDisplayName + associatedLureFrame.ToString());
           

        }


        yield return null;
    }


    //aka the LEARNING phase
    IEnumerator RunEncoding()
    {

        UnityEngine.Debug.Log("in encoding now");
        currentStage = TaskStage.Encoding;

        //reset encoding index; used to keep track of the order of encoding items
        encodingIndex = -1;

        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward)); //encoding will always move in forward direction
        LapCounter.lapCount = 0;
        if (isPractice)
        {
            yield return StartCoroutine(objController.SelectPracticeItems());
        }
        else
        {
            trialLogTrack.LogTaskStage(currentStage, true);
            yield return StartCoroutine(objController.SelectEncodingItems()); //selects all the stimuli items that will be shown for this particular trial

        

        }
            yield return StartCoroutine(PickEncodingLocations()); //picks frames at which the selected stimuli items will be shown

        yield return StartCoroutine(UpdateNextSpawnFrame()); //picks the next frame at which a stimuli item will be shown
            //yield return StartCoroutine(SpawnEncodingObjects()); //this will spawn all encoding objects on the track

        if (!_skipEncoding)
        {
            yield return StartCoroutine(videoLayerManager.ResumePlayback()); //resumes playback/movement
            while (LapCounter.lapCount < 1)
            {

                UnityEngine.Debug.Log("began lap number : " + LapCounter.lapCount.ToString());
                LapCounter.canStop = false;
                while (!LapCounter.canStop)
                {
                    yield return 0;
                }
                LapCounter.canStop = false;

                yield return StartCoroutine(videoLayerManager.PauseAllLayers()); //pauses playback


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
                ToggleFixation(true);

                //return the video to the start
                yield return StartCoroutine(videoLayerManager.ReturnToStart()); //returns to the starting frame

                float totalFixationTime = _fixedTime + UnityEngine.Random.Range(0.1f, 0.3f);
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

        spawnedObjects.Clear();
        //reset everything before the next block begins
        spawnFrames.Clear();
      
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

        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward)); //by default, we will always move in forward direction
        uiController.blackScreen.alpha = 1f;
        //make sure video is paused and returned to start
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        yield return StartCoroutine(videoLayerManager.ReturnToStart());

        uiController.blackScreen.alpha = 0f;

        //	objController.encodingList.Clear();
        LapCounter.lapCount = 0;
            player.transform.position = startTransform.position;

        yield return StartCoroutine(subjectLog.FlushLogFile());
            yield return null;
        }


    IEnumerator CreateWeatherPairs()
    {
        _weatherPairs = new List<WeatherPair>();
        _weatherPairs = Generate_weatherPairs();

        while (_weatherPairs.Count <= 0)
        {
            yield return 0;
        }


        yield return new WaitForSeconds(1f);


        yield return null;
    }

    IEnumerator RetrieveRandomizedTrialConditions()
    {
        yield return null;
    }

    //this will generate fresh lists of randomized retrieval order as well as weather differences
    IEnumerator GenerateRandomizedTrialConditions()
    {
        _retrievalTypeList = new List<int>();
        _weatherChangeTrials = new List<int>();
        _randomizedWeatherOrder = new List<int>();

        int trialsPerSession = totalTrials / Configuration.totalSessions;


        //we want each session to have randomly ordered but balanced list

        //create the first portion of the list representing the first session
        _retrievalTypeList  = UsefulFunctions.ReturnShuffledIntegerList(trialsPerSession); //this should equate to all trials for the session, if running BEHAVIORAL version


#if CLINICAL || CLINICAL_TEST
        //then fill up the remaining portions; each randomly ordered but balanced (meaning it has equal odd and even numbers) across all sessions
        for(int i=0;i<Configuration.totalSessions-1;i++)
        {
            List<int> tempList = new List<int>();
            tempList = UsefulFunctions.ReturnShuffledIntegerList(trialsPerSession);
            for(int j=0;j<tempList.Count;j++)
            {
                _retrievalTypeList.Add(tempList[j]);
            }
        }
#endif


        while(_retrievalTypeList.Count < trialsPerSession)
        {
            yield return 0;
        }

        UnityEngine.Debug.Log("returned shuffled retrieval type list of count " + _retrievalTypeList.Count.ToString());

        for(int i=0;i<_retrievalTypeList.Count;i++)
        {
            UnityEngine.Debug.Log("CHECK RETRIEVAL TYPE " + ((_retrievalTypeList[i] % 2 == 0) ? "SPATIAL" : "VERBAL"));
        }
        //changing weather trials will be interleaved, so an ordered list of ints will suffice
        _weatherChangeTrials = UsefulFunctions.ReturnListOfOrderedInts(trialsPerSession);

        while (_weatherChangeTrials.Count < trialsPerSession)
        {
            yield return 0;
        }

        UnityEngine.Debug.Log("returned shuffled weather change list");
        //only half the trials will have same weather
        _randomizedWeatherOrder = UsefulFunctions.ReturnShuffledIntegerList(trialsPerSession / 2);

        while (_randomizedWeatherOrder.Count < trialsPerSession / 2)
        {
            yield return 0;
        }
        UnityEngine.Debug.Log("returned shuffled weather order");


        //now instantiate a TrialCondition object -- which will then be split across different sessions

        //this will currently ONLY work for two sessions
        _trialConditions = new TrialConditions(_retrievalTypeList, _weatherChangeTrials, _randomizedWeatherOrder);


        //for clinical versions, we'll have to further split the trial conditions into two for the two sessions
#if CLINICAL || CLINICAL_TEST
        UnityEngine.Debug.Log("length of list before split " + _trialConditions.retrievalTypeList.Count.ToString());
        Tuple<TrialConditions,TrialConditions> trialConditionsBySession = UsefulFunctions.SplitTrialConditions(_trialConditions);
        TrialConditions sess1_conditions = trialConditionsBySession.Item1;
        UnityEngine.Debug.Log("length of list after split " + sess1_conditions.retrievalTypeList.Count.ToString());
        TrialConditions sess2_conditions = trialConditionsBySession.Item2;
       
        for (int i = 0; i < 2; i++)
        {
            string folder_path = Path.Combine(_subjectDirectory, "session_"+i.ToString(),"session"+i.ToString()+"_trialConditions.txt");
            UnityEngine.Debug.Log("writing at the path " + folder_path.ToString());
            System.IO.File.WriteAllText(folder_path, ((i==0) ? sess1_conditions.ToJSONString() : sess2_conditions.ToJSONString())); // write conditions into JSON formatted string in separate text files
        }
#endif
        yield return null;
    }

    //generates pairs for weathers; for example Sunny-Rainy; Sunny-Night
    List<WeatherPair> Generate_weatherPairs()
    {
        List<WeatherPair> tempPair = new List<WeatherPair>();
        int numWeathers = Configuration.ReturnWeatherTypes();
        for (int i=0;i<numWeathers; i++)
        {
            List<int> possibleWeatherCombinations = UsefulFunctions.ReturnListOfOrderedInts(numWeathers);
            possibleWeatherCombinations.RemoveAt(i); //remove self as we are only concerned with having distinct weather pairs
            //store the current index's weather type enum into a variable
            Weather.WeatherType selfType = (Weather.WeatherType)i;

            //cycle through all possible combinations
            for (int j = 0; j < possibleWeatherCombinations.Count; j++)
            {
                WeatherPair encodingPair = new WeatherPair(selfType, (Weather.WeatherType)possibleWeatherCombinations[j]);

                tempPair.Add(encodingPair);
                UnityEngine.Debug.Log("E: " + encodingPair.encodingWeather.weatherMode.ToString() + " R: " + encodingPair.retrievalWeather.weatherMode.ToString());
                
            }
        }
        int doubleList = tempPair.Count;
        for (int i = 0; i < doubleList; i++)
        {
            tempPair.Add(tempPair[i]);
        }

        //shuffle the weather pair 
        List<int> randIndices = UsefulFunctions.ReturnShuffledIntegerList(tempPair.Count);
        int maxCount = tempPair.Count;
        List<WeatherPair> resultPair = new List<WeatherPair>();
        for (int i = 0; i < maxCount; i++)
        {
            resultPair.Add(tempPair[randIndices[i]]);
        }

        return resultPair;
    }

    IEnumerator BeginTaskBlock()
    {
        //reset the block lists
        stimuliBlockSequence = new List<GameObject>();
        for (int i = 0; i < trialsPerBlock; i++)
            {


            //we will avoid showing this immediately after the practice
            if(_currBlockNum>0 || i>0)
                yield return StartCoroutine(DisplayNextTrialScreen());
                _trialCount++;
                 trialLogTrack.LogTrialLoop(_trialCount, true);
            yield return StartCoroutine(CheckForWeatherChange(TaskStage.Encoding, i));
                //run encoding
                yield return StartCoroutine(RunEncoding());


                //check to see if the weather should change between the encoding and retrieval
                yield return StartCoroutine(CheckForWeatherChange(TaskStage.Retrieval, i));

                //run retrieval
                yield return StartCoroutine(RunRetrieval(_trialCount));
            
                ToggleFixation(true);
                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine("ResetTrack");
            trialLogTrack.LogTrialLoop(_trialCount, false);
            //shows a black screen briefly for fixation
            ToggleFixation(false);

        }


            uiController.targetTextPanel.alpha = 0f;
            SetCarMovement(true);
            trialLogTrack.LogTaskStage(currentStage, false);
        //run the end of block tests
        yield return StartCoroutine(RunBlockTests());
        
        yield return null;
    }


    IEnumerator DisplayNextTrialScreen()
    {
        uiController.nextTrialPanel.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        uiController.nextTrialPanel.alpha = 0f;
        yield return null;
    }



    IEnumerator CheckForWeatherChange(TaskStage upcomingStage, int blockTrial)
    {
        //this will be interleaved; so we will only have Different Weather for every even trial
        if (_weatherChangeTrials[blockTrial] % 2 == 0) 
        {

            if(upcomingStage == TaskStage.Encoding)
            {
                UnityEngine.Debug.Log("WEATHER PATTERN DW");
                //we want to keep the weather the same as the previous trial's retrieval weather; so we check the _currentWeather and not change anything
                UnityEngine.Debug.Log("DIFF WEATHER TRIAL: " + _currentWeather.weatherMode.ToString());

                //we try to a pair with matching encoding weather and retrieve its corresponding retrieval weather, this will be changed the next time this coroutine is called during the retrieval phase
                _retrievalWeather = FindPaired_retrievalWeather(_currentWeather);

                ChangeLighting(_currentWeather);
            }

            //weather will only be changed during the retrieval phase
            else
            {
                UnityEngine.Debug.Log("changing weather for retrieval to " + _retrievalWeather.ToString());
                ChangeLighting(_retrievalWeather);
            }
        }

        //if it is an odd numbered trial, then the weather will remain same for both Encoding and Retrieval conditions
        else
        {
            UnityEngine.Debug.Log("SAME WEATHER TRIAL: " + _currentWeather.weatherMode.ToString());
            ChangeLighting(_currentWeather);
           
        }
        yield return null;
    }

    IEnumerator RunRetrieval(int sessTrial)
    {

        //retrieval time
        //SetCarMovement(false);
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());

        UnityEngine.Debug.Log("beginning retrieval");


        //reset retrieval index; used to keep track of the order of retrieval items
        retrievalIndex = 0;
     
        LapCounter.lapCount = 0; //reset lap count for retrieval 
        UnityEngine.Debug.Log("Experiment: " + _retrievalTypeList.Count);
        UnityEngine.Debug.Log("Experiment: " + " sesstrail: " + sessTrial);
        string targetNames = "";

        //check the randomly ordered list to see what the retrieval type should be
        if (_retrievalTypeList[sessTrial] % 2 == 0)
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
        _retCount = 0;

        //set drive mode to manual

        while (LapCounter.lapCount < 10 && !finishedRetrieval)
        {
            if (verbalRetrieval)
            {
                if (!_skipVerbalRetrieval)
                {
                    yield return StartCoroutine(RunVerbalRetrieval());

                }
                finishedRetrieval = true;
            }
            else
            {
                if (!_skipSpatialRetrieval)
                {
                    yield return StartCoroutine(RunSpatialRetrieval());
                    

                   
                }
            }
            currentStage = TaskStage.Feedback;
            UnityEngine.Debug.Log("finished all retrievals");


            finishedRetrieval = true;
            yield return StartCoroutine(videoLayerManager.PauseAllLayers());

            uiController.blackScreen.alpha = 1f;
            uiController.targetTextPanel.alpha = 0f;
            uiController.blackScreen.alpha = 0f;

            //set the car movement in forward direction
            yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
            yield return 0;
        }

        finishedRetrieval = false;
        yield return null;
    }

    IEnumerator RunVerbalRetrieval()
    {

        UnityEngine.Debug.Log("showing instructions for verbal retrieval");
        if (_showVerbalInstructions)
        {

            trialLogTrack.LogInstructions(true);
            yield return StartCoroutine(uiController.SetActiveInstructionPage("Verbal"));

            //wait until the instructions sequence is complete
            while (uiController.showInstructions)
            {
                yield return 0;
            }

            trialLogTrack.LogInstructions(false);
            _showVerbalInstructions = false;
        }

        UnityEngine.Debug.Log("finished showing instructions for verbal retrieval");

        uiController.blackScreen.alpha = 1f;



        yield return StartCoroutine(GenerateLureSpots());
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        //pick random start position
        yield return StartCoroutine(videoLayerManager.MoveToRandomPoint());

        //sort retrieval frames based on new starting position
        yield return StartCoroutine(Sort_retrievalFrames());

        uiController.blackScreen.alpha = 0f;




        //pick next frame
        yield return StartCoroutine(UpdateNextSpawnFrame());
        
        UnityEngine.Debug.Log("starting verbal retrieval");



        yield return StartCoroutine(videoLayerManager.ResumePlayback());
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);

        while (_retCount < _testLength)
        {
            yield return 0;
        }


        _retCount = 0;
        yield return new WaitForSeconds(1f);
        yield return null;
    }

    IEnumerator RunSpatialRetrieval()
    {
        uiController.fixationPanel.alpha = 1f;
        //SetCarMovement(false);
        player.GetComponent<CarMover>().ToggleSpatialRetrievalIndicator(true);
        yield return StartCoroutine(GenerateLureSpots()); //create lures
                                                          //trafficLightController.MakeVisible(false);


        //pick random start position
        yield return StartCoroutine(videoLayerManager.MoveToRandomPoint());
        //sort retrieval frames based on new starting position
        //yield return StartCoroutine("Sort_retrievalFrames");


        //this randomizes the order in which Item-Cued/spatial retrieval questions are asked
        yield return StartCoroutine(Randomize_retrievalFrames());


        //pick next frame
        yield return StartCoroutine(UpdateNextSpawnFrame());


        uiController.fixationPanel.alpha = 0f;

        //_carSpeed = 0f;
        _spatialFeedbackStatus.Clear();
        _spatialFeedbackStatus = new List<bool>();
        UnityEngine.Debug.Log("beginning spatial retrieval");

      

        //show instructions only during the practice
        if (_showSpatialInstructions)
        {
            trialLogTrack.LogInstructions(true);
            UnityEngine.Debug.Log("setting instructions");
            uiController.pageControls.alpha = 1f;
            yield return StartCoroutine(uiController.SetActiveInstructionPage("Spatial"));

            //wait until the instructions sequence is complete
            while(uiController.showInstructions)
            {
                yield return 0;
            }
            //   yield return StartCoroutine(ShowRetrievalInstructions());
            trialLogTrack.LogInstructions(false);
            uiController.pageControls.alpha = 0f;
            _showSpatialInstructions = false;
        }

        uiController.itemRetrievalInstructionPanel.alpha = 0f;


        uiController.driveControls.alpha = 1f;

        //mix spawned objects and lures into a combined list that will be used to test for this retrieval condition

        List<GameObject> spatialTestList = new List<GameObject>();

        for(int k=0;k<spawnedObjects.Count;k++)
        {
            spatialTestList.Add(spawnedObjects[k]);
        }
        UnityEngine.Debug.Log("spatial test list(before LURE) has " + spatialTestList.Count.ToString() + " items in it");

        for (int l = 0;l<lureObjects.Count;l++)
        {

            spatialTestList.Add(lureObjects[l]);
        }

        UnityEngine.Debug.Log("spatial test list has " + spatialTestList.Count.ToString() + " items in it");

        Experiment.Instance.uiController.markerCirclePanel.alpha = 1f;

        //shuffle the list, then order it
        var rand = new System.Random();
        var randomList = spatialTestList.OrderBy(x => rand.Next()).ToList();
        spatialTestList = randomList;

        for (int j = 0; j < _testLength; j++)
        {

            trialLogTrack.LogItemCuedReactivation(spatialTestList[j].gameObject, isLure, j);

            //ask the item cued question
            yield return StartCoroutine(ShowItemCuedReactivation(spatialTestList[j].gameObject));

            //resume playback
            yield return StartCoroutine(videoLayerManager.ResumePlayback());

            //set movement to manual
            player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Manual);

            uiController.spacebarPlaceItem.alpha = 1f;
            //wait for the player to press ActionButton to choose their location OR skip it if the player retrieved the object as "New"
            while (!Input.GetButtonDown("Action") && !_retrievedAsNew)
            {
                yield return 0;
            }

            uiController.spacebarPlaceItem.alpha = 0f;
            _retrievedAsNew = false; //reset this flag
            yield return StartCoroutine(videoLayerManager.PauseAllLayers());
            //stop car and calculate then proceed to next

            Transform currTrans = GetTransformForFrame(videoLayerManager.GetMainLayerCurrentFrameNumber());
            float dist = Vector3.Distance(spatialTestList[j].transform.position, currTrans.position);
            UnityEngine.Debug.Log("spatial feedback dist for  " + spatialTestList[j].gameObject.name + " is  " + dist.ToString());
            if (dist < 15f)
            {
                _spatialFeedbackStatus.Add(true);
            }
            else
            {
                _spatialFeedbackStatus.Add(false);
            }
            UnityEngine.Debug.Log("Hey there My friend");
            _spatialFeedbackPosition.Add(player.transform.position);
            trialLogTrack.LogRetrievalAttempt(spatialTestList[j].gameObject);

            yield return new WaitForSeconds(0.2f);
            UnityEngine.Debug.Log("Hey there My friend. I crossed it");

        }
        SetCarMovement(false);

        uiController.driveControls.alpha = 0f;
        Experiment.Instance.uiController.markerCirclePanel.alpha = 0f;
        player.GetComponent<CarMover>().ToggleSpatialRetrievalIndicator(false);
        uiController.itemRetrievalInstructionPanel.alpha = 0f;
        yield return null;
    }


    IEnumerator RunWeatherFamiliarization()
    {
        UnityEngine.Debug.Log("running weather familiarization");

        currentStage = TaskStage.WeatherFamiliarization;
        trialLogTrack.LogTaskStage(currentStage, true);

        uiController.trackScreeningPanel.alpha = 1f;
        uiController.spacebarContinue.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        uiController.spacebarContinue.alpha = 0f;
        uiController.trackScreeningPanel.alpha = 0f;


        for (int i=0;i<3;i++)
        {
            switch(i)
            {
                case 0:
                    _currentWeather = new Weather(Weather.WeatherType.Sunny);
                    break;
                case 1:
                    _currentWeather = new Weather(Weather.WeatherType.Rainy);
                    break;
                case 2:
                    _currentWeather = new Weather(Weather.WeatherType.Night);
                    break;
            }

            ChangeLighting(_currentWeather);
            UnityEngine.Debug.Log("changed weather to " +  _currentWeather.weatherMode.ToString());
            yield return StartCoroutine(BeginTrackScreening(true));
            //yield return StartCoroutine("RunEncoding");
            ToggleFixation(true);
            yield return StartCoroutine(videoLayerManager.ReturnToStart());
            yield return StartCoroutine(videoLayerManager.PauseAllLayers());

            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine("ResetTrack");
            ToggleFixation(false);
        }

        currentStage = TaskStage.WeatherFamiliarization;
        trialLogTrack.LogTaskStage(currentStage, false);
        yield return null;
    }

    IEnumerator RunBlockTests()
    {
        //pause all movement
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        uiController.blackScreen.alpha = 1f;

        trialLogTrack.LogTaskStage(TaskStage.BlockTests, true);

        uiController.pageControls.alpha = 0f;
        UnityEngine.Debug.Log("running end of block tests");
        UnityEngine.Debug.Log("stim block sequence length" + stimuliBlockSequence.Count.ToString());
        //show instructions
        uiController.followUpTestPanel.alpha = 1f;
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        uiController.followUpTestPanel.alpha = 0f;

        
        yield return StartCoroutine(GenerateBlockTestPairs());
        yield return StartCoroutine(GenerateContextRecollectionList()); //generate list from the remaining indices in the stimuliBlockSequence


        //perform each of those tests for the paired list in sequence
        var rand = new System.Random();
        int i1 = rand.Next(0, blockTestPairList.Count-1);
        int i2 = -1;
        int i3 = -1;
        int t;

        while (true)
            {
                t = rand.Next(0, blockTestPairList.Count - 1);
                if ((t != i1))
                {
                    i2 = t;
                    break;
                }
            }

        while (true) {
            t = rand.Next(0, blockTestPairList.Count-1);
            if ((t != i1) && (t!= i2))
            {
                i3 = t;
                break;
            }
        }
        bool tempBool;
        for (int i = 0; i < blockTestPairList.Count; i++)
        {
            if ((i == i1) || (i == i2) || (i == i3))
            {
                tempBool = true;
            }
            else {
                tempBool = false;
            }
            UnityEngine.Debug.Log("CountCountCount..........  " + blockTestPairList.Count+ "tempBool: " + tempBool);
            yield return StartCoroutine(RunTemporalOrderTest(blockTestPairList[i], tempBool));
            yield return StartCoroutine(RunTemporalDistanceTest(blockTestPairList[i]));
        }
        for (int i = 0; i < _contextDifferentWeatherTestList.Count; i++)
        {
            //this will be run on a randomized set of items that weren't included in the tests above
            yield return StartCoroutine(RunContextRecollectionTest(_contextDifferentWeatherTestList[i]));
        }

        
        blockTestPairList.Clear(); 
        for (int i = 0; i < stimuliBlockSequence.Count; i++)
        {
            Destroy(stimuliBlockSequence[i]);
        }
        UnityEngine.Debug.Log("finished destroying all objects from previous trials");
        stimuliBlockSequence.Clear();
        uiController.blackScreen.alpha = 0f;

        trialLogTrack.LogTaskStage(TaskStage.BlockTests, false);

      //  yield return StartCoroutine(videoLayerManager.ResumePlayback());
        yield return null;
    }

    //TODO: make this rule-based and not hard-coded
    IEnumerator GenerateBlockTestPairs()
    {
        Dictionary<int, List<GameObject>> loopDict = new Dictionary<int, List<GameObject>>();

        List<GameObject> currList = new List<GameObject>(); //temp list we will store items of each loop in
        int listIndex = 0; //to keep track of the loop number in the dictionary
        for (int i=0;i<stimuliBlockSequence.Count;i++)
        {
            currList.Add(stimuliBlockSequence[i]);
            if((i+1)%4==0)
            {
                loopDict.Add(listIndex, currList);
                currList.Clear(); //clear the list for the next loop
                listIndex++; //increment the counter
            }
        }
        //add 2 pairs encountered in the same loop
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[16], stimuliBlockSequence[19])); //loop 4
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[1], stimuliBlockSequence[4])); //loop 1



        //add 2 pairs encountered in the different loop, same weather
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[8], stimuliBlockSequence[11])); // loop 2 and 3
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[9], stimuliBlockSequence[12]));

        //add 2 pairs encountered in different loops, different weather; see the design document for more information
        blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[3], stimuliBlockSequence[6])); //loop 1 and 2
            blockTestPairList.Add(new BlockTestPair(stimuliBlockSequence[14], stimuliBlockSequence[17]));  // loop 3 and 4

    

        yield return null;
    }


    //TODO: make this rule-based and not hard-coded
    IEnumerator GenerateContextRecollectionList()
    {
            //different weather
            _contextDifferentWeatherTestList = new List<GameObject>(); //1,3,11,14
            _contextDifferentWeatherTestList.Add(stimuliBlockSequence[0]);
            _contextDifferentWeatherTestList.Add(stimuliBlockSequence[2]);
            _contextDifferentWeatherTestList.Add(stimuliBlockSequence[10]);
            _contextDifferentWeatherTestList.Add(stimuliBlockSequence[13]);



            //same weather
            _contextSameWeatherTestList = new List<GameObject>(); //6,8,16,19
            _contextSameWeatherTestList.Add(stimuliBlockSequence[5]);
            _contextSameWeatherTestList.Add(stimuliBlockSequence[7]);
            _contextSameWeatherTestList.Add(stimuliBlockSequence[15]);
            _contextSameWeatherTestList.Add(stimuliBlockSequence[18]);
       


        yield return null;
    }

    IEnumerator RunTemporalOrderTest(BlockTestPair testPair, bool shuffleT)
    {
        GameObject firstItem,secondItem;

        if (shuffleT) {
            GameObject temp;
            temp = testPair.firstItem;
            testPair.firstItem = testPair.secondItem;
            testPair.secondItem = temp;
        }
        /*if (UnityEngine.Random.value > 0.5f)
        {
            firstItem = testPair.firstItem;
            secondItem = testPair.secondItem;
        }
        else
        {
            firstItem = testPair.secondItem;
            secondItem = testPair.firstItem;


        }*/
        firstItem = testPair.firstItem;
        secondItem = testPair.secondItem;
        uiController.temporalOrderItemA.text =firstItem.gameObject.name;
            uiController.temporalOrderItemB.text = secondItem.gameObject.name;
        

        trialLogTrack.LogTemporalOrderTest(firstItem,secondItem,true);

        string selectionType = "TemporalOrder";

        yield return StartCoroutine(uiController.SetupSelectionOptions(selectionType));

        uiController.temporalOrderTestPanel.alpha = 1f;

        uiController.ToggleSelection(true);
        uiController.selectionControls.alpha = 1f;
        //wait for the options to be selected
        _canSelect = true;
        yield return StartCoroutine(WaitForSelection(selectionType));
        _canSelect = false;
        uiController.ToggleSelection(false);
        uiController.selectionControls.alpha = 0f;
        uiController.temporalOrderTestPanel.alpha = 0f;
        trialLogTrack.LogTemporalOrderTest(firstItem,secondItem, false);

        yield return null;
    }

    IEnumerator RunTemporalDistanceTest(BlockTestPair testPair)
    {

        uiController.temporalDistanceItemA.text = testPair.firstItem.gameObject.name;
        uiController.temporalDistanceItemB.text = testPair.secondItem.gameObject.name;

        trialLogTrack.LogTemporalDistanceTest(testPair, true);
        string selectionType = "TemporalDistance";

        yield return StartCoroutine(uiController.SetupSelectionOptions(selectionType));
        uiController.temporalDistanceTestPanel.alpha = 1f;


        //wait for the selection of options
        uiController.ToggleSelection(true);
        uiController.selectionControls.alpha = 1f;
        _canSelect = true;
        yield return StartCoroutine(WaitForSelection(selectionType));
        _canSelect = false;
        uiController.ToggleSelection(false);
        uiController.selectionControls.alpha = 0f;

        uiController.temporalDistanceTestPanel.alpha = 0f;
        trialLogTrack.LogTemporalDistanceTest(testPair, false);
        yield return null;
    }

    IEnumerator RunContextRecollectionTest(GameObject testGameObject)
    {
        uiController.contextRecollectionItem.text = testGameObject.name;
        uiController.contextRecollectionTestPanel.alpha = 1f;

        trialLogTrack.LogContextRecollectionTest(testGameObject, true);
        List<int> randOrder = new List<int>();

        string selectionType = "ContextRecollection";
        //wait for the selection of options
        yield return StartCoroutine(uiController.SetupSelectionOptions(selectionType));
        uiController.ToggleSelection(true);
        _canSelect = true;

        uiController.selectionControls.alpha = 1f;
        yield return StartCoroutine(WaitForSelection(selectionType));
        uiController.selectionControls.alpha = 0f;
        _canSelect = false;
        uiController.ToggleSelection(false);
        uiController.contextRecollectionTestPanel.alpha = 0f;
        trialLogTrack.LogContextRecollectionTest(testGameObject, false);
        yield return null;
    }


    IEnumerator WaitForSelection(string selectionType)
    {
        yield return StartCoroutine(UsefulFunctions.WaitForActionButton());
        trialLogTrack.LogUserChoiceSelection(uiController.GetSelectionIndex(), selectionType);
        yield return null;
    }

    IEnumerator CleanBlockSequence()
    {

        yield return null;
    }
    public Transform GetTransformForFrame(int frameNum)
    {
        Vector3 tempPos = Vector3.zero;
        Vector3 tempRot = Vector3.zero;
    

        if (frameNum < playerPositions.Count)
        {
            tempPos = playerPositions[frameNum];
        }

        if(frameNum < playerRotations.Count)
        {
            tempRot = playerRotations[frameNum];
        }

        resultObj.transform.position = tempPos;
        resultObj.transform.eulerAngles = tempRot;

        //UnityEngine.Debug.Log("final transform pos " + resultObj.transform.position.ToString());
        return resultObj.transform;
    }

    public bool CanSelectUI()
    {
        return _canSelect;
    }

    public IEnumerator ShowItemCuedReactivation(GameObject stimObject)
    {
        uiController.ResetRetrievalInstructions();
        uiController.driveControls.alpha = 0f;
        uiController.itemReactivationPanel.alpha = 1f;

        uiController.spacebarPlaceItem.alpha = 0f;
        uiController.itemReactivationText.text = stimObject.GetComponent<StimulusObject>().GetObjectName();

        string selectionType = "Item";

        yield return StartCoroutine(uiController.SetupSelectionOptions(selectionType));
        yield return new WaitForSeconds(Configuration.itemReactivationTime);
        uiController.itemReactivationDetails.alpha = 1f;
        uiController.ToggleSelection(true);
        uiController.selectionControls.alpha = 1f;
        _canSelect = true;
        yield return StartCoroutine(WaitForSelection(selectionType));

        uiController.itemReactivationDetails.alpha = 0f;
        uiController.itemReactivationPanel.alpha = 0f;
        _canSelect = false;
        uiController.selectionControls.alpha = 0f;
        uiController.ToggleSelection(false);

        //the option index 2 will correspond to "New", if that is selected, we skip the retrieval part
        if (uiController.GetSelectionIndex() == 2)
        {
            //do nothing
            _retrievedAsNew = true; // set this as retrieved as new

            UnityEngine.Debug.Log("retrieved as new");
        }
        else
        {
        yield return StartCoroutine(uiController.SetItemRetrievalInstructions(stimObject.GetComponent<StimulusObject>().GetObjectName()));
        }
        uiController.spacebarPlaceItem.alpha = 0f;
        uiController.driveControls.alpha = 1f; //reset this when the driving resumes
        yield return null;
    }


    public IEnumerator ShowLocationCuedReactivation(GameObject stimObject)
    {
        //SetCarMovement(false);
        yield return StartCoroutine(videoLayerManager.PauseAllLayers());
        uiController.ResetRetrievalInstructions();
        uiController.locationReactivationPanel.alpha = 1f;

        trialLogTrack.LogLocationCuedReactivation(stimObject, isLure, retrievalIndex);

        string selectionType = "Location";

        yield return StartCoroutine(uiController.SetupSelectionOptions(selectionType));
        yield return new WaitForSeconds(Configuration.locationReactivationTime);

        uiController.ToggleSelection(true);
        uiController.selectionControls.alpha = 1f;
        _canSelect = true;
        yield return StartCoroutine(WaitForSelection(selectionType));
        uiController.locationReactivationPanel.alpha = 0f;
        uiController.ToggleSelection(false);
        uiController.selectionControls.alpha = 0f;
        _canSelect = false;

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

        _retCount++;
        //UnityEngine.Debug.Log("finished verbal recall");
        SetCarMovement(true);
        yield return StartCoroutine(videoLayerManager.ResumePlayback());
        yield return null;
    }




    public IEnumerator StartVerbalRetrieval(GameObject objectQueried)
    {
        yield return new WaitForSeconds(1f);
        //uiController.verbalInstruction.alpha = 1f;

        string fileName = "";
        if(isPractice)
        {
            fileName = _subjectName + "_practice_" + _trialCount.ToString() + "_" + _retCount.ToString() + Configuration.audioFileExtension;
        }
        else
        {
            fileName = _subjectName + "_" + _trialCount.ToString() + "_" + _retCount.ToString() + Configuration.audioFileExtension;
        }
         
#if !UNITY_WEBGL
        audioRecorder.beepHigh.Play();
#endif



        //start recording
#if UNITY_WEBGL && !UNITY_EDITOR
			yield return StartCoroutine(Experiment.Instance.audioRec.Record(fileName, 5));
          //  if(firstAudio)
          //  {
            
		        //BrowserPlugin.GoFullScreen();
          //      firstAudio=false;
          //  }

#endif


        //start recording
#if !UNITY_WEBGL
        yield return StartCoroutine(audioRecorder.Record(sessionDirectory + "audio", fileName, recallTime));
#endif
        trialLogTrack.LogVerbalRetrievalAttempt(objectQueried, fileName);
        //play off beep
#if !UNITY_WEBGL
        audioRecorder.beepLow.Play();
#endif
        //_retCount++;
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
            

            //UnityEngine.Debug.Log("for item " + i.ToString());
            for (int j = 0; j < startableTransforms.Count; j++)
            {

                float dist = Vector3.Distance(startableTransforms[j].position, spawnedObjects[i].transform.position);
              
                if (dist < 10f)
                {
                    //UnityEngine.Debug.Log("distance " + dist.ToString());
                    //UnityEngine.Debug.Log("excluding  " + j.ToString());
                    int indexToRemove = UsefulFunctions.FindIndexOfInt(intResult,j);
                    result.RemoveAt(indexToRemove);
                    intResult.RemoveAt(indexToRemove);
                    j = startableTransforms.Count; //break out of this loop and move onto the next spawned object; we'll exclude just one transform per item
                }


            }
        }


        return result;
    }

    public IEnumerator CheckMicAccess()
    {
        uiController.micAccessPanel.alpha = 1f;
#if UNITY_WEBGL && !UNITY_EDITOR
		BrowserPlugin.CheckMicStatus();
#endif
        //hold here until we get permissions granted
        //UnityEngine.Debug.Log("waiting to be granted mic permissions");

        while(micStatus == 0)
        {
            yield return 0;
        }
        uiController.micAccessPanel.alpha = 0f;

        yield return null;
    }


    IEnumerator Sort_retrievalFrames()
    {
        int startFrame = videoLayerManager.GetMainLayerCurrentFrameNumber();
        List<int> temp = new List<int>();
        temp = DuplicateList(_sorted_retrievalFrames);
      
        List<int> result = new List<int>();
        int insertIndex = -100;
        int minDiff = 10000;
        for(int i=0;i<_sorted_retrievalFrames.Count;i++)
        {
            int currDiff = _sorted_retrievalFrames[i] - startFrame;
            //UnityEngine.Debug.Log("curr diff between " + _sorted_retrievalFrames[i].ToString() + " and " + startFrame.ToString() + " = " + currDiff.ToString());
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
            for (int i = 0; i < _sorted_retrievalFrames.Count; i++)
            {
                if (insertIndex + i < _sorted_retrievalFrames.Count)
                {
                    result.Add(_sorted_retrievalFrames[insertIndex + i]);
                    UnityEngine.Debug.Log("adding " + _sorted_retrievalFrames[insertIndex + i] + " at " + (insertIndex + i).ToString());
                }
                else
                {
                    UnityEngine.Debug.Log("adding " + _sorted_retrievalFrames[reverseIndex] + " at " + reverseIndex.ToString());
                    result.Add(_sorted_retrievalFrames[reverseIndex]);
                    reverseIndex++;
                }
            }

            //clear the _sorted_retrievalFrames and then update it
            _sorted_retrievalFrames.Clear();
            for(int i=0;i<result.Count;i++)
            {
                _sorted_retrievalFrames.Add(result[i]);
            }
        }

        yield return null;
    }


    //this randomizes the order in which Item-Cued/spatial retrieval questions are asked
    IEnumerator Randomize_retrievalFrames()
    {
        List<int> temp = new List<int>();
        
        temp = DuplicateList(_sorted_retrievalFrames);
        List<int> tempIndex = new List<int>();
        tempIndex = UsefulFunctions.ReturnShuffledIntegerList(_sorted_retrievalFrames.Count);
        for (int i = 0; i < _sorted_retrievalFrames.Count; i++)
        {
            int randindex = UnityEngine.Random.Range(0, tempIndex.Count - 1);
            temp.Add(_sorted_retrievalFrames[tempIndex[randindex]]);
            tempIndex.RemoveAt(randindex);
        }

        _sorted_retrievalFrames.Clear();
        _sorted_retrievalFrames = DuplicateList(temp);

        yield return null;
    }

     public IEnumerator UpdateNextSpawnFrame()
    {
        if (currentStage == TaskStage.Encoding)
        {
            if (_sortedSpawnFrames.Count > 0)
            {
                nextSpawnFrame = _sortedSpawnFrames[0];
                UnityEngine.Debug.Log("next spawn frame " + nextSpawnFrame.ToString());
                _sortedSpawnFrames.RemoveAt(0);
                encodingIndex++;
            }
            else
            {
                nextSpawnFrame = -10000;   //arbitrary value
            }
        }

        //this includes both stim items and lures, so we need to perform slightly different logic
        else
        {
            if (currentStage == TaskStage.VerbalRetrieval)
            {
                if (_sorted_retrievalFrames.Count > 0)
                {
                    nextSpawnFrame = _sorted_retrievalFrames[0];
                    isLure = lureBools[0];
                    lureBools.RemoveAt(0);
                    _sorted_retrievalFrames.RemoveAt(0);
                    if(!isLure)
                        retrievalIndex++; //only increment if not a lure
                }
                else
                {

                    nextSpawnFrame = -10000; //arbitrary value
                }
            }
            else
            {

                nextSpawnFrame = -10000;//arbitrary value
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
        float totalFixationTime = _fixedTime + UnityEngine.Random.Range(0.1f, 0.3f);
        uiController.fixationCross.alpha = 0f;
        yield return new WaitForSeconds(totalFixationTime);
        UnityEngine.Debug.Log("finished waiting for fixation");
        uiController.fixationPanel.alpha = 0f;

        trialLogTrack.LogFixation(false);
        yield return null;
    }

   

    //GETS CALLED FROM DEFAULTITEM.CS WHEN CHEST OPENS ON COLLISION WITH PLAYER.
    public IEnumerator WaitForTreasurePause(GameObject specialObject)
    {

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


    /*
     * When select Encoding Locations (the locations at which stimuli items will be shown), these are the rules:
     * Min distance between start and end point (startBuffer and endBuffer)
     * Minimum gap between successive stimuli (minFramesBetweenStimuli)
     * Minimum gap between lures and stimuli (minGapToLure)
     * 2 lures per trial and 5 stimuli items per trial
     */ 
    IEnumerator PickEncodingLocations()
    {
        List<int> intPicker = new List<int>();
        List<int> waypointFrames = new List<int>();
        List<int> chosenEncodingFrames = new List<int>();




        //we keep last three and first three seconds as buffer
        for (int i = Configuration.startBuffer; i < _currentMaxFrames - Configuration.endBuffer; i++)
        {
            intPicker.Add(i);
            waypointFrames.Add(i);
        }
        //temp list
        List<int> tempStorage = new List<int>();


        try
        {
            //we pick locations for encoding objects AND lure
            for (int i = 0; i < listLength; i++)
            {

                int randIndex = UnityEngine.Random.Range(0, intPicker.Count); // we won't be picking too close to beginning/end
                int randInt = intPicker[randIndex];
                int nearestIndex = 0;
                UnityEngine.Debug.Log("picked " + intPicker[randIndex].ToString());


                chosenEncodingFrames.Add(randInt);
                //setting minimum and max limits within which stimuli locations can be picked
                int minLimit = Mathf.Clamp(randIndex - Configuration.minFramesBetweenStimuli, 0, intPicker.Count - 1);
                int maxLimit = Mathf.Clamp(randIndex + Configuration.minFramesBetweenStimuli, 0, intPicker.Count - 1);

                int ind = minLimit;
                for (int j = minLimit; j < maxLimit; j++)
                {
                    if (Mathf.Abs(intPicker[ind] - randInt) < Configuration.minFramesBetweenStimuli)
                    {
                        intPicker.RemoveAt(ind);
                    }
                    else
                    {
                        ind++;
                    }

                }
                if (i < listLength)
                {
                    tempStorage.Add(randInt);

                    //add two frames in to create a min buffer between spawned items and lures
                    for (int j = 0; j < Configuration.minBufferLures; j++)
                    {
                        if (randInt + j < _currentMaxFrames)
                            tempStorage.Add(randInt + j);
                    }

                    //add the chosen encoding frame for stimuli into a list; spawnFrames will be our main list
                    spawnFrames.Add(chosenEncodingFrames[i]);
                }

            }

            //clearing for now
            intPicker.Clear();
            waypointFrames.Clear();


            //moving on to picking locations for lures

            //creating array of valid lure frames
            List<int> validLureFrames = new List<int>();
            for (int i = videoLayerManager.GetFrameRangeStart(); i < videoLayerManager.GetFrameRangeEnd(); i++)
            {

                validLureFrames.Add(i);
            }


            //TODO: Make this more efficient


            //we will now check to remove any frames at which stimuli will be spawned; we will REMOVE these frames from being considered valid for spawning lures
            //for each stimuli frame
            for (int i = 0; i < spawnFrames.Count; i++)
            {
                //traverse between +/- minGapLure to make sure enough distance is maintained on either side
                int minLureRange = spawnFrames[i] - Configuration.minGapToLure;
                int maxLureRange = spawnFrames[i] + Configuration.minGapToLure;

                minLureRange = Mathf.Clamp(minLureRange, 0, minLureRange);
                maxLureRange = Mathf.Clamp(maxLureRange, maxLureRange,videoLayerManager.GetFrameRangeEnd());
                for (int j = minLureRange; j < maxLureRange; j++)
                {

                    //remove frames from being considered for lures
                    UnityEngine.Debug.Log("about to find " + j.ToString());

                    //this is just a predicate match to find a matching value in the list to our "j" frame
                   int res = validLureFrames.FindIndex(0,validLureFrames.Count-1,
            delegate (int x)
            {
                return x == j;
            }
            );

                    UnityEngine.Debug.Log("found it at " + res.ToString());
                    if(res!=-1)
                        validLureFrames.RemoveAt(res);
              

                }
            }

            UnityEngine.Debug.Log("reduced possible options for lure spawning to " + validLureFrames.Count.ToString() + " frames");
            
            //refresh the lists; remove points with existing stim items associated with them; lures are not constrained to be at a min distance from nearest object

            //cycle through this list TWO times; since we onl
            for (int i = 0; i < 2; i++)
            {
                int randStartingLureFrame = UnityEngine.Random.Range(0, validLureFrames.Count - 1);
                int currFrame = validLureFrames[randStartingLureFrame];
                UnityEngine.Debug.Log("curr frame " + currFrame.ToString());

                    intPicker.Add(currFrame);
                    waypointFrames.Add(currFrame);
                ////we don't want lures to be too close to each other; so we clear the indices representing the min gap +/- to the chosen lure frame
                for (int j = currFrame - Configuration.minGapToLure; j < (currFrame + Configuration.minGapToLure); j++)
                {
                    int tempInt = j;
                    tempInt = Mathf.Clamp(tempInt, 0, validLureFrames.Count - 1); //to ensure validity
                    validLureFrames.Remove(tempInt);
                }
                //check immediately if i has exceeded updated bounds
                if (i >= validLureFrames.Count)
                    i = 0; //if yes, then reset the index to 0

            }

            //2 lures per trial
            for (int j = 0; j < Configuration.luresPerTrial; j++)
            {
                int randIndex = UnityEngine.Random.Range(0, validLureFrames.Count);
                UnityEngine.Debug.Log("picking at " + randIndex.ToString() + " while intpicker count is: " + intPicker.Count.ToString());
                int randInt = validLureFrames[randIndex];
                validLureFrames.RemoveAt(randIndex);
                UnityEngine.Debug.Log("lure picked at " + randInt.ToString());
                lureFrames.Add(randInt);
            }

            UnityEngine.Debug.Log("finished picking lure frames");


            //NEXT section is concerned with sorting the encoding and lure frames
            //now we sort the list in ascending order, so that the frames are in sequence and consistent with how we move during encoding (from first frame to last)
            List<int> sortedLureFrames = new List<int>();
            sortedLureFrames = DuplicateList(lureFrames);
            sortedLureFrames = SortListInAscending(sortedLureFrames);

            List<int> sortedWaypointFrames = new List<int>();
            sortedWaypointFrames = DuplicateList(chosenEncodingFrames);
            sortedWaypointFrames = SortListInAscending(sortedWaypointFrames);

            _sortedSpawnFrames = new List<int>();
            _sorted_retrievalFrames = new List<int>();

            lureBools = new List<bool>();

            List<int> tempWaypointFrames = new List<int>();
            tempWaypointFrames = DuplicateList(sortedWaypointFrames);

            for (int i = 0; i < listLength; i++)
            {
                _sortedSpawnFrames.Add(sortedWaypointFrames[0]);
                sortedWaypointFrames.RemoveAt(0);

            }

            //since it's sorted, we only concern ourself with the first index
            for (int i = 0; i < listLength + Configuration.luresPerTrial; i++)
            {
                if (sortedLureFrames.Count != 0 && tempWaypointFrames.Count != 0)
                {
                    if (sortedLureFrames[0] < tempWaypointFrames[0])
                    {
                        _sorted_retrievalFrames.Add(sortedLureFrames[0]);
                        lureBools.Add(true);
                        sortedLureFrames.RemoveAt(0);
                    }
                    else
                    {
                        _sorted_retrievalFrames.Add(tempWaypointFrames[0]);
                        lureBools.Add(false);
                        tempWaypointFrames.RemoveAt(0);
                    }
                }
                else
                {
                    if (sortedLureFrames.Count == 0)
                    {
                        _sorted_retrievalFrames.Add(tempWaypointFrames[0]);
                        lureBools.Add(false);
                        tempWaypointFrames.RemoveAt(0);
                    }
                    else
                    {
                        _sorted_retrievalFrames.Add(sortedLureFrames[0]);
                        lureBools.Add(true);
                        sortedLureFrames.RemoveAt(0);
                    }
                }
            }
            UsefulFunctions.Debug_PrintListToConsole(_sorted_retrievalFrames);
        }
        catch(NullReferenceException e)
        {
            UnityEngine.Debug.Log("Caught null exception " + e.StackTrace);
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


        float randJitterTime = UnityEngine.Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);
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

        //tell the trial controller to wait for the animation
        yield return StartCoroutine(Experiment.Instance.WaitForTreasurePause(specialObject));

        //should destroy the chest after the special object time
        //Destroy(gameObject);
    }
   



  

    public void SetCarMovement(bool shouldMove)
    {

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

        _retrievalObjList = new List<GameObject>();
        _retrievalPositions = new List<Vector3>();

        //finally add them to the retrieval lists
        for (int i = 0; i < retrievalSeqList.Count; i++)
        {
            UnityEngine.Debug.Log("adding " + spawnedObjects[retrievalSeqList[i]]);
            UnityEngine.Debug.Log("adding " + spawnFrames[retrievalSeqList[i]]);
            _retrievalObjList.Add(spawnedObjects[retrievalSeqList[i]]);
            _retrievalFrames.Add(spawnFrames[retrievalSeqList[i]]);
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
        while (LapCounter.lapCount < _maxLaps)
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
                    UnityEngine.Debug.Log("retrieval obj list outside " + _retrievalObjList.Count.ToString());
                    UnityEngine.Debug.Log(" what is " + _retrievalObjList[i].ToString());
                    string objName = _retrievalObjList[i].gameObject.name.Split('(')[0];
                    uiController.retrievalItemName.text = objName;

                    yield return StartCoroutine(UsefulFunctions.WaitForActionButton());

                    uiController.targetTextPanel.alpha = 1f;
                    uiController.retrievalItemName.text = objName;
                    SetCarMovement(false);
                    yield return new WaitForSeconds(1f);
                    while (!Input.GetButtonDown("Action") && !LapCounter.finishedLap)
                    {
                        yield return 0;
                    }
                    chosenLocations.Add(player.transform.position);
                    float difference = Vector3.Distance(_retrievalPositions[i], player.transform.position);
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

                UnityEngine.Debug.Log("_objLapper " + _objLapper.ToString() + " lapcount " + LapCounter.lapCount.ToString());
                while (_objLapper == LapCounter.lapCount)
                {
                    yield return 0;
                }
                UnityEngine.Debug.Log("_objLapper incrementing");
                _objLapper++;
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
        Transform resultTrans = null;
        if (playerPosDict.Count > 0 && playerPosDict.TryGetValue(169, out resultTrans))
        {
            UnityEngine.Debug.Log("result for frame 169: " + resultTrans.position.x.ToString() + "," + resultTrans.position.y.ToString() + "," + resultTrans.position.z.ToString());
        }

        //CHECK FOR RAPID EXIT
        if (Input.GetButtonDown("Exit"))
        {
            QuitTask();
        }
    }


    void QuitTask()
    {
        //TODO: prompt for confirmation before quitting

        Application.Quit();
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
#if UNITY_WEBGL || BEHAVIORAL
            Application.OpenURL("https://forms.gle/LRqwhAXe75bXRMZs9");
#endif
            try
            {
#if UNITY_STANDALONE_WIN
            File.Copy("C:/Users/" + System.Environment.UserName + "/AppData/LocalLow/JacobsLab/CityBlock/Player.log", sessionDirectory + "Player.log");
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX
                File.Copy("/Users/" + System.Environment.UserName + "/Library/Logs/JacobsLab/CityBlock/Player.log", sessionDirectory + "Player.log");
#endif
            }
            catch(FileNotFoundException e)
            {
                UnityEngine.Debug.Log("FILE NOT FOUND ERROR " + e.ToString());
            }

        }
    }
}
