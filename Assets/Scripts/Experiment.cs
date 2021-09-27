using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityStandardAssets.Utility;
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

    private bool canSelect = false;

    //audio clips
    public AudioClip magicWand;

    public GameObject imagePlanePrefab;

    public List<Transform> spawnableWaypoints;
    //public List<Transform> rightSpawnableWaypoints;



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
        Feedback,
        PostTaskScreening
    }

    public TaskStage currentStage = TaskStage.ItemScreening;

    public List<GameObject> spawnedObjects;
    public List<Vector3> spawnLocations;

    public List<GameObject> lureObjects;
    public List<Vector3> lureLocations;

    private List<GameObject> retrievalObjList;
    private List<Vector3> retrievalPositions;

    public List<Transform> startableTransforms;
     
    public WaypointCircuit waypointCircuit;

    public static int recallTime = 6;

    public static int blockLength = 24;

    public static int listLength = 5;

    public Transform startTransform;

    private bool wasMovingForward = false;
    private bool wasMovingReverse = false;
    private float movementTimer = 0f;

    private GameObject leftSpawnObj;
    private GameObject rightSpawnObj;


    //blackrock variables
    public static string ExpName = "T2";
    public static string BuildVersion = "0.9.9";
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

    private bool subjectInfoEntered = false;
    private bool ipAddressEntered = false;

    public IPAddress targetAddress;

    public static string defaultLoggingPath; //SET IN RESETDEFAULTLOGGINGPATH();
                                             //string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
                                             //public Text defaultLoggingPathDisplay;
                                             //public InputField loggingPathInputField;


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


    private bool isPractice = false;


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
    }
    // Use this for initialization
    void Start()
    {
        //player.GetComponent<CarController>().ChangeMaxSpeed(40f);
        spatialFeedbackStatus = new List<bool>();
        spatialFeedbackPosition = new List<Vector3>();
        StartCoroutine("BeginExperiment");
        spawnedObjects = new List<GameObject>();
        spawnLocations = new List<Vector3>();
        lureObjects = new List<GameObject>();
        lureLocations = new List<Vector3>();
        retrievalObjList = new List<GameObject>();
        retrievalPositions = new List<Vector3>();

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
        {//Directory.Exists(sessionDirectory)) {
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
    /*
	IEnumerator SpawnZones()
    {
		int prevRandom = 0;
		int waypointCount = spawnableWaypoints.Count;
		List<int> tempIntList = new List<int>();
		for (int i=0;i<waypointCount;i++)
        {
			tempIntList.Add(i);
        }

		List<Transform> chosenWaypoints = new List<Transform>();
		for(int j=0;j<2;j++)
        {
			int random = UnityEngine.Random.Range(2, tempIntList.Count-3);
			
			//here we make sure the two spawn locations are not too close to each other
			if(j ==1)
            {
				int diff = Mathf.Abs(random - prevRandom); 
				while(diff < 5)
                {
					random = UnityEngine.Random.Range(2, tempIntList.Count - 3);
					diff = Mathf.Abs(random - prevRandom);
					yield return 0;
                }
            }
			else
            {
				prevRandom = random; //set index for later comparison
            }
			UnityEngine.Debug.Log("random int " + random.ToString());

			chosenWaypoints.Add(spawnableWaypoints[random]);
			tempIntList.RemoveAt(random);
			tempIntList.RemoveAt(random-1);
			tempIntList.RemoveAt(random - 2);

			if (random + 1 < tempIntList.Count)
			{
				tempIntList.RemoveAt(random + 1);
			}

			if (random + 2 < tempIntList.Count)
			{
				tempIntList.RemoveAt(random + 2);
			}

			float yAngle = 0f;

			if(random<=7 || random>26)
            {
				yAngle = 0f;
            }
			else
            {
				yAngle = 90f;
            }

			if(j==1)
            {
				slowZoneObj = Instantiate(slowZonePrefab, chosenWaypoints[chosenWaypoints.Count - 1].position,Quaternion.Euler(new Vector3(0f,yAngle,0f))) as GameObject;
				trialLogTrack.LogSlowZoneLocation(slowZoneObj.transform.position);
            }
			else
            {
				speedZoneObj = Instantiate(speedZonePrefab, chosenWaypoints[chosenWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
				trialLogTrack.LogSpeedZoneLocation(speedZoneObj.transform.position);
			}

		}
		yield return null;
    }
	*/


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

        yield return StartCoroutine(GetSubjectInfo());

        // subjectName = "subj_" + GameClock.SystemTime_MillisecondsString;
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


        player.GetComponent<CarMover>().TurnCarEngine(true);

        //yield return StartCoroutine("BeginItemScreening");
        //	StartCoroutine("RandomizeTravelSpeed");
        //  yield return StartCoroutine("BeginTrackScreening");

        //	yield return StartCoroutine("SpawnZones");
        //repeat blocks twice
        yield return StartCoroutine("BeginTaskBlock");

        uiController.endSessionPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());

        player.GetComponent<CarMover>().TurnCarEngine(false);
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
                float maxTime = UnityEngine.Random.Range(3f, 10f);
                float speed = UnityEngine.Random.Range(30f, 60f);
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

        currentStage = TaskStage.TrackScreening;
        overheadCam.SetActive(true);
        trackFamiliarizationQuad.SetActive(true);
        playerIndicatorSphere.SetActive(true);
        trialLogTrack.LogTaskStage(currentStage, true);


        uiController.trackScreeningPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());
        uiController.trackScreeningPanel.alpha = 0f;
        player.gameObject.SetActive(true);
        trafficLightController.MakeVisible(true);
        yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        SetCarMovement(true);

        trafficLightController.MakeVisible(false);
        while (LapCounter.lapCount < 1)
        {
            yield return 0;
        }
        //  SetCarMovement(false);

        LapCounter.lapCount = 0;
        SetCarMovement(false);
        overheadCam.SetActive(false);
        trackFamiliarizationQuad.SetActive(false);
        playerIndicatorSphere.SetActive(false);
        trialLogTrack.LogTaskStage(currentStage, false);
        yield return null;
    }


    IEnumerator BeginPractice()
    {
        isPractice = true;
        currentStage = TaskStage.Practice;
        yield return StartCoroutine(ShowPracticeInstructions());

        //run encoding
        yield return StartCoroutine("RunEncoding");

        //run retrieval
        verbalRetrieval = false;
        currentStage = TaskStage.SpatialRetrieval;
        trialLogTrack.LogTaskStage(currentStage, true);
        yield return StartCoroutine("RunSpatialRetrieval");
        trialLogTrack.LogTaskStage(currentStage, false);

        ToggleFixation(true);
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine("ResetTrack");
        ToggleFixation(false);



        //run encoding
        yield return StartCoroutine("RunEncoding");

        verbalRetrieval = true;
        currentStage = TaskStage.VerbalRetrieval;
        trialLogTrack.LogTaskStage(currentStage, true);


        //run retrieval
        yield return StartCoroutine("RunVerbalRetrieval");
        trialLogTrack.LogTaskStage(currentStage, false);

        ToggleFixation(true);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine("ResetTrack");
        ToggleFixation(false);


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

    IEnumerator ShowPracticeInstructions()
    {

        uiController.practiceInstructionPanel.alpha = 1f;
        uiController.preEncodingInstructions.enabled = true;
        yield return StartCoroutine(WaitForActionButton());
        uiController.preEncodingInstructions.enabled = false;
        uiController.practiceInstructionPanel.alpha = 0f;
        yield return null;
    }

    IEnumerator ShowEncodingInstructions()
    {
        uiController.encodingPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());
        uiController.encodingPanel.alpha = 0f;
        yield return null;
    }
    IEnumerator ShowVerbalRetrievalInstructions()
    {
        uiController.verbalInstructionA.enabled = true;
        uiController.verbalInstructionB.enabled = false;
        uiController.verbalRetrievalPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());


        uiController.verbalInstructionA.enabled = false;
        uiController.verbalInstructionB.enabled = true;
        yield return StartCoroutine(WaitForActionButton());

        uiController.verbalRetrievalPanel.alpha = 0f;
        yield return null;
    }


    IEnumerator ShowRetrievalInstructions()
    {
        uiController.practiceInstructionPanel.alpha =1f;
        uiController.preSpatialRetrieval.enabled=true;

        yield return StartCoroutine(WaitForActionButton());
        uiController.preSpatialRetrieval.enabled = false;
        uiController.practiceInstructionPanel.alpha = 0f;

        string itemName = objController.ReturnStimuliDisplayText();
        uiController.itemReactivationText.text = itemName;
        uiController.itemReactivationPanel.alpha = 1f;

        yield return new WaitForSeconds(2f);


        uiController.spatialInstructionA.enabled = true;
        uiController.spatialInstructionB.enabled = false;
        uiController.retrievalPanel.alpha = 1f;
        yield return StartCoroutine(WaitForActionButton());


        uiController.itemReactivationPanel.alpha = 0f;
        uiController.spatialInstructionA.enabled = false;
        uiController.spatialInstructionB.enabled = true;

        yield return StartCoroutine(WaitForActionButton());

        uiController.itemReactivationPanel.alpha = 0f;

        uiController.retrievalPanel.alpha = 0f;
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

    IEnumerator GenerateLureSpots()
    {
        for (int i = 0; i < Configuration.luresPerTrial; i++)
        {
            GameObject lureObj = Instantiate(objController.lurePrefab, lureLocations[i], Quaternion.identity) as GameObject;

            GameObject colliderBoxRef = Instantiate(objController.itemBoxColliderPrefab, lureLocations[i], Quaternion.identity) as GameObject;
            //parent the collider box with the lure 
            colliderBoxRef.transform.parent = lureObj.transform;

            //associate the stimulus object 
            lureObj.GetComponent<StimulusObject>().LinkColliderObj(colliderBoxRef);
            lureObj.GetComponent<VisibilityToggler>().TurnVisible(false);
            lureObjects.Add(lureObj);
        }


        yield return null;
    }

    IEnumerator RunEncoding()
    {
            UnityEngine.Debug.Log("in encoding now");
        currentStage = TaskStage.Encoding;
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
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        player.GetComponent<CarMover>().ResetWaypointTarget();

        player.transform.position = startTransform.position;
            player.transform.rotation = startTransform.rotation;
            //int targetIndex = player.GetComponent<CarAIControl>().FindSubsequentWaypointIndexFromStart(player.transform); //this sets the target waypoint when you start from a random location; will ALWAYS be facing the forward direction
            //player.GetComponent<CarAIControl>().SetTarget(player.GetComponent<WaypointProgressTracker>().currentCircuit.Waypoints[targetIndex], targetIndex);

            chequeredFlag.transform.position = startTransform.position;
            chequeredFlag.transform.rotation = startTransform.rotation;

            //	player.GetComponent<CarAIControl>().ResetTargetToStart(); //reset waypoint target transform to forward facing the startTransform
            yield return StartCoroutine(PickEncodingLocations());
            yield return StartCoroutine(SpawnEncodingObjects()); //this will spawn all encoding objects on the track

        if (!skipEncoding)
        {

            while (LapCounter.lapCount < 1)
            {
                trafficLightController.MakeVisible(true);
                SetCarMovement(false);
                //set drive mode to auto
                player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
                yield return StartCoroutine(trafficLightController.StartCountdownToGreen());




                trafficLightController.MakeVisible(false);

                UnityEngine.Debug.Log("began lap number : " + LapCounter.lapCount.ToString());
                SetCarMovement(true);
                LapCounter.canStop = false;
                while (!LapCounter.canStop)
                {
                    yield return 0;
                }
                SetCarMovement(false);
                LapCounter.canStop = false;
                //can press spacebar to stop
                float forceStopTimer = 0f;
                trafficLightController.MakeVisible(true);
                yield return StartCoroutine(trafficLightController.ShowRed());

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


                trafficLightController.MakeVisible(false);

                //reset collisions for encoding objects
                for (int k = 0; k < spawnedObjects.Count; k++)
                {
                    if (spawnedObjects[k] != null)
                        spawnedObjects[k].GetComponent<StimulusObject>().ToggleCollisions(true);
                }


                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(ShowFixation());
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

            for (int k = 0; k < spawnedObjects.Count; k++)
            {
            //destroy the ItemColliderBox which is the parent
                Destroy(spawnedObjects[k].transform.parent.gameObject);
            }

            //reset everything before the next block begins
            spawnLocations.Clear();
            spawnedObjects.Clear();

        //destroy all lure objects
        for(int i=0;i<Configuration.luresPerTrial;i++)
        {
            if (lureObjects[i]!= null)
                Destroy(lureObjects[i]);
        }
        //reset lure lists as well
        lureLocations.Clear();
        lureObjects.Clear();

            //player.GetComponent<CarController>().ChangeMaxSpeed(40f);
            chequeredFlag.SetActive(true);

            //	objController.encodingList.Clear();
            LapCounter.lapCount = 0;
            player.transform.position = startTransform.position;
            yield return null;
        }

    IEnumerator BeginTaskBlock()
    {

        verbalRetrieval = false;

        //setting up car but do not move it yet
        StartCoroutine(player.GetComponent<CarMover>().MoveCar());
        SetCarMovement(false);

        //track familiarization
      //  yield return StartCoroutine("BeginTrackScreening");

        //practice
        yield return StartCoroutine("BeginPractice");
        yield return StartCoroutine("ResetTrack");

        for (int i = 0; i < blockLength; i++)
            {
                trialCount = i + 1;
                //run encoding
                yield return StartCoroutine("RunEncoding");

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
        List<Transform> validStartTransforms = GetValidStartTransforms(); //get valid waypoints that don't have an object already spawned there
        int randWaypoint = UnityEngine.Random.Range(0, validStartTransforms.Count - 1);

        Transform randStartTransform = validStartTransforms[randWaypoint];

        //disable player collider box before transporting to new location

        player.GetComponent<CarMover>().playerRigidbody.GetComponent<Rigidbody>().isKinematic = true;

        player.transform.position = randStartTransform.position;
        player.transform.rotation = randStartTransform.rotation;
        chequeredFlag.transform.position = randStartTransform.position;
        chequeredFlag.transform.rotation = randStartTransform.rotation;


        player.GetComponent<CarMover>().ResetTargetWaypoint(randStartTransform);

        trialLogTrack.LogRetrievalStartPosition(player.transform.position);

        player.GetComponent<CarMover>().playerRigidbody.GetComponent<Rigidbody>().isKinematic = false;

        player.GetComponent<CarMover>().Reset();
        //player.GetComponent<WaypointProgressTracker>().Reset();

        LapCounter.lapCount = 0; //reset lap count for retrieval 

        string targetNames = "";


        if (trialCount % 2 == 0)
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
        player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Auto);
        yield return StartCoroutine(player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));
        player.GetComponent<CarMover>().ResetWaypointTarget();
        yield return StartCoroutine(GenerateLureSpots());
        UnityEngine.Debug.Log("starting verbal retrieval");

        if (isPractice)
        {

            trialLogTrack.LogInstructions(true);
            yield return StartCoroutine(ShowVerbalRetrievalInstructions());
            trialLogTrack.LogInstructions(false);
        }

        trafficLightController.MakeVisible(true);
        yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        SetCarMovement(true);


        trafficLightController.MakeVisible(false);

        while (retCount < listLength)
        {
            yield return 0;
        }
        verbalRetrieval = false;
        SetCarMovement(false);

        trafficLightController.MakeVisible(false);
        yield return new WaitForSeconds(1f);
        yield return null;
    }

    IEnumerator RunSpatialRetrieval()
    {
        SetCarMovement(false);
        trafficLightController.MakeVisible(false);
        carSpeed = 0f;
        spatialFeedbackStatus.Clear();
        spatialFeedbackStatus = new List<bool>();
        UnityEngine.Debug.Log("beginning spatial retrieval");

      
        //disable collision of item collider box during spatial retrieval

        for (int k = 0; k < spawnedObjects.Count; k++)
        {
            if (spawnedObjects[k] != null)
                spawnedObjects[k].GetComponent<StimulusObject>().ToggleCollisions(false);
        }

        //spatial retrieval
        List<int> intPool = new List<int>();
        List<int> randIndex = new List<int>();
        for (int j = 0; j < listLength; j++)
        {
            intPool.Add(j);
        }
        for (int j = 0; j < listLength; j++)
        {
            int randInt = UnityEngine.Random.Range(0, intPool.Count - 1);
            randIndex.Add(intPool[randInt]);
            intPool.RemoveAt(randInt);
        }

        //show instructions only during the practice
        if (isPractice)
        {
            trialLogTrack.LogInstructions(true);
            yield return StartCoroutine(ShowRetrievalInstructions());
            trialLogTrack.LogInstructions(false);
        }

        chequeredFlag.SetActive(false);


        trafficLightController.MakeVisible(true);
        yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
        SetCarMovement(false);

        uiController.itemRetrievalInstructionPanel.alpha = 0f;
        trafficLightController.MakeVisible(false);

       


        for (int j = 0; j < listLength; j++)
        {
            UnityEngine.Debug.Log("retrieval num " + j.ToString());
            //targetNames = spawnedObjects[randIndex[j]].gameObject.name.Split('(')[0];
            //uiController.zRetrievalText.color = Color.white;
            //uiController.zRetrievalText.text = targetNames;
            yield return StartCoroutine(ShowItemCuedReactivation(spawnedObjects[randIndex[j]].gameObject));
            SetCarMovement(true);

            player.GetComponent<CarMover>().SetDriveMode(CarMover.DriveMode.Manual);
            //  uiController.targetTextPanel.alpha = 1f;

            //wait for the player to press X to choose their location
            while (!Input.GetKeyDown(KeyCode.X))
            {
                yield return 0;
            }

            //stop car and calculate then proceed to next
            SetCarMovement(false);


            float dist = Vector3.Distance(spawnedObjects[randIndex[j]].transform.position, player.transform.position);
            UnityEngine.Debug.Log("spatial feedback dist for  " + spawnedObjects[randIndex[j]].gameObject.name + " is  " + dist.ToString());
            if (dist < 15f)
            {
                spatialFeedbackStatus.Add(true);
            }
            else
            {
                spatialFeedbackStatus.Add(false);
            }
            spatialFeedbackPosition.Add(player.transform.position);
            trialLogTrack.LogRetrievalAttempt(spawnedObjects[randIndex[j]].gameObject, player);

            yield return new WaitForSeconds(0.2f);

        }
        SetCarMovement(false);
        uiController.itemRetrievalInstructionPanel.alpha = 0f;
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

        yield return StartCoroutine(uiController.SetItemRetrievalInstructions(stimObject.GetComponent<StimulusObject>().GetObjectName()));
        yield return null;
    }


    public IEnumerator ShowLocationCuedReactivation(GameObject stimObject)
    {
        SetCarMovement(false);
        uiController.ResetRetrievalInstructions();
        uiController.locationReactivationPanel.alpha = 1f;
        uiController.locationReactivationText.text = stimObject.GetComponent<StimulusObject>().GetObjectName();
        yield return StartCoroutine(uiController.SetupSelectionOptions("Location"));
        yield return new WaitForSeconds(Configuration.locationReactivationTime);

        uiController.ToggleSelection(true);
        canSelect = true;
        yield return StartCoroutine(WaitForSelection());
        uiController.locationReactivationPanel.alpha = 0f;
        uiController.ToggleSelection(false);
        canSelect = false;
        yield return StartCoroutine(uiController.SetLocationRetrievalInstructions());

        float randJitterTime = Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);
        yield return new WaitForSeconds(randJitterTime);
        uiController.microphoneIconImage.color = Color.green;
        UnityEngine.Debug.Log("begin verbal recall");
        yield return StartCoroutine(StartVerbalRetrieval(stimObject));
        //yield return StartCoroutine(WaitForActionButton());
        uiController.ResetRetrievalInstructions();
        UnityEngine.Debug.Log("finished verbal recall");
        SetCarMovement(true);
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
        //	WaypointCircuit currentCircuit = player.GetComponent<WaypointProgressTracker>().leftCircuit;
        for (int k = 0; k < startableTransforms.Count; k++)
        {
            result.Add(startableTransforms[k]);
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
                    result.RemoveAt(j);
                    j = startableTransforms.Count; //move onto the next spawned object; we'll exclude just one transform per item
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
        List<Vector3> waypointLocations = new List<Vector3>();
        List<Vector3> chosenEncodingLocations = new List<Vector3>();
        for (int i = 0; i < spawnableWaypoints.Count; i++)
        {
            intPicker.Add(i);
            waypointLocations.Add(spawnableWaypoints[i].position);
        }

        List<int> tempStorage = new List<int>();
        

        //we pick locations for encoding objects AND lure
        for (int i = 0; i < listLength; i++)
        {
            int randIndex = UnityEngine.Random.Range(2, intPicker.Count); // we won't be picking too close to beginning/end
            //UnityEngine.Debug.Log("rand index " + randIndex.ToString());
            /*
            while (randIndex - Configuration.minGapBetweenStimuli < 0 && randIndex + Configuration.minGapBetweenStimuli > intPicker.Count - 1)
            {
                randIndex = UnityEngine.Random.Range(0,intPicker.Count);
                yield return 0;
            }
            */

            int randInt = intPicker[randIndex];
            int nearestIndex = 0;
            UnityEngine.Debug.Log("picked " + randInt.ToString());
            

            chosenEncodingLocations.Add(waypointLocations[randInt]);
            string temp = "";
            UnityEngine.Debug.Log("intpicker length " + intPicker.Count.ToString());
            for (int j = 0; j < intPicker.Count; j++)
            {
                temp += intPicker[j].ToString() + ",";
            }
            // intPicker.RemoveAt(randIndex);
            nearestIndex = FindNearestIndex(tempStorage,randInt);
                    intPicker.RemoveAt(randIndex); //removing self

            //we only remove if the remaining adjacent indices are too close to each other in value 
            if (DetermineWaypointRemoval(randIndex, intPicker))
            {
                if (Mathf.Abs(randInt - intPicker[randIndex]) < Configuration.minGapBetweenStimuli)
                {
                    UnityEngine.Debug.Log("removing " + intPicker[randIndex]);
                    intPicker.RemoveAt(randIndex); //removing next
                }
            }

            if (DetermineWaypointRemoval(randIndex, intPicker))
            { 
                if (Mathf.Abs(randInt - intPicker[randIndex]) < Configuration.minGapBetweenStimuli)
                {
                    UnityEngine.Debug.Log("removing " + intPicker[randIndex]);
                    intPicker.RemoveAt(randIndex); //removing next
                }
            }

            if (DetermineWaypointRemoval(randIndex-1, intPicker))
            {
                if (Mathf.Abs(randInt - intPicker[randIndex - 1]) < Configuration.minGapBetweenStimuli)
                {
                    UnityEngine.Debug.Log("removing " + intPicker[randIndex - 1]);
                    intPicker.RemoveAt(randIndex - 1); // removing previous
                }
            }

            if (DetermineWaypointRemoval(randIndex-2, intPicker))
            {
                if (Mathf.Abs(randInt - intPicker[randIndex - 2]) < Configuration.minGapBetweenStimuli)
                {
                    UnityEngine.Debug.Log("removing " + intPicker[randIndex - 2]);
                    intPicker.RemoveAt(randIndex - 2); // removing previous
                }
            }

            //   }

            //  UnityEngine.Debug.Log("removing " + intPicker[randIndex]); //remove self first
            //  intPicker.RemoveAt(randIndex);

            /*

            if (Mathf.Abs(intPicker[randIndex - 1] - randInt) < Configuration.minGapBetweenStimuli)
            {
                UnityEngine.Debug.Log("removing " + intPicker[randIndex - 1]);
                intPicker.RemoveAt(randIndex - 1);
            }

            if (Mathf.Abs(intPicker[randIndex - 2] - randInt) < Configuration.minGapBetweenStimuli)
            {
                UnityEngine.Debug.Log("removing " + intPicker[randIndex - 2]);
                intPicker.RemoveAt(randIndex - 2);
            }
            /*
            UnityEngine.Debug.Log("removing " + intPicker[randIndex-1]);
            intPicker.RemoveAt(randIndex-1);
            UnityEngine.Debug.Log("removing " + intPicker[randIndex - 1]);
            intPicker.RemoveAt(randIndex - 1);
            UnityEngine.Debug.Log("removing " + intPicker[randIndex - 1]);
            intPicker.RemoveAt(randIndex - 1);
            UnityEngine.Debug.Log("removing " + intPicker[randIndex - 1]);
            intPicker.RemoveAt(randIndex - 1);
            */

            //  }

            if (i < listLength)
            {
                UnityEngine.Debug.Log("picking object at  " + randInt.ToString());
                tempStorage.Add(randInt);
                spawnLocations.Add(chosenEncodingLocations[i]);
            }

        }

        //5 lures per trial
        for (int j = 0; j < Configuration.luresPerTrial; j++)
        {
            int randIndex = UnityEngine.Random.Range(0, intPicker.Count);
            int randInt = intPicker[randIndex];
            intPicker.RemoveAt(randIndex);
            UnityEngine.Debug.Log("lure picked " + randInt.ToString());
            lureLocations.Add(waypointLocations[randInt]);
        }
        



        UnityEngine.Debug.Log("finished picking");
        yield return null;
    }

    //used to present items
    public IEnumerator PresentStimuli(GameObject stimulusObject)
    {
        //stop the car first
        player.GetComponent<CarMover>().ToggleCarMovement(false);
        //SetCarMovement(false);


        //wait until the car has stopped
        while (player.GetComponent<CarMover>().CheckIfMoving())
        {
            UnityEngine.Debug.Log("waiting for the car to stop moving");
            yield return 0;
        }

        //    string objectName = stimulusObject.GetComponent<StimulusObject>().GetObjectName();

        //move to stimuli presentation transform

        //  UnityEngine.Debug.Log("moving the item to presentation transform");
           stimulusObject.transform.position = player.GetComponent<CarMover>().presentationTransform.position;
           stimulusObject.transform.rotation = player.GetComponent<CarMover>().presentationTransform.rotation;

       // stimulusObject.GetComponent<VisibilityToggler>().TurnVisible(true);


        float randJitterTime = Random.Range(Configuration.minJitterTime, Configuration.maxJitterTime);
        float totalPresentationTime = Configuration.itemPresentationTime + randJitterTime;

        //make object visible
        if (stimulusObject.GetComponent<VisibilityToggler>() != null)
            stimulusObject.GetComponent<VisibilityToggler>().TurnVisible(true);

        yield return StartCoroutine(stimulusObject.GetComponent<DefaultItem>().RunCollision());
        UnityEngine.Debug.Log("finished running collision");

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

        //resume movement after resetting UI
        SetCarMovement(true);
        yield return null;
    }
    IEnumerator SpawnEncodingObjects()
    {
        UnityEngine.Debug.Log("number of spawn locations " + spawnLocations.Count.ToString());
        for (int i = 0; i < spawnLocations.Count; i++)
        {
            //GameObject encodingObj = Instantiate(objController.encodingList[i], new Vector3(spawnLocations[i].x, spawnLocations[i].y + 1.5f, spawnLocations[i].z), Quaternion.identity) as GameObject;
            GameObject colliderBox = Instantiate(objController.itemBoxColliderPrefab, new Vector3(spawnLocations[i].x, spawnLocations[i].y +1.5f, spawnLocations[i].z), Quaternion.identity) as GameObject;
            GameObject encodingObj = colliderBox.GetComponent<CarStopper>().stimulusObject;
            encodingObj.name = encodingObj.name + "_" + i.ToString();
            spawnedObjects.Add(encodingObj);
            trialLogTrack.LogEncodingItemSpawn(encodingObj.name.Split('(')[0], encodingObj.transform.position);

            encodingObj.GetComponent<FacePosition>().ShouldFacePlayer = true;
            encodingObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
            colliderBox.GetComponent<FacePosition>().ShouldFacePlayer = true;
            colliderBox.GetComponent<FacePosition>().TargetPositionTransform = player.transform;

            encodingObj.GetComponent<VisibilityToggler>().TurnVisible(false);


            //adjust the stimulus object's position so it appears right above the indicator
         //   encodingObj.transform.position = colliderBox.GetComponent<CarStopper>().positionIndicator.transform.position + new Vector3(0f, 1.5f, 0f);

            //parent the collider box with the encoding object
           // colliderBox.transform.parent = encodingObj.transform;

            //colliderBox.transform.localPosition = Vector3.zero;
           // colliderBox.transform.localRotation = Quaternion.identity;

            //associate the stimulus object 
            encodingObj.GetComponent<StimulusObject>().LinkColliderObj(colliderBox);



        }
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
            UnityEngine.Debug.Log("adding " + spawnLocations[retrievalSeqList[i]]);
            retrievalObjList.Add(spawnedObjects[retrievalSeqList[i]]);
            retrievalPositions.Add(spawnLocations[retrievalSeqList[i]]);
        }

        yield return null;
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
                for (int i = 0; i < blockLength; i++)
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
                spawnLocations.Clear();
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
                UnityEngine.Debug.Log("initiating spawn sequence");
                yield return StartCoroutine(objController.InitiateSpawnSequence());

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
        if ((currentStage == TaskStage.SpatialRetrieval || currentStage == TaskStage.VerbalRetrieval) && canSelect)
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
